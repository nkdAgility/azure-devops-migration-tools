using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Common;
using Microsoft.TeamFoundation.Server;
using Microsoft.TeamFoundation.Work.WebApi;
using MigrationTools._EngineV1.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.FieldMaps;
using MigrationTools.Processors;
using Newtonsoft.Json;
using Serilog.Context;
using Serilog.Events;
using static Microsoft.TeamFoundation.WorkItemTracking.Client.Node;
using ILogger = Serilog.ILogger;


namespace MigrationTools.Enrichers
{
    public enum TfsNodeStructureType
    {
        Area,
        Iteration
    }

    public struct TfsNodeStructureSettings
    {
        public string SourceProjectName;
        public string TargetProjectName;

        public Dictionary<string, bool> FoundNodes;
    }

    /// <summary>
    /// The TfsNodeStructureEnricher is used to create missing nodes in the target project. To configure it add a `TfsNodeStructureOptions` section to `CommonEnrichersConfig` in the config file. Otherwise defaults will be applied. 
    /// </summary>
    public class TfsNodeStructure : WorkItemProcessorEnricher
    {
        private readonly Dictionary<string, NodeInfo> _pathToKnownNodeMap = new Dictionary<string, NodeInfo>();
        private string[] _nodeBasePaths;
        private TfsNodeStructureOptions _Options;

        private ICommonStructureService4 _sourceCommonStructureService;

        private TfsLanguageMapOptions _sourceLanguageMaps;
        private ProjectInfo _sourceProjectInfo;

        private ILogger contextLog;

        private string _sourceProjectName;
        private NodeInfo[] _sourceRootNodes;
        private ICommonStructureService4 _targetCommonStructureService;
        private TfsLanguageMapOptions _targetLanguageMaps;
        private string _targetProjectName;
        private KeyValuePair<string, string>? _lastResortRemapRule;

        public TfsNodeStructure(IServiceProvider services, ILogger<TfsNodeStructure> logger, ITelemetryLogger telemetryLogger)
            : base(services, logger, telemetryLogger)
        {
            contextLog = Serilog.Log.ForContext<TfsNodeStructure>();
        }

        public TfsNodeStructureOptions Options
        {
            get { return _Options; }
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override void Configure(bool save = true, bool filter = true)
        {
            throw new NotImplementedException();
        }

        public override void Configure(IProcessorEnricherOptions options)
        {
            _Options = (TfsNodeStructureOptions)options;
        }

        public void ApplySettings(TfsNodeStructureSettings settings)
        {
            _sourceProjectName = settings.SourceProjectName;
            _targetProjectName = settings.TargetProjectName;
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new NotImplementedException();
        }

        public string GetNewNodeName(string sourceNodePath, TfsNodeStructureType nodeStructureType)
        {
            Log.LogDebug("NodeStructureEnricher.GetNewNodeName({sourceNodePath}, {nodeStructureType})", sourceNodePath, nodeStructureType.ToString());

            var mappers = GetMaps(nodeStructureType);
            var lastResortRule = GetLastResortRemappingRule();

            Log.LogDebug("NodeStructureEnricher.GetNewNodeName::Mappers", mappers);
            foreach (var mapper in mappers)
            {
                Log.LogDebug("NodeStructureEnricher.GetNewNodeName::Mappers::{key}", mapper.Key);
                if (Regex.IsMatch(sourceNodePath, mapper.Key, RegexOptions.IgnoreCase))
                {
                    Log.LogDebug("NodeStructureEnricher.GetNewNodeName::Mappers::{key}::Match", mapper.Key);
                    string replacement = Regex.Replace(sourceNodePath, mapper.Key, mapper.Value);
                    Log.LogDebug("NodeStructureEnricher.GetNewNodeName::Mappers::{key}::replaceWith({replace})", mapper.Key, replacement);
                    return replacement;
                } else
                {
                    Log.LogDebug("NodeStructureEnricher.GetNewNodeName::Mappers::{key}::NoMatch", mapper.Key);
                }
            }

            if (!Regex.IsMatch(sourceNodePath, lastResortRule.Key, RegexOptions.IgnoreCase))
            {
                Log.LogWarning("NodeStructureEnricher.NodePathNotAnchoredException({sourceNodePath}, {nodeStructureType})", sourceNodePath, nodeStructureType.ToString());
                throw new NodePathNotAnchoredException($"This path is not anchored in the source project name: {sourceNodePath}");
            }

            return Regex.Replace(sourceNodePath, lastResortRule.Key, lastResortRule.Value);
        }

        private KeyValuePair<string, string> GetLastResortRemappingRule()
        {
            if (_lastResortRemapRule == null)
            {
                // We need to escape different symbols:
                // - On the _search_ side, everything is taken care of by Regex.Escape()
                // - On the _replace_ side we need to escape the $ sign only
                // Note that an escaped white space ("\ ") is _not_ an issue on the search side, the engine will understand that it's meant to be a literal white space.
                var searchEscapedSourceProjectName = Regex.Escape(_sourceProjectName);
                var replaceEscapedSourceProjectName = _sourceProjectName.Replace("$", "$$");
                var replaceEscapedTargetProjectName = _targetProjectName.Replace("$", "$$");

                _lastResortRemapRule = new KeyValuePair<string, string>($"^{searchEscapedSourceProjectName}", $"{replaceEscapedTargetProjectName}");
            }

            return _lastResortRemapRule.Value;
        }

        private NodeInfo GetOrCreateNode(string nodePath, DateTime? startDate, DateTime? finishDate)
        {
            contextLog.Debug("TfsNodeStructure:GetOrCreateNode({nodePath}, {startDate}, {finishDate})", nodePath, startDate, finishDate);
            if (_pathToKnownNodeMap.TryGetValue(nodePath, out var info))
            {
                Log.LogInformation(" Node {0} already migrated, nothing to do", nodePath);
                return info;
            }

            Log.LogInformation(" Processing Node: {0}, start date: {1}, finish date: {2}", nodePath, startDate, finishDate);

            // We don't know the node yet, so try to find the closest existing ancestor for the node
            var currentAncestorPath = nodePath;
            var pathSegments = new Stack<string>();
            NodeInfo parentNode = null;
            do
            {
                var match = Regex.Match(currentAncestorPath, @"(?<parentPath>^.+)?\\(?<nodeName>[^\\]+)\\?$");
                if (match.Success)
                {
                    var nodeName = match.Groups["nodeName"].Value;

                    // Store the list of nodes to creates on the way down
                    pathSegments.Push(nodeName);

                    // Move up the path
                    currentAncestorPath = match.Groups["parentPath"].Success
                        ? match.Groups["parentPath"].Value
                        : string.Empty;
                    _pathToKnownNodeMap.TryGetValue(currentAncestorPath, out parentNode);
                    if (parentNode == null)
                    {
                        try
                        {
                            parentNode = _targetCommonStructureService.GetNodeFromPath(currentAncestorPath);
                        } catch (Exception ex)
                        {
                            Log.LogDebug("  Not Found:", currentAncestorPath);
                            Telemetry.TrackException(ex, null, null);
                            parentNode = null;
                        }
                        
                    }
                }
                else
                {
                    throw new InvalidOperationException($"This does not look like a valid area or iteration path: {currentAncestorPath}");
                }
            } while (parentNode == null && !string.IsNullOrEmpty(currentAncestorPath));

            if (parentNode == null)
            {
                Log.LogCritical($"Path {nodePath} is not anchored in the target project, it cannot be created.");
                Environment.Exit(-1);
            }

            // Now that we have a parent, we can start creating nodes down the tree from that point
            do
            {
                var currentNodeName = pathSegments.Pop();
                var currentNodePath = $@"{parentNode.Path}\{currentNodeName}";
                try
                {
                    parentNode = _targetCommonStructureService.GetNodeFromPath(currentNodePath);
                    _pathToKnownNodeMap[parentNode.Path] = parentNode;
                    Log.LogDebug("  Node {node} already exists", currentNodePath);
                    Log.LogTrace("{node}", parentNode);
                }
                catch (CommonStructureSubsystemException ex)
                {
                    try
                    {
                        var newPathUri = _targetCommonStructureService.CreateNode(currentNodeName, parentNode.Uri);
                        Log.LogDebug("  Node {newPathUri} has been created", newPathUri);
                        parentNode = _targetCommonStructureService.GetNode(newPathUri);
                        _pathToKnownNodeMap[parentNode.Path] = parentNode;
                    }
                    catch
                    {
                        Log.LogError(ex, "Creating Node");
                        throw;
                    }
                }

                if (startDate != null && finishDate != null)
                {
                    try
                    {
                        _targetCommonStructureService.SetIterationDates(parentNode.Uri, startDate, finishDate);
                        Log.LogDebug("  Node {node} has been assigned {startDate} / {finishDate}", currentNodePath,
                            startDate, finishDate);
                    }
                    catch (CommonStructureSubsystemException ex)
                    {
                        Log.LogWarning(ex, " Unable to set {node}dates of {startDate} / {finishDate}", currentNodePath,
                            startDate, finishDate);
                    }
                }
            } while (!string.Equals(parentNode.Path, nodePath, StringComparison.InvariantCultureIgnoreCase));

            return parentNode;
        }

        private Dictionary<string, string> GetMaps(TfsNodeStructureType nodeStructureType)
        {
            switch (nodeStructureType)
            {
                case TfsNodeStructureType.Area:
                    return _Options.AreaMaps;
                case TfsNodeStructureType.Iteration:
                    return _Options.IterationMaps;
                default:
                    throw new ArgumentOutOfRangeException(nameof(nodeStructureType), nodeStructureType, null);
            }
        }

        public override void ProcessorExecutionBegin(IProcessor processor)
        {
            if (Options.Enabled)
            {
                Log.LogInformation("Migrating all Nodes before the Processor run.");
                EntryForProcessorType(processor);
                if (Options.ReplicateAllExistingNodes)
                {
                    MigrateAllNodeStructures();
                }
                RefreshForProcessorType(processor);
            }
        }

        protected override void EntryForProcessorType(IProcessor processor)
        {
            if (processor is null)
            {
                IMigrationEngine engine = Services.GetRequiredService<IMigrationEngine>();
                if (_sourceCommonStructureService is null)
                {
                    _sourceCommonStructureService = (ICommonStructureService4)engine.Source.GetService<ICommonStructureService4>();
                    _sourceProjectInfo = _sourceCommonStructureService.GetProjectFromName(engine.Source.Config.AsTeamProjectConfig().Project);
                    _sourceRootNodes = _sourceCommonStructureService.ListStructures(_sourceProjectInfo.Uri);
                    _sourceLanguageMaps = engine.Source.Config.AsTeamProjectConfig().LanguageMaps;
                    _sourceProjectName = engine.Source.Config.AsTeamProjectConfig().Project;
                }
                if (_targetCommonStructureService is null)
                {
                    _targetCommonStructureService = (ICommonStructureService4)engine.Target.GetService<ICommonStructureService4>();
                    _targetLanguageMaps = engine.Target.Config.AsTeamProjectConfig().LanguageMaps;
                    _targetProjectName = engine.Target.Config.AsTeamProjectConfig().Project;
                }
            }
            else
            {
                if (_sourceCommonStructureService is null)
                {
                    var source = (TfsWorkItemEndpoint)processor.Source;
                    _sourceCommonStructureService = (ICommonStructureService4)source.TfsCollection.GetService<ICommonStructureService>();
                    _sourceProjectInfo = _sourceCommonStructureService.GetProjectFromName(source.Project);
                    _sourceRootNodes = _sourceCommonStructureService.ListStructures(_sourceProjectInfo.Uri);
                    _sourceLanguageMaps = source.Options.LanguageMaps;
                    _sourceProjectName = source.Project;
                }
                if (_targetCommonStructureService is null)
                {
                    var target = (TfsWorkItemEndpoint)processor.Target;
                    _targetCommonStructureService = (ICommonStructureService4)target.TfsCollection.GetService<ICommonStructureService4>();
                    _targetLanguageMaps = target.Options.LanguageMaps;
                    _targetProjectName = target.Project;
                }
            }
        }

        protected override void RefreshForProcessorType(IProcessor processor)
        {
            if (processor is null)
            {
                IMigrationEngine engine = Services.GetRequiredService<IMigrationEngine>();
                ((TfsWorkItemMigrationClient)engine.Target.WorkItems).Store?.RefreshCache(true);
            }
            else
            {
                TfsEndpoint target = (TfsEndpoint)processor.Target;
                target.TfsStore.RefreshCache(true);
            }
        }

        private void CreateNodes(XmlNodeList nodeList, string treeType, TfsNodeStructureType nodeStructureType)
        {
            foreach (var item in nodeList.OfType<XmlElement>())
            {
                // We work on the system paths, but user-friendly paths are used in maps
                var userFriendlyPath = GetUserFriendlyPath(item.Attributes["Path"].Value);

                var shouldCreateNode = ShouldCreateNode(userFriendlyPath);
                var isParentOfSelectedBasePath = CheckIsParentOfSelectedBasePath(userFriendlyPath);
                if (!shouldCreateNode && !isParentOfSelectedBasePath)
                {
                    // It is not a selected path or a descendant, and it cannot be the parent of a selected path, so we can skip it.
                    continue;
                }

                if (shouldCreateNode)
                {
                    DateTime? startDate = null;
                    DateTime? finishDate = null;
                    if (treeType == "Iteration")
                    {
                        if (item.Attributes["StartDate"] != null)
                        {
                            startDate = DateTime.Parse(item.Attributes["StartDate"].Value);
                        }
                        if (item.Attributes["FinishDate"] != null)
                        {
                            finishDate = DateTime.Parse(item.Attributes["FinishDate"].Value);
                        }
                    }

                    var newUserPath = GetNewNodeName(userFriendlyPath, nodeStructureType);
                    var newSystemPath = GetSystemPath(newUserPath, nodeStructureType, _targetLanguageMaps);

                    var targetNode = GetOrCreateNode(newSystemPath, startDate, finishDate);
                    _pathToKnownNodeMap[targetNode.Path] = targetNode;
                }

                if (item.HasChildNodes)
                {
                    // Again, ...Parent/Children/ActualChildNode... in the XML, so we skip Children
                    CreateNodes(item.ChildNodes[0].ChildNodes, treeType, nodeStructureType);
                }
            }
        }

        private string GetSystemPath(string newUserPath, TfsNodeStructureType structureType, TfsLanguageMapOptions languageMap)
        {

            string matchtext = @"^(?<projectName>[^\\]+)(\\(?<restOfThePath>.*))?$"; //^(?<projectName>[^\\]+)\\(?<restOfThePath>.*)$
            var match = Regex.Match(newUserPath, matchtext);
            if (!match.Success)
            {
                throw new InvalidOperationException($"This path is not a valid area or iteration path: {newUserPath}");
            }

            var structureName = GetLocalizedNodeStructureTypeName(structureType, languageMap);

            var systemPath = $"\\{match.Groups["projectName"].Value}\\{structureName}";
            if (match.Groups["restOfThePath"].Success)
            {
                systemPath  +=  $"\\{match.Groups["restOfThePath"]}";
            }
            return systemPath;
        }

        private static string GetUserFriendlyPath(string systemNodePath)
        {
            // Shape of the path is \SourceProject\StructureType\Rest\Of\The\Path, user-friendly shape skips StructureType and initial \
            var match = Regex.Match(systemNodePath, @"^\\(?<sourceProject>[^\\]+)\\[^\\]+\\(?<restOfThePath>.*)$");
            if (!match.Success)
            {
                throw new InvalidOperationException($"This path is not a valid area or iteration path: {systemNodePath}");
            }

            return $"{match.Groups["sourceProject"].Value}\\{match.Groups["restOfThePath"].Value}";
        }

        private void MigrateAllNodeStructures()
        {
            _nodeBasePaths = Options.NodeBasePaths;

            Log.LogDebug("NodeStructureEnricher.MigrateAllNodeStructures({nodeBasePaths}, {areaMaps}, {iterationMaps})", _nodeBasePaths, _Options.AreaMaps, _Options.IterationMaps);
            //////////////////////////////////////////////////
            ProcessCommonStructure(_sourceLanguageMaps.AreaPath, _targetLanguageMaps.AreaPath, _targetProjectName, TfsNodeStructureType.Area);
            //////////////////////////////////////////////////
            ProcessCommonStructure(_sourceLanguageMaps.IterationPath, _targetLanguageMaps.IterationPath, _targetProjectName, TfsNodeStructureType.Iteration);
            //////////////////////////////////////////////////
        }

        private string GetLocalizedNodeStructureTypeName(TfsNodeStructureType value, TfsLanguageMapOptions languageMap)
        {
            if (languageMap.AreaPath.IsNullOrEmpty() || languageMap.IterationPath.IsNullOrEmpty())
            {
                contextLog.Warning("TfsNodeStructure::GetLocalizedNodeStructureTypeName - Language map is empty for either Area or Iteration!");
                contextLog.Verbose("languageMap: {@languageMap}", languageMap);
            }
            switch (value)
            {
                case TfsNodeStructureType.Area:
                    return languageMap.AreaPath.IsNullOrEmpty() ? "Area" : languageMap.AreaPath; // a ? "if_true" : "if_false";

                case TfsNodeStructureType.Iteration:
                    return languageMap.IterationPath.IsNullOrEmpty() ? "Iteration" : languageMap.IterationPath;

                default:
                    throw new InvalidOperationException("Not a valid NodeStructureType ");
            }
        }

        private void ProcessCommonStructure(string treeTypeSource, string localizedTreeTypeName, string projectTarget, TfsNodeStructureType nodeStructureType)
        {
            Log.LogDebug("NodeStructureEnricher.ProcessCommonStructure({treeTypeSource}, {treeTypeTarget})", treeTypeSource, localizedTreeTypeName);

            var startPath = ("\\" + this._sourceProjectName + "\\" + treeTypeSource).ToLower();
            Log.LogDebug("Source Node Path StartsWith [{startPath}]", startPath);

            // (i.e. "\CoolProject\Area" )
            var nodes = _sourceRootNodes.Where(n => n.Path.ToLower().StartsWith(startPath));
            if (nodes.Count() > 1)
            {
                Exception ex = new Exception(string.Format("Unable to load Common Structure for Source because more than one node path matches \"{0}\". {1}", treeTypeSource, JsonConvert.SerializeObject(nodes.Select(x => x.Path))));
                Log.LogError(ex, "Unable to load Common Structure for Source.");
                throw ex;
            }
            NodeInfo sourceNode = nodes.FirstOrDefault();
            if (sourceNode == null) // May run into language problems!!! This is to try and detect that
            {
                Exception ex = new Exception(string.Format("Unable to load Common Structure for Source. This is usually due to different language versions. Validate that '{0}' is the correct name in your version. ", treeTypeSource));
                Log.LogError(ex, "Unable to load Common Structure for Source.");
                throw ex;
            }
            XmlElement sourceTree = _sourceCommonStructureService.GetNodesXml(new string[] { sourceNode.Uri }, true);
            NodeInfo structureParent;
            try // May run into language problems!!! This is to try and detect that
            {
                structureParent = _targetCommonStructureService.GetNodeFromPath(string.Format("\\{0}\\{1}", projectTarget, localizedTreeTypeName));
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception(string.Format("Unable to load Common Structure for Target.This is usually due to different language versions. Validate that '{0}' is the correct name in your version. ", localizedTreeTypeName), ex);
                Log.LogError(ex2, "Unable to load Common Structure for Target.");
                Telemetry.TrackException(ex2, null, null);
                throw ex2;
            }

            _pathToKnownNodeMap[structureParent.Path] = structureParent;

            if (sourceTree.ChildNodes[0].HasChildNodes)
            {
                // The XPath would look like this: /Nodes/Node[Name=Area]/Children/...
                // The Path attributes however look like that for the children of the Area node: /SourceProject/Area/SourceArea
                CreateNodes(sourceTree.ChildNodes[0].ChildNodes[0].ChildNodes, localizedTreeTypeName, nodeStructureType);
            }
        }

        /// <summary>
        /// Checks node-to-be-created with allowed BasePath's
        /// </summary>
        /// <param name="userFriendlyPath">The user-friendly path of the source node</param>
        /// <returns>true/false</returns>
        private bool ShouldCreateNode(string userFriendlyPath)
        {
            if (_nodeBasePaths == null || _nodeBasePaths.Length == 0)
            {
                return true;
            }

            var invertedPath = "!" + userFriendlyPath;
            var exclusionPatterns = _nodeBasePaths.Where(oneBasePath => oneBasePath.StartsWith("!", StringComparison.InvariantCulture));
            if (_nodeBasePaths.Any(oneBasePath => userFriendlyPath.StartsWith(oneBasePath)) &&
                !exclusionPatterns.Any(oneBasePath => invertedPath.StartsWith(oneBasePath)))
            {
                return true;
            }

            Log.LogWarning("The node {nodePath} is being excluded due to your basePath setting. ", userFriendlyPath);
            return false;
        }

        /// <summary>
        /// Checks whether a path is a parent of a selected base path (meaning we cannot skip it entirely)
        /// </summary>
        /// <param name="userFriendlyPath">The user-friendly path of the source node</param>
        /// <returns>A boolean indicating whether the path is a parent of any positively selected base path.</returns>
        /// <exception cref="NotImplementedException"></exception>
        private bool CheckIsParentOfSelectedBasePath(string userFriendlyPath)
        {
            return _nodeBasePaths != null ? _nodeBasePaths.Where(onePath => !onePath.StartsWith("!"))
                                 .Any(onePath => onePath.StartsWith(userFriendlyPath)) : false;
        }

        public string GetFieldNameFromTfsNodeStructureType(TfsNodeStructureType nodeType)
        {
            string fieldName = "";
            switch (nodeType)
            {
                case TfsNodeStructureType.Iteration:
                    fieldName = "System.IterationPath";
                    break;
                case TfsNodeStructureType.Area:
                    fieldName = "System.AreaPath";
                    break;

            }
            return fieldName;
        }

        public List<NodeStructureItem> CheckForMissingPaths(List<WorkItemData> workItems, TfsNodeStructureType nodeType)
        {
            EntryForProcessorType(null);
            contextLog.Debug("TfsNodeStructure:CheckForMissingPaths");
            _targetCommonStructureService.ClearProjectInfoCache();

            string fieldName = GetFieldNameFromTfsNodeStructureType(nodeType);

            List<NodeStructureItem> nodePaths = workItems.SelectMany(x => x.Revisions.Values)
                //.Where(x => x.Fields[fieldName].Value.ToString().Contains("\\"))
                .Select(x => new NodeStructureItem() { sourcePath = x.Fields[fieldName].Value.ToString(), nodeType = nodeType.ToString() })
                .Distinct()
                .ToList();

            contextLog.Debug("TfsNodeStructure:CheckForMissingPaths::{nodeType}Nodes::{count}", nodeType.ToString(), nodePaths.Count);

            List<NodeStructureItem> missingPaths = new List<NodeStructureItem>();

            foreach (var missingItem in nodePaths)
            {
                contextLog.Debug("TfsNodeStructure:CheckForMissingPaths:Checking::{sourceSystemPath}", missingItem.sourceSystemPath);
                contextLog.Verbose("TfsNodeStructure:CheckForMissingPaths:Checking::{@missingItem}", missingItem);
                bool keepProcessing = true;
                try
                {
                    missingItem.targetPath = GetNewNodeName(missingItem.sourcePath, nodeType);
                    contextLog.Verbose("TfsNodeStructure:CheckForMissingPaths:GetNewNodeName::{@missingItem}", missingItem);
                }
                catch (NodePathNotAnchoredException ex) {
                    contextLog.Debug("TfsNodeStructure:CheckForMissingPaths:NodePathNotAnchoredException::{sourceSystemPath}", missingItem.sourceSystemPath);
                    contextLog.Verbose("TfsNodeStructure:CheckForMissingPaths:NodePathNotAnchoredException::{@missingItem}", missingItem);
                    missingItem.anchored = false;
                    List<int> workItemsNotAncored = workItems.SelectMany(x => x.Revisions.Values)
                        .Where(x => x.Fields[fieldName].Value.ToString().Contains(missingItem.sourcePath))
                        .Select(x => x.WorkItemId)
                        .Distinct()
                        .ToList();
                    missingItem.workItems = workItemsNotAncored;
                    keepProcessing = false;
                    missingPaths.Add(missingItem);
                }
                if (keepProcessing)
                {
                    missingItem.targetSystemPath = GetSystemPath(missingItem.targetPath, nodeType, _targetLanguageMaps);
                    missingItem.sourceSystemPath = GetSystemPath(missingItem.sourcePath, nodeType, _sourceLanguageMaps);
                    PopulateIterationDatesFronSource(missingItem);
                    try
                    {
                        contextLog.Debug("TfsNodeStructure:CheckForMissingPaths:CheckTarget::{targetSystemPath}", missingItem.targetSystemPath);
                        NodeInfo c = _targetCommonStructureService.GetNodeFromPath(missingItem.targetSystemPath);
                        contextLog.Verbose("TfsNodeStructure:CheckForMissingPaths:CheckTarget::FOUND::{@missingItem}::FOUND", missingItem);
                    }
                    catch
                    {
                        contextLog.Debug("TfsNodeStructure:CheckForMissingPaths:CheckTarget::NOTFOUND:{targetSystemPath}", missingItem.targetSystemPath);
                        if (_Options.ShouldCreateMissingRevisionPaths && ShouldCreateNode(missingItem.targetSystemPath))
                        {
                           
                            GetOrCreateNode(missingItem.targetSystemPath, missingItem.startDate, missingItem.finishDate);
                        }
                        else
                        {
                            missingPaths.Add(missingItem);
                            contextLog.Verbose("TfsNodeStructure:CheckForMissingPaths:CheckTarget::LOG-ONLY::{@missingItem}", missingItem);
                        }
                    }
                }
            }
            if(_Options.ShouldCreateMissingRevisionPaths )
            {
                _targetCommonStructureService.ClearProjectInfoCache();
            }
            return missingPaths;
        }

        private void PopulateIterationDatesFronSource(NodeStructureItem missingItem)
        {
            if (missingItem.nodeType == "Iteration")
            {
                contextLog.Debug("TfsNodeStructure:PopulateIterationDatesFronSource:{sourceSystemPath}", missingItem.sourceSystemPath);
                try
                {
                    var sourceNode = _sourceCommonStructureService.GetNodeFromPath(missingItem.sourceSystemPath);
                    missingItem.startDate = sourceNode.StartDate;
                    missingItem.finishDate = sourceNode.FinishDate;
                    missingItem.sourcePathExists = true;
                }
                catch (Exception)
                {
                    contextLog.Verbose("TfsNodeStructure:PopulateIterationDatesFronSource:{@missingItem}", missingItem);
                    missingItem.startDate = null;
                    missingItem.finishDate = null;
                    missingItem.sourcePathExists = false;
                }
            }
        }

        public List<NodeStructureItem> GetMissingRevisionNodes(List<WorkItemData> workItems)
        {
            List<NodeStructureItem> missingPaths = CheckForMissingPaths(workItems, TfsNodeStructureType.Area);
            missingPaths.AddRange(CheckForMissingPaths(workItems, TfsNodeStructureType.Iteration));
            return missingPaths;
        }

        public List<int> GetWorkItemIDsFromMissingRevisionNodes(List<NodeStructureItem> missingItems)
        {
            List<int> workItemsNotAncored = missingItems
                .Where(x => x.anchored = false)
                .SelectMany(x => x.workItems)
                .Distinct()
                .ToList();
            return workItemsNotAncored;
        }


        public bool ValidateTargetNodesExist(List<NodeStructureItem> missingItems)
        {
            if (missingItems.Count > 0)
            {
                contextLog.Warning("!! There are MISSING Area or Iteration Paths");
                contextLog.Warning("NOTE: It is NOT possible to migrate a work item if the Area or Iteration path does not exist on the target project. This is because the work item will be created with the same Area and Iteration path as the source work item with the project name swapped. The work item will not be created if the path does not exist. The only way to resolve this is to follow the instructions:");
                contextLog.Warning("!! There are {missingAreaPaths} Nodes (Area or Iteration) found in the history of the Source that are missing from the Target! These MUST be added or mapped before we can continue using the instructions on https://nkdagility.com/learn/azure-devops-migration-tools//Reference/v2/ProcessorEnrichers/TfsNodeStructure/#iteration-maps-and-area-maps", missingItems.Count);
                foreach (NodeStructureItem missingItem in missingItems)
                {
                    string mapper = GetMappingForMissingItem(missingItem);
                    bool isMapped = mapper.IsNullOrEmpty()?false:true;
                   string workItemList = "n/a";
                    if (missingItem.workItems != null)
                    {
                        workItemList = string.Join(",", missingItem.workItems);
                    }
                    if (isMapped)
                    {
                        contextLog.Warning("MAPPED {nodeType}: sourcePath={sourcePath}, mapper={mapper}", missingItem.nodeType, missingItem.sourcePath, mapper);
                    } else { 
                        contextLog.Warning("MISSING {nodeType}: sourcePath={sourcePath}, targetPath={targetPath}, anchored={anchored}, IDs={workItems}", missingItem.nodeType, missingItem.sourcePath, missingItem.targetPath, missingItem.anchored, workItemList);
                    }
                }

                return true;
            }
            return false;
        }

        public string GetMappingForMissingItem(NodeStructureItem missingItem)
        {
            var mappers = GetMaps((TfsNodeStructureType)Enum.Parse(typeof(TfsNodeStructureType), missingItem.nodeType, true));
            foreach (var mapper in mappers)
            {
                if (Regex.IsMatch(missingItem.sourcePath, mapper.Key, RegexOptions.IgnoreCase))
                {
                    return mapper.Key;
                }
            }
            return null;
        }

        private const string RegexPatternForAreaAndIterationPathsFix = "\\[?(?<key>System.AreaPath|System.IterationPath)+\\]?[^']*'(?<value>[^']*(?:''.[^']*)*)'";

        public string FixAreaPathAndIterationPathForTargetQuery(string sourceWIQLQuery, string sourceProject, string targetProject, ILogger? contextLog)
        {

            string targetWIQLQuery = sourceWIQLQuery;

            if (string.IsNullOrWhiteSpace(targetWIQLQuery))
            {
                return targetWIQLQuery;
            }

            var matches = Regex.Matches(targetWIQLQuery, RegexPatternForAreaAndIterationPathsFix);


            if (string.IsNullOrWhiteSpace(sourceProject)
                || string.IsNullOrWhiteSpace(targetProject)
                || sourceProject == targetProject)
            {
                return targetWIQLQuery;
            }

            foreach (Match match in matches)
            {
                var value = match.Groups["value"].Value;
                if (string.IsNullOrWhiteSpace(value) || !value.StartsWith(sourceProject))
                    continue;

                var fieldType = match.Groups["key"].Value;
                TfsNodeStructureType structureType;
                switch (fieldType)
                {
                    case "System.AreaPath":
                        structureType = TfsNodeStructureType.Area;
                        break;
                    case "System.IterationPath":
                        structureType = TfsNodeStructureType.Iteration;
                        break;
                    default:
                        throw new InvalidOperationException($"Field type {fieldType} is not supported for query remapping.");
                }

                var remappedPath = GetNewNodeName(value, structureType);
                targetWIQLQuery = targetWIQLQuery.Replace(value, remappedPath);
            }

            contextLog?.Information("[FilterWorkItemsThatAlreadyExistInTarget] is enabled. Source project {sourceProject} is replaced with target project {targetProject} on the WIQLQuery which resulted into this target WIQLQueryBit \"{targetWIQLQueryBit}\" .", sourceProject, targetProject, targetWIQLQuery);

            return targetWIQLQuery;
        }

    }
}
