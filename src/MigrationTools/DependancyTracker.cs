using System;
using System.Diagnostics;

namespace MigrationTools
{
    public class DependancyTracker : IDisposable
    {
        private readonly Stopwatch timer;
        private DateTime startTime;

        private readonly Action<TimeSpan> f;

        public DependancyTracker(Action<TimeSpan> f)
        {
            this.f = f;
            timer = Stopwatch.StartNew();
            startTime = DateTime.UtcNow;
        }

        public void Dispose()
        {
            timer.Stop();
            f(timer.Elapsed);
        }
    }
}