using System;
using MigrationTools.Configuration;

namespace MigrationTools.Engine.Containers
{
    public abstract class EngineContainer<TItemType>
    {
        private readonly IServiceProvider _services;
        private readonly EngineConfiguration _Config;
        private bool _configured = false;

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
        }

        protected abstract void Configure();

        public void EnsureConfigured()
        {
            if (!_configured)
            {
                Configure();
                _configured = true;
            }
        }
    }
}