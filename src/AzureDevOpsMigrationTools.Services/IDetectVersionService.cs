using System;

namespace AzureDevOpsMigrationTools.Services
{
    public interface IDetectVersionService
    {
        Version GetLatestVersion();
    }
}