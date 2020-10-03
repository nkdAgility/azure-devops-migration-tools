using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Microsoft.TeamFoundation.Server;
using MigrationTools;
using MigrationTools.Configuration;
using MigrationTools.Configuration.Processing;
using MigrationTools.DataContracts;
using MigrationTools.Engine.Processors;
using MigrationTools.Enrichers;
using Serilog;

namespace MigrationTools.Clients.AzureDevops.ObjectModel.Enrichers
{
    public class NodeStructureEnricher : IWorkItemEnricher
    {
        private bool _prefixProjectToNodes = false;
        private ICommonStructureService _sourceCommonStructureService;
        private ICommonStructureService _targetCommonStructureService;
        private ProjectInfo _sourceProjectInfo;
        private NodeInfo[] _sourceRootNodes;
        private string[] _nodeBasePaths;

        public IMigrationEngine Engine { get; }

        public void Configure(bool save = true, bool filterWorkItemsThatAlreadyExistInTarget = true)
        {
            
        }

        public void Enrich(WorkItemData sourceWorkItem, WorkItemData targetWorkItem)
        {
   
        }

        public NodeStructureEnricher(IMigrationEngine engine)
        {
            Engine = engine;
            _sourceCommonStructureService = (ICommonStructureService)Engine.Source.GetService<ICommonStructureService>();
            _targetCommonStructureService = (ICommonStructureService)Engine.Target.GetService<ICommonStructureService4>();
            _sourceProjectInfo = _sourceCommonStructureService.GetProjectFromName(Engine.Source.Config.Project);
            _sourceRootNodes = _sourceCommonStructureService.ListStructures(_sourceProjectInfo.Uri);
        }

        public void MigrateAllNodeStructures(bool prefixProjectToNodes, string[] nodeBasePaths)
        {
            _prefixProjectToNodes = prefixProjectToNodes;
            _nodeBasePaths = nodeBasePaths;
            //////////////////////////////////////////////////
            ProcessCommonStructure( Engine.Source.Config.LanguageMaps.AreaPath, Engine.Target.Config.LanguageMaps.AreaPath);
            //////////////////////////////////////////////////
            ProcessCommonStructure( Engine.Source.Config.LanguageMaps.IterationPath, Engine.Target.Config.LanguageMaps.IterationPath);
            //////////////////////////////////////////////////
        }

        private void ProcessCommonStructure(string treeTypeSource, string treeTypeTarget)
        {
            NodeInfo sourceNode = (from n in _sourceRootNodes where n.Path.Contains(treeTypeSource) select n).Single();
            if (sourceNode == null) // May run into language problems!!! This is to try and detect that
            {
                Exception ex = new Exception(string.Format("Unable to load Common Structure for Source. This is usually due to diferent language versions. Validate that '{0}' is the correct name in your version. ", treeTypeSource));
                Log.Error(ex, "Unable to load Common Structure for Source.");
                throw ex;
            }
            XmlElement sourceTree = _sourceCommonStructureService.GetNodesXml(new string[] { sourceNode.Uri }, true);
            NodeInfo structureParent;
            try // May run into language problems!!! This is to try and detect that
            {
                structureParent = _targetCommonStructureService.GetNodeFromPath(string.Format("\\{0}\\{1}", Engine.Target.Config.Project, treeTypeTarget));
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception(string.Format("Unable to load Common Structure for Target.This is usually due to diferent language versions. Validate that '{0}' is the correct name in your version. ", treeTypeTarget), ex);
                Log.Error(ex2, "Unable to load Common Structure for Target.");
                throw ex2;
            }
            if (_prefixProjectToNodes)
            {
                structureParent = CreateNode( Engine.Source.Config.Project, structureParent);
            }
            if (sourceTree.ChildNodes[0].HasChildNodes)
            {
                CreateNodes(sourceTree.ChildNodes[0].ChildNodes[0].ChildNodes, structureParent, treeTypeTarget);
            }
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
                    targetNode = CreateNode( newNodeName, parentPath);
                }
                if (item.HasChildNodes)
                {
                    CreateNodes(item.ChildNodes[0].ChildNodes, targetNode, treeType);
                }
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
                    Log.Verbose("--IgnoreNode: {0}", nodePath);
                    return false;
                }
            }

            return true;
        }

        private NodeInfo CreateNode(string name, NodeInfo parent)
        {
            string nodePath = string.Format(@"{0}\{1}", parent.Path, name);
            NodeInfo node = null;

            Log.Verbose("--CreateNode: {0}", nodePath);
            try
            {
                node = _targetCommonStructureService.GetNodeFromPath(nodePath);
                Log.Verbose("...found");
            }
            catch (CommonStructureSubsystemException ex)
            {
                try
                {
                    string newPathUri = _targetCommonStructureService.CreateNode(name, parent.Uri);
                    Log.Verbose("...created");
                    node = _targetCommonStructureService.GetNode(newPathUri);
                }
                catch
                {
                    Log.Error(ex, "Creating Node");
                    Log.Verbose("...missing");
                    throw;
                }
            }
            Trace.WriteLine(String.Empty);
            return node;
        }

        private NodeInfo CreateNode(string name, NodeInfo parent, DateTime? startDate, DateTime? finishDate)
        {
            string nodePath = string.Format(@"{0}\{1}", parent.Path, name);
            NodeInfo node = null;
            Log.Verbose("--CreateNode: {0}, start date: {1}, finish date: {2}", nodePath, startDate, finishDate);
            try
            {
                node = _targetCommonStructureService.GetNodeFromPath(nodePath);
                Log.Verbose("...found");
            }
            catch (CommonStructureSubsystemException ex)
            {
                try
                {
                    string newPathUri = _targetCommonStructureService.CreateNode(name, parent.Uri);
                    Log.Verbose("...created");
                    node = _targetCommonStructureService.GetNode(newPathUri);
                }
                catch
                {
                    Log.Warning(ex, "Missing ");
                    throw;
                }
            }

            try
            {
                ((ICommonStructureService4)_targetCommonStructureService).SetIterationDates(node.Uri, startDate, finishDate);
                Log.Verbose("...dates assigned");
            }
            catch (CommonStructureSubsystemException ex)
            {
                Log.Warning(ex, "Dates not set ");
            }

            Log.Verbose(String.Empty);
            return node;
        }


    }
}