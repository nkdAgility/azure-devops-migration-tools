namespace MigrationTools.DataContracts.Pipelines
{
    public partial class Projects : RestApiDefinition
    {
        public Project Project { get; set; }

        public override bool HasTaskGroups()
        {
            return false;
        }

        public override bool HasVariableGroups()
        {
            return false;
        }

        public override void ResetObject()
        {
        }
    }
}