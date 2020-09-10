using System;

namespace MigrationTools.Services
{
    public interface IDetectVersionService
    {
        Version GetLatestVersion();
    }
}