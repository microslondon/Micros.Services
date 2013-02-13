using System.Linq;

namespace Micros.Orchard.MsBuildTasks.Packaging
{
    internal class PackageInfo
    {
        public string Name { get; set; }

        public string Version { get; set; }

        public string Location { get; set; }

        public PackageType Type { get; set; }

        public string GetGalleryFeedVersion()
        {
            var versionNumber = this.Version;
            for (var i = this.Version.Count(c => c == '.'); i < 3; i++)
                versionNumber += ".0";

            return versionNumber;
        }

        public string GetPackageId()
        {
            return "Orchard." + this.Type + "." + this.Name;
        }
    }
}
