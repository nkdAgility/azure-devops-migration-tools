using Microsoft.Extensions.Hosting;
using MigrationTools.Core.Configuration;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;

namespace MigrationTools.Core.Engine.Containers
{
    public abstract class EngineContainer<TItemType>
    {
        private readonly IHost _Host;
        private readonly EngineConfiguration _Config;

        public abstract TItemType Items
        { get; }

        public static implicit operator TItemType(EngineContainer<TItemType> ecType)
        {
            return ecType.Items;
        }

        protected IHost Host
        {
            get { return _Host; }
        }

        protected EngineConfiguration Config
        {
            get { return _Config; }
        }


        public EngineContainer(IHost host, EngineConfiguration config)
        {
            _Host = host;
            _Config = config;
            Configure();
        }
        protected abstract void Configure();
    }
}
