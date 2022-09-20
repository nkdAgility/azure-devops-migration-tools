using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Text;
using System.Text.Json.Serialization;

namespace MigrationTools.DataContracts.WorkItems
{
    [ApiPath("wit/wiql")]
    [ApiName("Wiql")]
    public class Wiql : RestApiDefinition
    {
        public override bool HasTaskGroups()
        {
            throw new NotImplementedException();
        }

        public override bool HasVariableGroups()
        {
            throw new NotImplementedException();
        }

        public override void ResetObject()
        {
            throw new NotImplementedException();
        }
    }

    public class WiqlRequest
    {
        public string Query { get; set; }
    }

    public class WiqlResponse
    {
        public WiqlResponseWorkItem[] WorkItems { get; set; } 
    }

    public class WiqlResponseWorkItem
    {
        public int Id { get; set; }
        public string Url { get; set; }
    }

    [ApiPath("wit/workitemsbatch")]
    [ApiName("WorkItem")]
    public class WorkItemBatchResult : RestApiDefinition
    {
        public int Count { get; set; }
        public WorkItem[] Value { get; set; }

        public override bool HasTaskGroups()
        {
            throw new NotImplementedException();
        }

        public override bool HasVariableGroups()
        {
            throw new NotImplementedException();
        }

        public override void ResetObject()
        {
            throw new NotImplementedException();
        }
    }

    public class WorkItemBatchRequest
    {
        public List<int> Ids { get; set; }

        [Newtonsoft.Json.JsonProperty("$expand")]
        public string Expand { get; set; } = "relations";
    }

    [ApiPath("wit/workitems")]
    [ApiName("WorkItem")]
    public class WorkItem : RestApiDefinition
    {
        public Relation[] Relations { get; set; }

        public string Url { get; set; }

        public override bool HasTaskGroups()
        {
            throw new NotImplementedException();
        }

        public override bool HasVariableGroups()
        {
            throw new NotImplementedException();
        }

        public override void ResetObject()
        {
            throw new NotImplementedException();
        }
    }

    public class Relation
    {
        public string Rel { get; set; }
        public string Url { get; set; }
        public Attributes Attributes { get; set; }

    }

    public class Attributes
    {
        public string Name { get;set; }
    }
}
