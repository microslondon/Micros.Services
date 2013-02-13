using System.Diagnostics;
using System.IO;

namespace Micros.Orchard.MsBuildTasks.Packaging
{
    internal class OrchardPackageStreamGenerator
    {
        private readonly string workingDirectory;

        private readonly string orchardWebDirectory;

        public OrchardPackageStreamGenerator(string workingDirectory, string orchardWebDirectory)
        {
            this.workingDirectory = workingDirectory;
            this.orchardWebDirectory = orchardWebDirectory;
        }

        public FileStream GetFileStreamForPackage(PackageInfo packageInfo)
        {
            this.GenerateOrchardPackage(packageInfo);
            return new FileStream(this.workingDirectory + "\\" + packageInfo.GetPackageId() + "." + packageInfo.Version + ".nupkg", FileMode.Open);
        }

        private void GenerateOrchardPackage(PackageInfo packageInfo)
        {
            var process = new Process
                {
                    StartInfo =
                        new ProcessStartInfo(this.orchardWebDirectory + "\\bin\\" + "orchard.exe")
                            {
                                CreateNoWindow = false,
                                WorkingDirectory = this.orchardWebDirectory + "\\bin",
                                Arguments = "package create " + packageInfo.Name + " " + this.workingDirectory,
                                RedirectStandardInput = true,
                                RedirectStandardOutput = true,
                                UseShellExecute = false
                            }
                };

            process.Start();

            process.WaitForExit();
            process.Close();
        }
    }
}
