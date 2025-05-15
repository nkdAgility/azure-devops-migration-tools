using MigrationTools.Processors.Infrastructure;

namespace MigrationTools.Processors
{
    public class TfsWorkItemOverwriteAreasAsTagsProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// This is a required parameter. That define the root path of the iteration. To get the full path use `\`
        /// </summary>
        /// <default>\</default>
        public string AreaIterationPath { get; set; }
    }
}
