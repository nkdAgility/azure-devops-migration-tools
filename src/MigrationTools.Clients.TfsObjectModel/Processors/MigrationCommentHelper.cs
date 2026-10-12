namespace MigrationTools.Processors
{
    /// <summary>
    /// Builds the History text of the last replayed revision when GenerateMigrationComment is on (#3192).
    /// The comment is added to that revision instead of being saved in a separate later revision.
    /// </summary>
    public static class MigrationCommentHelper
    {
        public static string Comment(object reflectedUri)
        {
            return $"This work item was migrated from a different project or organization. You can find the old version at <a href=\"{reflectedUri}\">{reflectedUri}</a>.";
        }

        /// <summary>Appends the migration comment to the revision's own History, keeping the source comment first.</summary>
        public static string AppendTo(string revisionHistory, object reflectedUri)
        {
            var comment = Comment(reflectedUri);
            if (string.IsNullOrWhiteSpace(revisionHistory))
            {
                return comment;
            }
            if (revisionHistory.Contains(comment))
            {
                return revisionHistory;
            }
            return revisionHistory + "<br><br>" + comment;
        }
    }
}
