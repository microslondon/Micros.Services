
using System.Drawing;
using ImagingService.Configuration;

namespace ImagingService.ImageProcessing
{
    public interface IImageProcessor
    {
        Image SourceImage { get; set; }
        string SourceImageFilePath { get; set; }
        ClientConfiguration ClientConfiguration { get; set; }

        bool ProcessImage();
    }
}
