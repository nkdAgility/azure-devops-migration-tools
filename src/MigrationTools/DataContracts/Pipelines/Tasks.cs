using System;
using System.Collections.Generic;
using System.Text;

namespace MigrationTools.DataContracts.Pipelines
{
    [ApiPath("distributedtask/tasks", false)]
    [ApiName("Tasks")]
    public class TaskDefinition : RestApiDefinition
    {
        public override bool HasTaskGroups() => false;

        public override bool HasVariableGroups() => false;

        public override void ResetObject() {}
    }
}
