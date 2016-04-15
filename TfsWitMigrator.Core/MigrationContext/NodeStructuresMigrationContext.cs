using Microsoft.TeamFoundation.Server;
using System;
using System.Diagnostics;
using System.Xml;

namespace TfsWitMigrator.Core
{
    public class NodeStructuresMigrationContext : MigrationContextBase
    {

        public override string Name
        {
            get
            {
                return "NodeStructuresMigrationContext";
            }
        }

        public NodeStructuresMigrationContext(MigrationEngine me) : base(me)
        {
        }

        internal override void InternalExecute()
        {
            //////////////////////////////////////////////////
            ICommonStructureService sourceCss = (ICommonStructureService)me.Source.Collection.GetService(typeof(ICommonStructureService));
            ProjectInfo sourceProjectInfo = sourceCss.GetProjectFromName(me.Source.Name);
            NodeInfo[] sourceNodes = sourceCss.ListStructures(sourceProjectInfo.Uri);
            XmlElement sourceAreaTree = sourceCss.GetNodesXml(new string[] { sourceNodes[0].Uri }, true);
            XmlElement sourceIterationsTree = sourceCss.GetNodesXml(new string[] { sourceNodes[1].Uri }, true);
            //////////////////////////////////////////////////
            ICommonStructureService targetCss = (ICommonStructureService)me.Target.Collection.GetService(typeof(ICommonStructureService));
            //////////////////////////////////////////////////

            NodeInfo areaParent = CreateNode(targetCss, me.Source.Name, targetCss.GetNodeFromPath(string.Format("\\{0}\\Area", me.Target.Name)));
            CreateNodes(sourceAreaTree.ChildNodes[0].ChildNodes[0].ChildNodes, targetCss, areaParent);
            CreateNodes(sourceIterationsTree.ChildNodes[0].ChildNodes[0].ChildNodes, targetCss, targetCss.GetNodeFromPath(string.Format("\\{0}\\Iteration", me.Target.Name)));
            //////////////////////////////////////////////////
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
            catch (CommonStructureSubsystemException)
            {
                Trace.Write("...missing");
                string newPathUri = css.CreateNode(name, parent.Uri);
                Trace.Write("...created");
                node = css.GetNode(newPathUri);
            }
            return node;
        }
    
    }
}