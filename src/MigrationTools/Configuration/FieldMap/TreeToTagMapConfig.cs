namespace MigrationTools.Configuration.FieldMap
{
    public class TreeToTagMapConfig : IFieldMapConfig
    {
        public string WorkItemTypeName { get; set; }
        public int toSkip { get; set; }
        public int timeTravel { get; set; }

        public string FieldMap
        {
            get
            {
                return "TreeToTagFieldMap";
            }
        }
    }
}
