using System.Collections.Generic;
using MigrationTools.Tools.Infrastructure;

namespace MigrationTools.Tools
{
    /// <summary>
    /// Only want approved tags on the target? This keeps the tags you list in `includeTags`, always drops the tags in `excludeTags`, and logs every tag it drops. An entry matches the exact tag name (case-sensitive); start it with `regex:` to use a .NET regular expression. Run it after every field map that adds tags, with the same `ApplyTo`: maps with `ApplyTo` `*` run before type-specific maps, and maps in one scope run in configuration order.
    /// </summary>
    /// <status>ready</status>
    /// <processingtarget>Work Item Field</processingtarget>
    public class TagFilterMapOptions : FieldMapOptions
    {
        /// <summary>
        /// The approved tags. When it has entries, only tags that match one of them are kept. When it is empty, every tag is kept unless it matches `excludeTags`.
        /// </summary>
        /// <default>[]</default>
        public List<string> includeTags { get; set; } = new List<string>();

        /// <summary>
        /// The blocked tags. A tag that matches one of them is always dropped, even when it also matches `includeTags`.
        /// </summary>
        /// <default>[]</default>
        public List<string> excludeTags { get; set; } = new List<string>();

        public void SetExampleConfigDefaults()
        {
            ApplyTo = new List<string>() { "*" };
            includeTags = new List<string>() { "Approved", "Release", "regex:^Team-" };
            excludeTags = new List<string>() { "Team-Legacy" };
        }
    }
}
