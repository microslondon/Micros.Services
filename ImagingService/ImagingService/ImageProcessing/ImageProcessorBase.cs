
using System.Drawing;
using System.IO;
using ImagingService.Configuration;

namespace ImagingService.ImageProcessing
{
    public abstract class ImageProcessorBase : IImageProcessor
    {
        protected const string FileNameFormat = "{0}\\{1}{2}.{3}";
        protected const string FilePathFormat = "{0}\\{1}\\{2}\\{3}";

        public Image SourceImage { get; set; }
        public string SourceImageFilePath { get; set; }
        public ClientConfiguration ClientConfiguration { get; set; }

        public abstract bool ProcessImage();

        protected static void MoveImageToProcessedFolder(string sourceImagePath, string processedPath)
        {
            File.Copy(sourceImagePath, string.Format("{0}\\{1}", processedPath, Path.GetFileName(sourceImagePath)), true);
            File.Delete(sourceImagePath);
        }
    }
}
