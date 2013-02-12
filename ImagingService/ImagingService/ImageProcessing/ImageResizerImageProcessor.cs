
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using ImageResizer;
using ImagingService.Configuration;

namespace ImagingService.ImageProcessing
{
    public class ImageResizerImageProcessor : ImageProcessorBase
    {
        public ImageResizerImageProcessor()
        {
        }

        public ImageResizerImageProcessor(Image sourceImage, string sourceImagePath, ClientConfiguration clientConfiguration)
        {
            SourceImage = sourceImage;
            SourceImageFilePath = sourceImagePath;
            ClientConfiguration = clientConfiguration;
        }

        public override bool ProcessImage()
        {
            try
            {
                var sourceImageFileName = Path.GetFileNameWithoutExtension(SourceImageFilePath);

                if (SourceImage == null || sourceImageFileName == null)
                    return false;

                var destinationFilePath = string.Format(FilePathFormat, ClientConfiguration.DestinationPath, sourceImageFileName[0], sourceImageFileName[1], sourceImageFileName[2]);
                Directory.CreateDirectory(destinationFilePath);

                foreach (var imageVariant in ClientConfiguration.ImageVariants)
                {
                    var destinationFileName = string.Format(FileNameFormat, destinationFilePath, sourceImageFileName, imageVariant.NameAddition, imageVariant.TargetFormat);

                    ImageBuilder.Current.Build(SourceImage, destinationFileName,
                                               new ResizeSettings
                                               {
                                                   MaxWidth = imageVariant.Width,
                                                   MaxHeight = imageVariant.Height,
                                                   Format = imageVariant.TargetFormat
                                               }, false);
                }

                SourceImage.Dispose();
                MoveImageToProcessedFolder(SourceImageFilePath, ClientConfiguration.ProcessedPath);
            }
            catch (Exception ex)
            {
                Trace.TraceError("{0}: Unable to process image {1}, exception: {2}", GetType(), SourceImageFilePath, ex.Message);
                return false;
            }

            return true;
        }
    }
}
