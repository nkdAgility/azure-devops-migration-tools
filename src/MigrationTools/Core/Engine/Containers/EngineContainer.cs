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
        private readonly IServiceProvider _services;
        private readonly EngineConfiguration _Config;

        public abstract TItemType Items
        { get; }

        public static implicit operator TItemType(EngineContainer<TItemType> ecType)
        {
            return ecType.Items;
        }

        protected IServiceProvider Services
        {
            get { return _services; }
        }

        protected EngineConfiguration Config
        {
            get { return _Config; }
        }


        public EngineContainer(IServiceProvider services, EngineConfiguration config)
        {
            _services = services;
            _Config = config;
            Configure();
        }
        protected abstract void Configure();
    }
}
