using System;
using MigrationTools.Engine;

namespace MigrationTools.Engine.Containers
{
    public class WitMapper : IWitdMapper
    {
        private string _MapTo;

        public WitMapper(string mapTo)
        {
            this._MapTo = mapTo;
        }

        public string Map()
        {
            return _MapTo;
        }
    }
}