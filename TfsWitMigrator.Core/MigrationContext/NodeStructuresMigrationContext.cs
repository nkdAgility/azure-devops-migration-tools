using Microsoft.TeamFoundation.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace VSTS.DataBulkEditor.Engine
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

            NodeInfo sourceAreaNode = (from n in sourceNodes where n.Path.Contains("Area") select n).Single();

            XmlElement sourceAreaTree = sourceCss.GetNodesXml(new string[] { sourceAreaNode.Uri }, true);

            NodeInfo sourceIterationNode = (from n in sourceNodes where n.Path.Contains("Iteration") select n).Single();
            XmlElement sourceIterationsTree = sourceCss.GetNodesXml(new string[] { sourceIterationNode.Uri }, true);
            //////////////////////////////////////////////////
            ICommonStructureService targetCss = (ICommonStructureService)me.Target.Collection.GetService(typeof(ICommonStructureService));
            //////////////////////////////////////////////////

            NodeInfo areaParent = CreateNode(targetCss, me.Source.Name, targetCss.GetNodeFromPath(string.Format("\\{0}\\Area", me.Target.Name)));
            if (sourceAreaTree.ChildNodes[0].HasChildNodes)
            { 
            CreateNodes(sourceAreaTree.ChildNodes[0].ChildNodes[0].ChildNodes, targetCss, areaParent);
            }
            NodeInfo iterationParent = CreateNode(targetCss, me.Source.Name, targetCss.GetNodeFromPath(string.Format("\\{0}\\Iteration", me.Target.Name)));
            if (sourceIterationsTree.ChildNodes[0].HasChildNodes)
            {
                CreateNodes(sourceIterationsTree.ChildNodes[0].ChildNodes[0].ChildNodes, targetCss, iterationParent);
            }
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
            catch (CommonStructureSubsystemException ex)
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