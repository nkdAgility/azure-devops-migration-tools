using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;
using VSTS.DataBulkEditor.Engine.Configuration.Processing;

namespace VSTS.DataBulkEditor.Engine
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
            ICommonStructureService targetCss = (ICommonStructureService)me.Target.Collection.GetService(typeof(ICommonStructureService));
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
                CreateNodes(sourceTree.ChildNodes[0].ChildNodes[0].ChildNodes, targetCss, structureParent);
            }
        }

        private void CreateNodes(XmlNodeList nodeList, ICommonStructureService css, NodeInfo parentPath)
        {
            foreach (XmlNode item in nodeList)
            {
                string newNodeName = item.Attributes["Name"].Value;
                NodeInfo targetNode = CreateNode(css, newNodeName, parentPath);
                if (item.HasChildNodes)
                {
                    CreateNodes(item.ChildNodes[0].ChildNodes, css, targetNode);
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
    
    }
}