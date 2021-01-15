namespace MigrationTools.DataContracts.Pipelines
{
    [ApiPath("distributedtask/deploymentgroups")]
    [ApiName("Deployment Groups")]
    public class DeploymentGroup : RestApiDefinition
    {
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
            //We are not migrating this, right now only using the names for mapping it over to target
        }
    }
}
