namespace MigrationTools.Processors
{
    /// <summary>
    /// Decides whether migrating one kind of definition (build pipelines, release pipelines, variable groups or
    /// service connections) left something behind.
    /// </summary>
    /// <remarks>
    /// Definitions that already exist in the target are expected and do not count. A definition that was selected,
    /// does not exist in the target and was then filtered out (for example because a task is not installed or a task
    /// group or variable group mapping is missing) is skipped. A definition that was sent to the target but is not in
    /// the mappings the target returned was rejected.
    /// </remarks>
    public class DefinitionMigrationOutcome
    {
        /// <summary>Number of selected source definitions.</summary>
        public int Selected { get; }

        /// <summary>Number of selected source definitions that already exist in the target, by name.</summary>
        public int Existing { get; }

        /// <summary>Number of definitions sent to the target.</summary>
        public int Planned { get; }

        /// <summary>Number of definitions the target created.</summary>
        public int Created { get; }

        /// <summary>Number of definitions that do not exist in the target and were filtered out before being sent.</summary>
        public int Skipped => Selected - Existing - Planned;

        /// <summary>Number of definitions sent to the target that it did not create.</summary>
        public int Failed => Planned - Created;

        public bool HasFailures => Skipped > 0 || Failed > 0;

        private DefinitionMigrationOutcome(int selected, int existing, int planned, int created)
        {
            Selected = selected;
            Existing = existing;
            Planned = planned;
            Created = created;
        }

        /// <param name="selected">Selected source definitions.</param>
        /// <param name="notInTarget">Selected source definitions that do not already exist in the target.</param>
        /// <param name="planned">Definitions sent to the target after every filter.</param>
        /// <param name="created">Mappings the target returned for the definitions sent.</param>
        public static DefinitionMigrationOutcome Create(int selected, int notInTarget, int planned, int created)
        {
            return new DefinitionMigrationOutcome(selected, selected - notInTarget, planned, created);
        }
    }
}
