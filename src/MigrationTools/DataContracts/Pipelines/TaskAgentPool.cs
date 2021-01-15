namespace MigrationTools.DataContracts.Pipelines
{
    [ApiPath("distributedtask/queues")]
    [ApiName("Agent Pools")]
    public class TaskAgentPool : RestApiDefinition
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
