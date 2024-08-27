using System;
using Microsoft.Extensions.Options;
using MigrationTools._EngineV1.Configuration;

namespace MigrationTools._EngineV1.Containers
{
    public abstract class EngineContainer<TItemType>
    {
        private readonly IServiceProvider _services;
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


        protected EngineContainer(IServiceProvider services)
        {
            _services = services;
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