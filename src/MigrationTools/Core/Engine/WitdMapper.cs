using System;
using MigrationTools.Core.Engine;

namespace MigrationTools.Core.Engine
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