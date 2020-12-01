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
        public abstract bool HasTaskGroups();
        public abstract bool HasVariableGroups();
    }

    public class TaskGroupStore
    {
        public string Name { get; set; }
        public string Id { get; set; }
    }

    public class RestResultDefinition<ValueType> where ValueType : RestApiDefinition, new()
    {
        public long Count { get; set; }
        public IEnumerable<ValueType> Value { get; set; }
    }
}