using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;


namespace MigrationTools.Services
{
    public interface IMigrationToolVersionInfo
    {
        string ProductVersion { get; }
        string FileVersion { get;}
        string GitTag { get; }
    }
    public class MigrationToolVersionInfo : IMigrationToolVersionInfo
    {
        public string ProductVersion { get; set; }
        public string FileVersion { get; set; }
        public string GitTag { get; set; }

        public MigrationToolVersionInfo()
        {
            FileVersionInfo myFileVersionInfo = FileVersionInfo.GetVersionInfo(Assembly.GetEntryAssembly()?.Location);
            ProductVersion = myFileVersionInfo.ProductVersion;
            FileVersion = myFileVersionInfo.FileVersion;
            try
            {
                GitTag = ThisAssembly.Git.Tag;
            }
            catch (Exception)
            {
                // Do nothing
            }
            
        }
    }

    public interface IMigrationToolVersion
    {
        (Version version, string PreReleaseLabel, string versionString) GetRunningVersion();
    }

    public class MigrationToolVersion : IMigrationToolVersion
    {
        private IMigrationToolVersionInfo _MigrationToolVersionInfo;

        public MigrationToolVersion()
        {
            _MigrationToolVersionInfo = new MigrationToolVersionInfo();
        }

        public MigrationToolVersion(IMigrationToolVersionInfo migrationToolVersionInfo)
        {
            _MigrationToolVersionInfo = migrationToolVersionInfo;
        }

        public (Version version, string PreReleaseLabel, string versionString) GetRunningVersion()
        {
            try
            {
                var matches = Regex.Matches(_MigrationToolVersionInfo.ProductVersion, @"^(?<major>0|[1-9]\d*)\.(?<minor>0|[1-9]\d*)\.(?<build>0|[1-9]\d*)(?:-((?<label>:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+(?<fullEnd>[0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$");
                Version version = new Version(_MigrationToolVersionInfo.FileVersion);
                string textVersion = "0.0.0-local";
                if (version.CompareTo(new Version(0, 0, 0, 0)) == 0)
                {
                    if (_MigrationToolVersionInfo.GitTag != null)
                    {
                        textVersion = _MigrationToolVersionInfo.GitTag.Replace("Preview", "Local").Replace("v", "");
                    }
                }
                else
                {
                    if (matches[0].Groups[1].Success)
                    {
                        textVersion = version.Major + "." + version.Minor + "." + version.Build + "-" + matches[0].Groups[1].Value;
                    }
                    else
                    {
                        textVersion = version.Major + "." + version.Minor + "." + version.Build;
                    }

                }
                return (version, matches[0].Groups[1].Value, textVersion);
            }
            catch (Exception ex)
            {
                return (new Version(0, 0, 0, 0), "ex", "0.0.0-ex");
            }
           
        }
    }

}
