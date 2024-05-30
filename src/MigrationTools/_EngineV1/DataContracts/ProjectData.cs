using System;

namespace MigrationTools._EngineV1.DataContracts
{
    public class ProjectData
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public object internalObject { get; set; }
        public string Url { get; set; }
        public Guid Guid { get; set; }
    }
}