using System.Collections.Generic;

namespace MigrationTools.DataContracts.WorkItems
{
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

    public class WorkItemBatchResult
    {
        public int Count { get; set; }
        public WorkItem[] Value { get; set; }

    }

    public class WorkItemBatchRequest
    {
        public List<int> Ids { get; set; }

        [Newtonsoft.Json.JsonProperty("$expand")]
        public string Expand { get; set; } = "relations";
    }

    public class WorkItem
    {
        public int Id { get; set; }
        public Relation[] Relations { get; set; }

        public string Url { get; set; }
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
        public string Comment { get; set; }
    }

    public class AddLink
    {
        public string Op { get; set; } = "add";
        public string Path { get; set; } = "/relations/-";

        public Relation Value { get; set; }
    }
}
