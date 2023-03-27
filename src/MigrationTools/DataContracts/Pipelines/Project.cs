using System;
using System.Collections.Generic;
using System.Text;
using MigrationTools.DataContracts.Process;

namespace MigrationTools.DataContracts.Pipelines
{

    [ApiPath("projects")]
    [ApiName("Build Piplines")]
    public partial class Project : ISynchronizeable<Project>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public Uri Url { get; set; }

        public string State { get; set; }

        public long Revision { get; set; }

        public string Visibility { get; set; }

        public DateTimeOffset LastUpdateTime { get; set; }

        public Project CloneAsNew()
        {
            return new Project()
            {
                Id = Id,
                Name = Name,
                Url = Url,
                State = State,
                Revision = Revision,
                Visibility = Visibility,
            };
        }

        public void UpdateWithExisting(Project existing)
        {
            Id = existing.Id;
            Name = existing.Name;
            Url = existing.Url;
            State = existing.State;
            Revision = existing.Revision;
            Visibility = existing.Visibility;
        }
    }
}
