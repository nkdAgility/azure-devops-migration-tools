using System;

namespace MigrationTools.Configuration
{
    public class TeamProjectConfig : IMigrationClientConfig
    {
        public Uri Collection { get; set; }
        public string Project { get; set; }
        public string ReflectedWorkItemIDFieldName { get; set; }
        public bool AllowCrossProjectLinking { get; set; }
        public string PersonalAccessToken { get; set; }
        public LanguageMaps LanguageMaps { get; set; }

        public Type MigrationClient { get { return MigrationClient; } }

        public IMigrationClientConfig PopulateWithDefault()
        {
            Project = "myProjectName";
            AllowCrossProjectLinking = false;
            Collection = new Uri("https://dev.azure.com/nkdagility-preview/");
            ReflectedWorkItemIDFieldName = "Custom.ReflectedWorkItemId";
            PersonalAccessToken = "";
            LanguageMaps = new LanguageMaps() { AreaPath = "Area", IterationPath = "Iteration" };
            return this;
        }

        public override string ToString()
        {
            return string.Format("{0}/{1}", Collection, Project);
        }
    }

    public class LanguageMaps
    {
        public string AreaPath { get; set; }
        public string IterationPath { get; set; }
    }
}