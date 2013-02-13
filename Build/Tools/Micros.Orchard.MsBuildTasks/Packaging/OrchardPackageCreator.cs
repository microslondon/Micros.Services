using System.Diagnostics;

namespace Micros.Orchard.MsBuildTasks.Packaging
{
    internal class OrchardPackageCreator
    {
        private readonly string workingDirectory;

        private readonly string orchardWebDirectory;

        public OrchardPackageCreator(string workingDirectory, string orchardWebDirectory)
        {
            this.workingDirectory = workingDirectory;
            this.orchardWebDirectory = orchardWebDirectory;
        }

        public void CreatePackageFile(PackageInfo packageInfo)
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
