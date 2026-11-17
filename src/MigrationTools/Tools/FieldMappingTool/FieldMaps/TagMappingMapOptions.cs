using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Need to rename tags on the way across? Each tag is renamed by the first entry in `mappings` whose `source` matches it; tags that match no entry are kept unchanged, and duplicates a rename creates are removed. A `source` matches the exact tag name (case-sensitive); start it with `regex:` to use a .NET regular expression, and the `target` may then use groups such as `$1`. A rename that would give an empty tag keeps the original. Run it before `TagFilterMap` with the same `ApplyTo`, so the filter only needs the new names: maps with `ApplyTo` `*` run before type-specific maps, and maps in one scope run in configuration order.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class TagMappingMapOptions : FieldMapOptions
    {
        /// <summary>
        /// The renames, applied in order; the first `source` that matches a tag wins.
        /// </summary>
        /// <default>[]</default>
        public List<TagMapping> mappings { get; set; } = new List<TagMapping>();

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            mappings = new List<TagMapping>()
            {
                new TagMapping { source = "Bug-Bash", target = "BugBash" },
                new TagMapping { source = "regex:^v1\\..*$", target = "Legacy" }
            };
        }
    }

    /// <summary>
    /// One rename for `TagMappingMap`.
    /// </summary>
    public class TagMapping
    {
        /// <summary>
        /// The tag to rename: an exact, case-sensitive tag name, or `regex:` followed by a .NET regular expression.
        /// </summary>
        public string source { get; set; }

        /// <summary>
        /// The new tag name. For a `regex:` source, the matched text is replaced with this value, so groups such as `$1` can be used; use `^` and `$` in the pattern to replace the whole tag.
        /// </summary>
        public string target { get; set; }
    }
}
