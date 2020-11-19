using System;
using System.Collections.Generic;
using MigrationTools.Endpoints;

namespace MigrationTools.Processors
{
    /// <summary>
    /// The `TfsSharedQueryProcessor` enabled you to migrate queries from one locatio nto another.
    /// </summary>
    public class TfsSharedQueryProcessorOptions : ProcessorOptions
    {
        /// <summary>
        /// The name of the shared folder, setting the default name
        /// </summary>
        private string sharedFolderName = "Shared Queries";

        /// <summary>
        /// Do we add the source project name into the folder path
        /// </summary>
        /// <default>false</default>
        public bool PrefixProjectToNodes { get; set; }

        /// <summary>
        /// The name of the shared folder, made a parameter incase it every needs to be edited
        /// </summary>
        /// <default>Shared Queries</default>
        public string SharedFolderName
        {
            get { return sharedFolderName; }
            set { sharedFolderName = value; }
        }

        /// <summary>
        /// Mapping of the source to the target
        /// </summary>
        public Dictionary<string, string> SourceToTargetFieldMappings { get; set; }

        public override string Processor => nameof(TfsSharedQueryProcessor);

        public override Type ToConfigure => typeof(TfsSharedQueryProcessor);

        public override IProcessorOptions GetDefault()
        {
            return this;
        }

        public override void SetDefaults()
        {
            var e1 = new TfsEndpointOptions();
            e1.SetDefaults();
            e1.Project = "sourceProject";
            Source = e1;
            var e2 = new TfsEndpointOptions();
            e2.SetDefaults();
            e2.Project = "targetProject";
            Target = e2;
        }
    }
}