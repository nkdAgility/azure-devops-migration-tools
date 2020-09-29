using System;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using Microsoft.TeamFoundation.Server;
using MigrationTools;
using MigrationTools.Core;
using MigrationTools.Core.Configuration;
using MigrationTools.Core.Configuration.Processing;
using MigrationTools.Core.Engine.Processors;
using Serilog;

namespace VstsSyncMigrator.Engine
{
    public class NodeStructuresMigrationContext : MigrationProcessorBase
    {
        NodeStructuresMigrationConfig _config;

        public override string Name
        {
            get
            {
                return "NodeStructuresMigrationContext";
            }
        }

        public NodeStructuresMigrationContext(IMigrationEngine me, IServiceProvider services, ITelemetryLogger telemetry) : base(me, services, telemetry)
        {
        }

        public override void Configure(IProcessorConfig config)
        {
            _config = (NodeStructuresMigrationConfig)config;
        }

        protected override void InternalExecute()
        {
            if (_config == null)
            {
                throw new Exception("You must call Configure() first");
            }
            //////////////////////////////////////////////////
            ICommonStructureService sourceCss = (ICommonStructureService)Engine.Source.GetService<ICommonStructureService>();
            ProjectInfo sourceProjectInfo = sourceCss.GetProjectFromName(Engine.Source.Config.Project);
            NodeInfo[] sourceNodes = sourceCss.ListStructures(sourceProjectInfo.Uri);
            //////////////////////////////////////////////////
            ICommonStructureService targetCss = (ICommonStructureService)Engine.Target.GetService<ICommonStructureService4>();

            //////////////////////////////////////////////////
            ProcessCommonStructure(Engine.Source.Config.LanguageMaps.AreaPath, Engine.Target.Config.LanguageMaps.AreaPath, sourceNodes, targetCss, sourceCss);
            //////////////////////////////////////////////////
            ProcessCommonStructure(Engine.Source.Config.LanguageMaps.IterationPath, Engine.Target.Config.LanguageMaps.IterationPath, sourceNodes, targetCss, sourceCss);
            //////////////////////////////////////////////////
        }

        private void ProcessCommonStructure(string treeTypeSource, string treeTypeTarget, NodeInfo[] sourceNodes, ICommonStructureService targetCss, ICommonStructureService sourceCss)
        {
            NodeInfo sourceNode = (from n in sourceNodes where n.Path.Contains(treeTypeSource) select n).Single();
            if (sourceNode == null) // May run into language problems!!! This is to try and detect that
            {
                Exception ex = new Exception(string.Format("Unable to load Common Structure for Source. This is usually due to diferent language versions. Validate that '{0}' is the correct name in your version. ", treeTypeSource));
                Log.Error(ex, "Unable to load Common Structure for Source.");
                throw ex;
            }
            XmlElement sourceTree = sourceCss.GetNodesXml(new string[] { sourceNode.Uri }, true);
            NodeInfo structureParent;
            try // May run into language problems!!! This is to try and detect that
            {
                structureParent = targetCss.GetNodeFromPath(string.Format("\\{0}\\{1}", Engine.Target.Config.Project, treeTypeTarget));
            }
            catch (Exception ex)
            {
                Exception ex2 = new Exception(string.Format("Unable to load Common Structure for Target.This is usually due to diferent language versions. Validate that '{0}' is the correct name in your version. ", treeTypeTarget), ex);
                Log.Error(ex2, "Unable to load Common Structure for Target.");
                throw ex2;
            }
            if (_config.PrefixProjectToNodes)
            {
                structureParent = CreateNode(targetCss, Engine.Source.Config.Project, structureParent);
            }
            if (sourceTree.ChildNodes[0].HasChildNodes)
            {
                CreateNodes(sourceTree.ChildNodes[0].ChildNodes[0].ChildNodes, targetCss, structureParent, treeTypeTarget);
            }
        }

        private void CreateNodes(XmlNodeList nodeList, ICommonStructureService css, NodeInfo parentPath, string treeType)
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

                    targetNode = CreateNode(css, newNodeName, parentPath, startDate, finishDate);
                }
                else
                {
                    targetNode = CreateNode(css, newNodeName, parentPath);
                }
                if (item.HasChildNodes)
                {
                    CreateNodes(item.ChildNodes[0].ChildNodes, css, targetNode, treeType);
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

            if (_config.BasePaths != null && _config.BasePaths.Any())
            {
                var split = nodePath.Split('\\');
                var removeProjectAndType = split.Skip(3);
                var path = string.Join(@"\", removeProjectAndType);

                // We need to check if the path is a parent path of one of the base paths, as we need those
                foreach (var basePath in _config.BasePaths)
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

                if (!_config.BasePaths.Any(p => path.StartsWith(p, StringComparison.InvariantCultureIgnoreCase)))
                {
                    Trace.WriteLine(string.Format("--IgnoreNode: {0}", nodePath));
                    return false;
                }
            }

            return true;
        }

        private NodeInfo CreateNode(ICommonStructureService css, string name, NodeInfo parent)
        {
            string nodePath = string.Format(@"{0}\{1}", parent.Path, name);
            NodeInfo node = null;

            Trace.Write(string.Format("--CreateNode: {0}", nodePath));
            try
            {
                node = css.GetNodeFromPath(nodePath);
                Trace.Write("...found");
            }
            catch (CommonStructureSubsystemException ex)
            {
                try
                {
                    string newPathUri = css.CreateNode(name, parent.Uri);
                    Trace.Write("...created");
                    node = css.GetNode(newPathUri);
                }
                catch
                {
                    Log.Error(ex, "Creating Node");
                    Trace.Write("...missing");
                    throw;
                }
            }

            Trace.WriteLine(String.Empty);
            return node;
        }

        private NodeInfo CreateNode(ICommonStructureService css, string name, NodeInfo parent, DateTime? startDate, DateTime? finishDate)
        {
            string nodePath = string.Format(@"{0}\{1}", parent.Path, name);
            NodeInfo node = null;
            Trace.Write(string.Format("--CreateNode: {0}, start date: {1}, finish date: {2}", nodePath, startDate, finishDate));
            try
            {
                node = css.GetNodeFromPath(nodePath);
                Trace.Write("...found");
            }
            catch (CommonStructureSubsystemException ex)
            {
                try
                {
                    string newPathUri = css.CreateNode(name, parent.Uri);
                    Log.Information("...created");
                    node = css.GetNode(newPathUri);
                }
                catch
                {
                    Log.Warning(ex, "Missing ");
                    throw;
                }
            }

            try
            {
                ((ICommonStructureService4)css).SetIterationDates(node.Uri, startDate, finishDate);
                Trace.Write("...dates assigned");
            }
            catch (CommonStructureSubsystemException ex)
            {
                Log.Warning(ex, "Dates not set ");
            }

            Trace.WriteLine(String.Empty);
            return node;
        }

    }
}