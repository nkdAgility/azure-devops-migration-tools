using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using VstsSyncMigrator.Engine.Configuration.Processing;

namespace VstsSyncMigrator.Engine
{
    public class NodeStructuresMigrationContext : MigrationContextBase
    {
        NodeStructuresMigrationConfig config;

        public override string Name
        {
            get
            {
                return "NodeStructuresMigrationContext";
            }
        }

        public NodeStructuresMigrationContext(MigrationEngine me, NodeStructuresMigrationConfig config) : base(me, config)
        {
            this.config = config;
        }

        internal override void InternalExecute()
        {
            //////////////////////////////////////////////////
            ICommonStructureService sourceCss = (ICommonStructureService)me.Source.Collection.GetService(typeof(ICommonStructureService));
            ProjectInfo sourceProjectInfo = sourceCss.GetProjectFromName(me.Source.Name);
            NodeInfo[] sourceNodes = sourceCss.ListStructures(sourceProjectInfo.Uri);
            //////////////////////////////////////////////////
            ICommonStructureService targetCss = (ICommonStructureService)me.Target.Collection.GetService(typeof(ICommonStructureService4));

            //////////////////////////////////////////////////
            ProcessCommonStructure("Area", sourceNodes, targetCss, sourceCss);
            //////////////////////////////////////////////////
            ProcessCommonStructure("Iteration", sourceNodes, targetCss, sourceCss);
            //////////////////////////////////////////////////
        }

        private void ProcessCommonStructure(string treeType, NodeInfo[] sourceNodes, ICommonStructureService targetCss, ICommonStructureService sourceCss)
        {
            NodeInfo sourceNode = (from n in sourceNodes where n.Path.Contains(treeType) select n).Single();
            XmlElement sourceTree = sourceCss.GetNodesXml(new string[] { sourceNode.Uri }, true);
            NodeInfo structureParent = targetCss.GetNodeFromPath(string.Format("\\{0}\\{1}", me.Target.Name, treeType));
            if (config.PrefixProjectToNodes)
            {
                structureParent = CreateNode(targetCss, me.Source.Name, structureParent);
            }
            if (sourceTree.ChildNodes[0].HasChildNodes)
            {
                CreateNodes(sourceTree.ChildNodes[0].ChildNodes[0].ChildNodes, targetCss, structureParent, treeType);
            }
        }

        private void CreateNodes(XmlNodeList nodeList, ICommonStructureService css, NodeInfo parentPath, string treeType)
        {
            foreach (XmlNode item in nodeList)
            {
                string newNodeName = item.Attributes["Name"].Value;
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
                Telemetry.Current.TrackException(ex);
                Trace.Write("...missing");
                string newPathUri = css.CreateNode(name, parent.Uri);
                Trace.Write("...created");
                node = css.GetNode(newPathUri);
            }
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
                Telemetry.Current.TrackException(ex);
                Trace.Write("...missing");
                string newPathUri = css.CreateNode(name, parent.Uri);
                Trace.Write("...created");
                node = css.GetNode(newPathUri);
                ((ICommonStructureService4)css).SetIterationDates(node.Uri, startDate, finishDate);
                Trace.Write("...dates assigned");
            }
            return node;
        }
    }
}