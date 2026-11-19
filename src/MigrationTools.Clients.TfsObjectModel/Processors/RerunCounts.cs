namespace MigrationTools.Processors
{
    /// <summary>
    /// What happened to each source work item in a run, for the summary at the end (#3299). An item is counted only once
    /// it has finished, so a failed item is counted as failed and nothing else.
    /// </summary>
    public class RerunCounts
    {
        public enum Outcome
        {
            None,
            Created,
            Updated,
            InSync,
        }

        private Outcome _pending;

        public int Created { get; private set; }
        public int Updated { get; private set; }
        public int InSync { get; private set; }
        public int Failed { get; private set; }

        public void Begin() => _pending = Outcome.None;

        public void Set(Outcome outcome) => _pending = outcome;

        public void Succeeded()
        {
            switch (_pending)
            {
                case Outcome.Created: Created++; break;
                case Outcome.Updated: Updated++; break;
                case Outcome.InSync: InSync++; break;
            }
            _pending = Outcome.None;
        }

        public void Fail()
        {
            Failed++;
            _pending = Outcome.None;
        }

        public override string ToString() =>
            $"{InSync} in sync and untouched, {Updated} updated, {Created} created, {Failed} failed";
    }
}
