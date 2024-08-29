﻿using System;
using System.Collections.Generic;
using MigrationTools.Enrichers;
using MigrationTools.Options;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    public class StringManipulatorToolOptions : ToolOptions
    {

        /// <summary>
        /// Max number of chars in a string. Applied last, and set to 1000000 by default.
        /// </summary>
        /// <default>1000000</default>
        public int MaxStringLength { get; set; }

        /// <summary>
        /// List of regex based string manipulations to apply to all string fields. Each regex replacement is applied in order and can be enabled or disabled.
        /// </summary>
        /// <default>{}</default>
        public List<RegexStringManipulator> Manipulators { get; set; }
    }

    public class RegexStringManipulator
    {
        public bool Enabled { get; set; }
        public string Pattern { get; set; }
        public string Replacement { get; set; }
        public string Description { get; set; }
    }
}