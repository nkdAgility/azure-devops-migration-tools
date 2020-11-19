using System;

namespace MigrationTools.Host.Services
{
    public interface IDetectVersionService
    {
        Version GetLatestVersion();
        bool SkipUpdateCheck();
    }
}