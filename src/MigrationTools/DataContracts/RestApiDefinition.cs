using System.Collections.Generic;

namespace MigrationTools.DataContracts
{
    public abstract class RestApiDefinition
    {
        private string sId;
        public virtual string Name { get; set; }
        public virtual string Id { get; set; }

        public string GetSourceId()
        {
            return sId;
        }

        public void SetSourceId(string id)
        {
            sId = id;
        }

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

        public virtual dynamic ToJson() { return this; }
    }

    public class RestResultDefinition<ValueType> where ValueType : RestApiDefinition, new()
    {
        public long Count { get; set; }
        public IEnumerable<ValueType> Value { get; set; }
    }
}