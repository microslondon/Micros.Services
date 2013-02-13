using System.Collections.Generic;
using System.IO;
using System.Linq;

using Microsoft.Build.Framework;

namespace Micros.Orchard.MsBuildTasks.Packaging
{
    internal class PackageSearch
    {
        private readonly IBuildEngine buildEngine;

        public PackageSearch(IBuildEngine buildEngine)
        {
            this.buildEngine = buildEngine;
        }

        public IEnumerable<PackageInfo> FindPackageSubjects(string rootSearchDirectory, string projectPrefix)
        {
            this.buildEngine.LogMessageEvent(
                new BuildMessageEventArgs(
                    "Finding subjects to package in directory: " + rootSearchDirectory,
                    string.Empty,
                    "Packages",
                    MessageImportance.Normal));

            foreach (var directory in Directory.EnumerateDirectories(rootSearchDirectory, projectPrefix + ".*", SearchOption.AllDirectories))
            {
                var directoryInfo = new DirectoryInfo(directory);
                var packageSubject = new PackageInfo
                    {
                        Name = directoryInfo.Name,
                        Type = directoryInfo.Parent.Name == "Modules" ? PackageType.Module : PackageType.Theme,
                        Location = directoryInfo.FullName
                    };

                var version = this.GetVersionFromDescriptorFile(directoryInfo, packageSubject.Type);

                if (!string.IsNullOrEmpty(version))
                {
                    packageSubject.Version = version;
                    yield return packageSubject;
                }
            }
        }

        private string GetVersionFromDescriptorFile(DirectoryInfo directoryInfo, PackageType type)
        {
            var descriptorFileName = type + ".txt";
            var descriptorFile = directoryInfo.GetFiles(descriptorFileName).FirstOrDefault();

            if (descriptorFile == null) return null;

            var lines = File.ReadAllLines(descriptorFile.FullName);
            var versionLine = lines.FirstOrDefault(line => line.Contains("Version:"));
            return !string.IsNullOrEmpty(versionLine)
                ? versionLine.Replace("Version:", string.Empty).Trim()
                : null;
        }
    }
}
