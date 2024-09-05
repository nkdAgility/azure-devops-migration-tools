using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MigrationTools.Services.Shadows
{
    public class MeterFactoryFake : IMeterFactory
    {
        public Meter Create(MeterOptions options)
        {
            return new Meter(options);
        }

        public void Dispose()
        {
            // do nothing
        }
    }
}
