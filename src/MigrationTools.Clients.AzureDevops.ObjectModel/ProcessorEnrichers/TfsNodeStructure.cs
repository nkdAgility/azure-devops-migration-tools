using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Server;
using MigrationTools._EngineV1.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.FieldMaps;
using MigrationTools.Processors;
using Newtonsoft.Json;
using Serilog.Context;
using Serilog.Events;
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

        public TfsNodeStructure(IServiceProvider services, ILogger<TfsNodeStructure> logger)
            : base(services, logger)
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
                    Log.LogDebug("NodeStructureEnricher.GetNewNodeName::Mappers::{key}::match detected", mapper.Key);
                    string replacement = Regex.Replace(sourceNodePath, mapper.Key, mapper.Value);
                    Log.LogDebug("NodeStructureEnricher.GetNewNodeName::Mappers::{key}::replaceWith({replace})", mapper.Key, replacement);
                    return replacement;
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

                _lastResortRemapRule = _Options.PrefixProjectToNodes
                    ? new KeyValuePair<string, string>($"^{searchEscapedSourceProjectName}", $"{replaceEscapedTargetProjectName}\\{replaceEscapedSourceProjectName}")
                    : new KeyValuePair<string, string>($"^{searchEscapedSourceProjectName}", $"{replaceEscapedTargetProjectName}");
            }

            return _lastResortRemapRule.Value;
        }

        private NodeInfo GetOrCreateNode(string nodePath, DateTime? startDate, DateTime? finishDate)
        {
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
                        parentNode= _targetCommonStructureService.GetNodeFromPath(currentAncestorPath);
                    }
                }
                else
                {
                    throw new InvalidOperationException($"This does not look like a valid area or iteration path: {currentAncestorPath}");
                }
            } while (parentNode == null && !string.IsNullOrEmpty(currentAncestorPath));

            if (parentNode == null)
            {
                throw new InvalidOperationException(
                    $"Path {nodePath} is not anchored in the target project, it cannot be created.");
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
                MigrateAllNodeStructures();
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
                    var newSystemPath = GetSystemPath(newUserPath, nodeStructureType);

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

        private string GetSystemPath(string newUserPath, TfsNodeStructureType structureType)
        {
            var match = Regex.Match(newUserPath, @"^(?<projectName>[^\\]+)\\(?<restOfThePath>.*)$");
            if (!match.Success)
            {
                throw new InvalidOperationException($"This path is not a valid area or iteration path: {newUserPath}");
            }

            var structureName = GetTargetLocalizedNodeStructureTypeName(structureType);

            return $"\\{match.Groups["projectName"].Value}\\{structureName}\\{match.Groups["restOfThePath"]}";
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

        private string GetTargetLocalizedNodeStructureTypeName(TfsNodeStructureType value)
        {
            switch (value)
            {
                case TfsNodeStructureType.Area:
                    return _targetLanguageMaps.AreaPath;

                case TfsNodeStructureType.Iteration:
                    return _targetLanguageMaps.IterationPath;

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

        public List<string> CheckForMissingPaths(List<WorkItemData> workItems, TfsNodeStructureType nodeType)
        {
            EntryForProcessorType(null);
            contextLog.Debug("TfsNodeStructure:CheckForMissingPaths");
            _targetCommonStructureService.ClearProjectInfoCache();

            string fieldName = GetFieldNameFromTfsNodeStructureType(nodeType);

            List<string> areaPaths = workItems.SelectMany(x => x.Revisions.Values)
                .Where(x => x.Fields[fieldName].Value.ToString().Contains("\\"))
                .Select(x => x.Fields[fieldName].Value.ToString())
                .Distinct()
                .ToList();

            List<string> missingPaths = new List<string>();

            foreach (var areaPath in areaPaths)
            {
                contextLog.Debug($"TfsNodeStructure:CheckForMissingPaths:Checking::{areaPath}");
                var newpath = "";
                bool keepProcessing = true;
                try
                {
                    newpath = GetNewNodeName(areaPath, nodeType);
                    contextLog.Debug($"TfsNodeStructure:CheckForMissingPaths:newpath::{newpath}");
                }
                catch(NodePathNotAnchoredException ex) {
                    contextLog.Debug($"TfsNodeStructure:CheckForMissingPaths:NodePathNotAnchoredException::{areaPath}");
                    keepProcessing = false;
                    missingPaths.Add(newpath);
                }
                if (keepProcessing) {
                    var systempath  = GetSystemPath(newpath, nodeType);
                    try
                    {
                        contextLog.Debug($"TfsNodeStructure:CheckForMissingPaths:CheckTarget::{systempath}");
                        NodeInfo c = _targetCommonStructureService.GetNodeFromPath(systempath);
                        contextLog.Debug($"TfsNodeStructure:CheckForMissingPaths:CheckTarget::{systempath}::FOUND");
                    }
                    catch
                    {
                        contextLog.Debug($"TfsNodeStructure:CheckForMissingPaths:CheckTarget::{systempath}::NOTFOUND");
                        if (_Options.ShouldCreateMissingRevisionPaths && ShouldCreateNode(systempath))
                        {
                        
                            contextLog.Debug($"TfsNodeStructure:CheckForMissingPaths:CheckTarget::{systempath}::CREATE");
                            GetOrCreateNode(systempath, null, null);
                        }
                        else
                        { 
                            missingPaths.Add(newpath);
                            contextLog.Debug($"TfsNodeStructure:CheckForMissingPaths:CheckTarget::{systempath}::LOG-ONLY");
                        }
                    }
                }
            }
            return missingPaths;
        }
            

        public bool ValidateTargetNodesExist(List<WorkItemData> workItems)
        {
            bool passedValidation = true;
            List<string> missingAreaPaths = CheckForMissingPaths(workItems, TfsNodeStructureType.Area);
            if (missingAreaPaths.Count > 0)
            {
                contextLog.Fatal("!! There are {missingAreaPaths} AreaPaths found in the history of the Source that are missing from the Target. These MUST be added or mapped with a fieldMap before we can continue.", missingAreaPaths.Count);
                foreach (string areaPath in missingAreaPaths)
                {
                    contextLog.Warning("MISSING Area: {areaPath}", areaPath);
                }

                passedValidation = false;
            }
            List<string> missingIterationPaths = CheckForMissingPaths(workItems, TfsNodeStructureType.Iteration);
            if (missingIterationPaths.Count > 0)
            {
                contextLog.Fatal("!! There are {missingIterationPaths} IterationPaths found in the history of the Source that are missing from the Target. These MUST be added or mapped with a fieldMap before we can continue.", missingIterationPaths.Count);
                foreach (string iterationPath in missingIterationPaths)
                {
                    contextLog.Warning("MISSING Iteration: {iterationPath}", iterationPath);
                }
                passedValidation = false;
            }
            return passedValidation;
        }
    }
}
