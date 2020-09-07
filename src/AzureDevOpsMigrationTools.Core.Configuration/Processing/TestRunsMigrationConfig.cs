//using System;
//using System.Collections.Generic;

//namespace AzureDevOpsMigrationTools.Core.Configuration.Processing
//{
//    public class TestRunsMigrationConfig : ITfsProcessingConfig
//    {
//        public string Status
//        {
//            get { return "Experimental"; }
//        }

//        public bool Enabled { get; set; }

//        public string Processor
//        {
//            get { return "TestRunsMigrationContext); }
//        }

//        /// <inheritdoc />
//        public bool IsProcessorCompatible(IReadOnlyList<ITfsProcessingConfig> otherProcessors)
//        {
//            return true;
//        }
//    }
//}