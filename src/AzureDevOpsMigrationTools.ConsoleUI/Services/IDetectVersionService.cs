using System;

namespace AzureDevOpsMigrationTools
{
    public interface IDetectVersionService
    {
        Version GetLatestVersion();
    }
}