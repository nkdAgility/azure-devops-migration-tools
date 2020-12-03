using System.Collections.Generic;

namespace MigrationTools.DataContracts
{
    public abstract class RestApiDefinition
    {
        public string Name { get; set; }
        public string Id { get; set; }

        /// <summary>
        /// reset values that cannot be set on new objects
        /// </summary>
        /// <returns>The clean RestApiDefinition</returns>
        public abstract void ResetObject();

        /// <summary>
        /// Checks whether the Definition references Taskgroups
        /// </summary>
        /// <returns>bool</returns>
        public abstract bool HasTaskGroups();

        /// <summary>
        /// Checks whether the Definition references Variablegroups
        /// </summary>
        /// <returns>bool</returns>
        public abstract bool HasVariableGroups();
    }

    public class RestResultDefinition<ValueType> where ValueType : RestApiDefinition, new()
    {
        public long Count { get; set; }
        public IEnumerable<ValueType> Value { get; set; }
    }
}