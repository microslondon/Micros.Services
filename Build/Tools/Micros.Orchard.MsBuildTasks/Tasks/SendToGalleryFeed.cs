using System;
using System.IO;
using System.Linq;

using Gallery.Core.Domain;

using Micros.Orchard.MsBuildTasks.Packaging;

using Microsoft.Build.Framework;
using Microsoft.Http;

namespace Micros.Orchard.MsBuildTasks.Tasks
{
    public class SendToGalleryFeed : ITask
    {
        public IBuildEngine BuildEngine { get; set; }
        public ITaskHost HostObject { get; set; }

        public string ProjectPrefix { get; set; }
        public string FeedUrl { get; set; }
        public string WebDirectoryPath { get; set; }
        public string WorkingDirectory { get; set; }

        public bool Execute() 
        {
            var packageSubjects = new PackageSearch(BuildEngine).FindPackageSubjects(WebDirectoryPath, ProjectPrefix).ToList();
            
            foreach (var subject in packageSubjects)
                BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Found " + subject.Type + " " + subject.Name + " version " + subject.Version, string.Empty, "Packages", MessageImportance.Normal));

            var galleryFeedService = new GalleryFeedService(FeedUrl);
            var packageCreator = new OrchardPackageCreator(WorkingDirectory, WebDirectoryPath);

            foreach (var subject in packageSubjects)
            {
                if (!galleryFeedService.DoesPackageExistInFeed(subject))
                {
                    try
                    {
                        packageCreator.CreatePackageFile(subject);
                    }
                    catch (Exception ex)
                    {
                        BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Error creating package for " + subject.Name + " - " + ex.Message, string.Empty, "Packages", MessageImportance.Normal));
                        return false;
                    }

                    HttpResponseMessage httpResponse = null;
                    Package package;

                    using (var stream = new FileStream(WorkingDirectory + "\\Orchard." + subject.Type + "." + subject.Name + "." + subject.Version + ".nupkg", FileMode.Open))
                    {
                        package = galleryFeedService.UploadPackageToFeed(subject, stream, out httpResponse);
                    }

                    if (package == null)
                    {
                        BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Failed to upload package " + subject.Name + " with response: " + httpResponse, string.Empty, "Packages", MessageImportance.Normal));
                        return false;
                    }

                    if (!galleryFeedService.TrySetPackageType(package, subject, out httpResponse))
                    {
                        BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Failed to set package type for " + subject.Name + " with response: " + httpResponse, string.Empty, "Packages", MessageImportance.Normal));
                        return false;
                    }

                    if (!galleryFeedService.TryCreateScreenshot(package, out httpResponse))
                    {
                        BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Failed to create screenshot for " + subject.Name + " with response: " + httpResponse, string.Empty, "Packages", MessageImportance.Normal));
                        return false;
                    }

                    if (!galleryFeedService.TryPublishPackage(package, out httpResponse))
                    {
                        BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Failed to publish package " + subject.Name + " with response: " + httpResponse, string.Empty, "Packages", MessageImportance.Normal));
                        return false;
                    }
                }
                else
                {
                    BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Package for " + subject.Name + " version " + subject.Version + " already exists in feed", string.Empty, "Packages", MessageImportance.Normal));
                    continue;
                }

                BuildEngine.LogMessageEvent(new BuildMessageEventArgs("Uploaded " + subject.Name + " to " + FeedUrl, string.Empty, "Packages", MessageImportance.Normal));
            }

            return true;
        }
    }
}
