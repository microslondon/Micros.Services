using System.IO;
using System.Net;
using System.Web.Script.Serialization;

using Gallery.Core.Domain;

using Microsoft.Http;

namespace Micros.Orchard.MsBuildTasks.Packaging
{
    internal class GalleryFeedService
    {
        private readonly string feedUrl;
        private const string accessKey = "3f83baa9-accb-43ce-b6cc-995c511613aa";
        private const string createUrl = "PackageFiles/" + accessKey + "/nupkg";
        private const string screenshotUrl = "Screenshots/" + accessKey;
        private const string publishUrl = "PublishedPackages/Publish";

        public GalleryFeedService(string feedUrl)
        {
            this.feedUrl = feedUrl;
        }

        public bool DoesPackageExistInFeed(PackageInfo packageInfo)
        {
            var packageUrl = "Packages/" + accessKey + "/" + packageInfo.GetPackageId() + "/" + packageInfo.GetGalleryFeedVersion();
            using (var client = new HttpClient(this.feedUrl))
            {
                var response = client.Head(packageUrl);

                if (response.StatusCode == HttpStatusCode.OK)
                    return true;

                if (response.StatusCode == HttpStatusCode.NotFound)
                    return false;
            }

            return false;
        }

        public Package UploadPackageToFeed(PackageInfo packageInfo, FileStream packageStream, out HttpResponseMessage response)
        {
            using (var client = new HttpClient(this.feedUrl))
            {
                response = client.Post(createUrl, HttpContent.Create(packageStream, "application/octet-stream", null));
                if (response.StatusCode == HttpStatusCode.OK)
                    return new JavaScriptSerializer().Deserialize<Package>(response.Content.ReadAsString());

                return null;
            }
        }

        public bool TrySetPackageType(Package package, PackageInfo packageInfo, out HttpResponseMessage response)
        {
            package.PackageType = packageInfo.Type.ToString();
            using (var client = new HttpClient(this.feedUrl))
            {
                var packageUrl = "Packages/" + accessKey + "/" + package.Id + "/" + package.Version;

                using (response = client.Put(packageUrl, HttpContentExtensions.CreateDataContract(package)))
                    return response.StatusCode == HttpStatusCode.OK;
            }
        }

        public bool TryCreateScreenshot(Package package, out HttpResponseMessage response)
        {
            using (var client = new HttpClient(this.feedUrl))
            {
                var screenshotContents =
                    HttpContentExtensions.CreateDataContract(
                        new Screenshot { PackageId = package.Id, PackageVersion = package.Version });

                using (response = client.Post(screenshotUrl, screenshotContents))
                    return response.StatusCode == HttpStatusCode.OK;
            }
        }

        public bool TryPublishPackage(Package package, out HttpResponseMessage response)
        {
            using (var client = new HttpClient(this.feedUrl))
            {
                client.DefaultHeaders.ContentType = "application/json";
                var postData = new { key = accessKey, id = package.Id, version = package.Version };

                using (response = client.Post(publishUrl, HttpContent.Create(new JavaScriptSerializer().Serialize(postData), "application/json; charset=utf-8")))
                    return response.StatusCode == HttpStatusCode.OK;
            }
        }
    }
}
