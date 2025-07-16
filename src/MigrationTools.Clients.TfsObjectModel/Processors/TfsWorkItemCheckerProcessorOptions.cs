using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    public class TfsWorkItemCheckerProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// If set to <see langword="true"/>, the migration will continue even if some fields in the source work item
        /// do not exist in the target work item type. If set to <see langword="false"/>, the migration will stop with
        /// error in this case. Default value is <see langword="false"/>.
        /// </summary>
        public bool StopIfMissingFieldsInTarget { get; set; } = true;
    }
}
