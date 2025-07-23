using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    /// <summary>
    /// Options for processor <see cref="TfsWorkItemTypeValidatorProcessor"/>.
    /// </summary>
    public class TfsWorkItemTypeValidatorProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// If set to <see langword="true"/>, migration process will stop if there are some validation errors.
        /// If set to <see langword="false"/>, migration process will continue, for example to support some other validation
        /// processors.
        /// Default value is <see langword="true"/>.
        /// </summary>
        public bool StopIfValidationFails { get; set; } = true;
    }
}
