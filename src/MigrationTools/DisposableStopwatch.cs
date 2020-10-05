using System;
using System.Diagnostics;

namespace MigrationTools
{
    public class DisposableStopwatch : IDisposable
    {
        private readonly Stopwatch sw;
        private readonly Action<TimeSpan> f;

        public DisposableStopwatch(Action<TimeSpan> f)
        {
            this.f = f;
            sw = Stopwatch.StartNew();
        }

        public void Dispose()
        {
            sw.Stop();
            f(sw.Elapsed);
        }
    }
}