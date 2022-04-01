using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.TeamFoundation.Server;
using MigrationTools._EngineV1.Clients;
using MigrationTools.DataContracts;
using MigrationTools.Endpoints;
using MigrationTools.Processors;
using Newtonsoft.Json;

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
        private Dictionary<string, bool> _foundNodes = new Dictionary<string, bool>();
        private string[] _nodeBasePaths;
        private TfsNodeStructureOptions _Options;

        private bool _prefixProjectToNodes = false;

        private ICommonStructureService4 _sourceCommonStructureService;

        private TfsLanguageMapOptions _sourceLanguageMaps;
        private ProjectInfo _sourceProjectInfo;

        private string _sourceProjectName;
        private NodeInfo[] _sourceRootNodes;
        private ICommonStructureService4 _targetCommonStructureService;
        private TfsLanguageMapOptions _targetLanguageMaps;
        private string _targetProjectName;

        public TfsNodeStructure(IServiceProvider services, ILogger<TfsNodeStructure> logger)
            : base(services, logger)
        {
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
            _foundNodes = settings.FoundNodes;
        }

        [Obsolete("Old v1 arch: this is a v2 class", true)]
        public override int Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
            throw new NotImplementedException();
        }

        public string GetNewNodeName(string sourceNodeName, TfsNodeStructureType nodeStructureType, string targetStructureName = null, string sourceStructureName = null)
        {
            Log.LogDebug("NodeStructureEnricher.GetNewNodeName({sourceNodeName}, {nodeStructureType})", sourceNodeName, nodeStructureType.ToString());
            var tStructureName = targetStructureName ?? NodeStructureTypeToLanguageSpecificName(_targetLanguageMaps, nodeStructureType);
            var sStructureName = sourceStructureName ?? NodeStructureTypeToLanguageSpecificName(_sourceLanguageMaps, nodeStructureType);
            // Replace project name with new name (if necessary) and inject nodePath (Area or Iteration) into path for node validation
            string newNodeName;
            if (_prefixProjectToNodes)
            {
                newNodeName = $@"{_targetProjectName}\{tStructureName}\{sourceNodeName}";
            }
            else
            {
                var regex = new Regex(Regex.Escape(_sourceProjectName));
                if (sourceNodeName.StartsWith($@"{_sourceProjectName}\{sStructureName}\"))
                {
                    newNodeName = regex.Replace(sourceNodeName, _targetProjectName, 1);
                }
                else
                {
                    newNodeName = regex.Replace(sourceNodeName, $@"{_targetProjectName}\{tStructureName}", 1);
                }
            }

            // Validate the node exists
            if (!TargetNodeExists(newNodeName))
            {
                Log.LogWarning("The Node '{newNodeName}' does not exist, leaving as '{newProjectName}'. This may be because it has been renamed or moved and no longer exists, or that you have not migrateed the Node Structure yet.", newNodeName, _targetProjectName);
                newNodeName = _targetProjectName;
            }

            // Remove nodePath (Area or Iteration) from path for correct population in work item
            if (newNodeName.StartsWith(_targetProjectName + '\\' + tStructureName + '\\'))
            {
                newNodeName = newNodeName.Remove(newNodeName.IndexOf($@"{nodeStructureType}\"), $@"{nodeStructureType}\".Length);
            }
            else if (newNodeName.StartsWith(_targetProjectName + '\\' + tStructureName))
            {
                newNodeName = newNodeName.Remove(newNodeName.IndexOf($@"{nodeStructureType}"), $@"{nodeStructureType}".Length);
            }
            newNodeName = newNodeName.Replace(@"\\", @"\");

            return newNodeName;
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

        private NodeInfo CreateNode(string name, NodeInfo parent, DateTime? startDate, DateTime? finishDate)
        {
            System.Threading.Thread.Sleep(1000);
            string nodePath = string.Format(@"{0}\{1}", parent.Path, name);
            NodeInfo node;
            Log.LogInformation(" Processing Node: {0}, start date: {1}, finish date: {2}", nodePath, startDate, finishDate);
            try
            {
                node = _targetCommonStructureService.GetNodeFromPath(nodePath);
                Log.LogDebug("  Node {node} already exists", nodePath);
                Log.LogTrace("{node}", node);
            }
            catch (CommonStructureSubsystemException ex)
            {
                try
                {
                    string newPathUri = _targetCommonStructureService.CreateNode(name, parent.Uri);
                    Log.LogDebug("  Node {newPathUri} has been created", newPathUri);
                    node = _targetCommonStructureService.GetNode(newPathUri);
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
                    _targetCommonStructureService.SetIterationDates(node.Uri, startDate, finishDate);
                    Log.LogDebug("  Node {node} has been assigned {startDate} / {finishDate}", nodePath, startDate, finishDate);
                }
                catch (CommonStructureSubsystemException ex)
                {
                    Log.LogWarning(ex, " Unable to set {node}dates of {startDate} / {finishDate}", nodePath, startDate, finishDate);
                }
            }
            return node;
        }

        private void CreateNodes(XmlNodeList nodeList, NodeInfo parentPath, string treeType)
        {
            foreach (XmlNode item in nodeList)
            {
                string newNodeName = item.Attributes["Name"].Value;

                if (!ShouldCreateNode(parentPath, newNodeName))
                {
                    continue;
                }

                NodeInfo targetNode;
                if (treeType == "Iteration")
                {
                    DateTime? startDate = null;
                    DateTime? finishDate = null;
                    if (item.Attributes["StartDate"] != null)
                    {
                        startDate = DateTime.Parse(item.Attributes["StartDate"].Value);
                    }
                    if (item.Attributes["FinishDate"] != null)
                    {
                        finishDate = DateTime.Parse(item.Attributes["FinishDate"].Value);
                    }

                    targetNode = CreateNode(newNodeName, parentPath, startDate, finishDate);
                }
                else
                {
                    targetNode = CreateNode(newNodeName, parentPath, null, null);
                }
                if (item.HasChildNodes)
                {
                    CreateNodes(item.ChildNodes[0].ChildNodes, targetNode, treeType);
                }
            }
        }

        private void MigrateAllNodeStructures()
        {
            _prefixProjectToNodes = Options.PrefixProjectToNodes;
            _nodeBasePaths = Options.NodeBasePaths;

            Log.LogDebug("NodeStructureEnricher.MigrateAllNodeStructures({prefixProjectToNodes}, {nodeBasePaths})", _prefixProjectToNodes, _nodeBasePaths);
            //////////////////////////////////////////////////
            ProcessCommonStructure(_sourceLanguageMaps.AreaPath, _sourceProjectName, _targetLanguageMaps.AreaPath, _targetProjectName);
            //////////////////////////////////////////////////
            ProcessCommonStructure(_sourceLanguageMaps.IterationPath, _sourceProjectName, _targetLanguageMaps.IterationPath, _targetProjectName);
            //////////////////////////////////////////////////
        }

        private string NodeStructureTypeToLanguageSpecificName(TfsLanguageMapOptions languageMaps, TfsNodeStructureType value)
        {
            // insert switch statement here
            switch (value)
            {
                case TfsNodeStructureType.Area:
                    return languageMaps.AreaPath;

                case TfsNodeStructureType.Iteration:
                    return languageMaps.IterationPath;

                default:
                    throw new InvalidOperationException("Not a valid NodeStructureType ");
            }
        }

        private void ProcessCommonStructure(string treeTypeSource, string sourceTarget, string treeTypeTarget, string projectTarget)
        {
            Log.LogDebug("NodeStructureEnricher.ProcessCommonStructure({treeTypeSource}, {treeTypeTarget})", treeTypeSource, treeTypeTarget);

            var startPath = ("\\" + this._sourceProjectName + "\\" + treeTypeSource).ToLower();
            Log.LogDebug("Source Node Path StartsWith [{startPath}]", startPath);

            var nodes = _sourceRootNodes.Where((n) =>
            {
                // (i.e. "\CoolProject\Area" )
                return n.Path.ToLower().StartsWith(startPath);
            });
            if (nodes.Count() > 1)
            {
                Exception ex = new Exception(string.Format("Unable to load Common Structure for Source because more than one node path matches \"{0}\". {1}", treeTypeSource, JsonConvert.SerializeObject(nodes.Select(x => x.Path))));
                Log.LogError(ex, "Unable to load Common Structure for Source.");
                throw ex;
            }
            NodeInfo sourceNode = nodes.First();
            if (sourceNode == null) // May run into language problems!!! This is to try and detect that
            {
                Exception ex = new Exception(string.Format("Unable to load Common Structure for Source. This is usually due to diferent language versions. Validate that '{0}' is the correct name in your version. ", treeTypeSource));
                Log.LogError(ex, "Unable to load Common Structure for Source.");
                throw ex;
            }
            XmlElement sourceTree = _sourceCommonStructureService.GetNodesXml(new string[] { sourceNode.Uri }, true);
            NodeInfo structureParent;
            try // May run into language problems!!! This is to try and detect that
            {
                structureParent = _targetCommonStructureService.GetNodeFromPath(string.Format("\\{0}\\{1}", projectTarget, treeTypeTarget));
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception(string.Format("Unable to load Common Structure for Target.This is usually due to diferent language versions. Validate that '{0}' is the correct name in your version. ", treeTypeTarget), ex);
                Log.LogError(ex2, "Unable to load Common Structure for Target.");
                throw ex2;
            }
            if (_prefixProjectToNodes)
            {
                structureParent = CreateNode(sourceTarget, structureParent, null, null);
            }
            if (sourceTree.ChildNodes[0].HasChildNodes)
            {
                CreateNodes(sourceTree.ChildNodes[0].ChildNodes[0].ChildNodes, structureParent, treeTypeTarget);
            }
        }

        /// <summary>
        /// Checks node-to-be-created with allowed BasePath's
        /// </summary>
        /// <param name="parentPath">Parent Node</param>
        /// <param name="newNodeName">Node to be created</param>
        /// <returns>true/false</returns>
        private bool ShouldCreateNode(NodeInfo parentPath, string newNodeName)
        {
            string nodePath = string.Format(@"{0}\{1}", parentPath.Path, newNodeName);

            if (_nodeBasePaths != null && _nodeBasePaths.Any())
            {
                var split = nodePath.Split('\\');
                var removeProjectAndType = split.Skip(3);
                var path = string.Join(@"\", removeProjectAndType);

                // We need to check if the path is a parent path of one of the base paths, as we need those
                foreach (var basePath in _nodeBasePaths)
                {
                    var splitBase = basePath.Split('\\');

                    for (int i = 0; i < splitBase.Length; i++)
                    {
                        if (string.Equals(path, string.Join(@"\", splitBase.Take(i)), StringComparison.InvariantCultureIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                if (!_nodeBasePaths.Any(p => path.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Log.LogWarning("The node {nodePath} is being excluded due to your basePath setting. ", nodePath);
                    return false;
                }
            }

            return true;
        }

        private bool TargetNodeExists(string nodePath)
        {
            if (!_foundNodes.ContainsKey(nodePath))
            {
                try
                {
                    var node = _targetCommonStructureService.GetNodeFromPath(nodePath);
                    _foundNodes.Add(nodePath, true);
                }
                catch
                {
                    _foundNodes.Add(nodePath, false);
                }
            }
            return _foundNodes[nodePath];
        }
    }
}