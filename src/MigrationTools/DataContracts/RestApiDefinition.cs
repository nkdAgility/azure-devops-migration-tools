using System.Collections.Generic;

namespace MigrationTools.DataContracts
{
    public abstract class RestApiDefinition
    {
        public string Name { get; set; }
        public string Id { get; set; }

        /// <summary>
        /// removes the values of generated propperties
        /// </summary>
        /// <returns>The cleanuped this</returns>
        public abstract RestApiDefinition GetMigrationObject();
    }

    public class RestResultDefinition<ValueType> where ValueType : RestApiDefinition, new()
    {
        public long Count { get; set; }
        public IEnumerable<ValueType> Value { get; set; }
    }
}