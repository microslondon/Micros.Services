using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using ImagingService.Configuration;

namespace ImagingService.ImageProcessing
{
    public class LegacyImageProcessor : ImageProcessorBase
    {
         public LegacyImageProcessor()
        {
        }

         public LegacyImageProcessor(Image sourceImage, string sourceImagePath, ClientConfiguration clientConfiguration)
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

                    WriteImage(destinationFileName, imageVariant, 0);
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

        
        /// <summary>
        /// Perform the image transformation on the bitmap
        /// </summary>
        public void WriteImage(string destinationPathName, ImageVariantProperties imageManipulationCriteria, int targetFileSize)
        {
            var scaleBy = imageManipulationCriteria.ScaleBy;
            var destinationImageSize = new Size(imageManipulationCriteria.Width, imageManipulationCriteria.Height);
            const int compressionQuality = 100;

            var sourceBitmap = new Bitmap(SourceImage);

            Size destSize = destinationImageSize;

            double maxDest;
            double maxSource;

            switch (scaleBy)
            {
                case (ScaleBy.Width):
                    maxSource = sourceBitmap.Width;
                    maxDest = destSize.Width;
                    break;

                case (ScaleBy.Height):
                    maxSource = sourceBitmap.Height;
                    maxDest = destSize.Height;
                    break;

                default:
                    if ((destSize.Width / (double)sourceBitmap.Width) < (destSize.Height / (double)sourceBitmap.Height))
                    {
                        maxSource = sourceBitmap.Width;
                        maxDest = destSize.Width;
                    }
                    else
                    {
                        maxSource = sourceBitmap.Height;
                        maxDest = destSize.Height;
                    }
                    break;
            }

            if (maxSource > maxDest)
            {
                double scalingFactor = maxDest / maxSource;
                double newWidth = sourceBitmap.Width * scalingFactor;
                double newHeight = sourceBitmap.Height * scalingFactor;
                destSize.Width = (int)newWidth;
                destSize.Height = (int)newHeight;
            }
            else
            {
                destSize = sourceBitmap.Size;
            }

            Bitmap destBitmap;
            if (destSize.Width != sourceBitmap.Width)
            {
                destBitmap = new Bitmap(destSize.Width, destSize.Height, PixelFormat.Format24bppRgb);

                Graphics scaledImage = Graphics.FromImage(destBitmap);
                scaledImage.InterpolationMode = InterpolationMode.HighQualityBicubic;

                // Draw the image outside dest bitmap to get rid of border created by
                // bicubic interpolation
                scaledImage.DrawImage(sourceBitmap, -1, -1, destSize.Width + 2, destSize.Height + 2);
            }
            else
            {
                destBitmap = sourceBitmap;
            }

            ImageCodecInfo codecInfo = EncoderForFile(destinationPathName);
            if (codecInfo == null)
            {
                throw new ArgumentException(String.Format("Encoder not available for files with extension '{0}'.", Path.GetExtension(destinationPathName)), "destinationPathName");
            }

            // Create an Encoder object based on the GUID
            // for the Quality parameter category.
            Encoder qualityEncoder = Encoder.Quality;

            double currentQuality = (double)compressionQuality;

            EncoderParameters qualityEncoderParameters = new EncoderParameters(1);
            EncoderParameter qualityEncoderParameter = new EncoderParameter(qualityEncoder, (long)currentQuality);
            qualityEncoderParameters.Param[0] = qualityEncoderParameter;

            long actualSize;
            byte[] destBytes;
            using (MemoryStream destStream = new MemoryStream())
            {
                destBitmap.Save(destStream, codecInfo, qualityEncoderParameters);
                actualSize = destStream.Length; // size of file may not be our target
                destBytes = destStream.GetBuffer(); // bytes to go in file
            }

            bool targetFileSizeSpecified = (targetFileSize > 0);
            if (targetFileSizeSpecified)
            { // refine quality parameter to get target file size
                double previousQuality = 0.0;
                long previousSize = 0;
                for (int attempts = 0; attempts < 5; attempts++)
                {
                    double sizeDifference = targetFileSize - actualSize;
                    if ((Math.Abs(sizeDifference) / targetFileSize) < 10.0 / 100.0)
                    {
                        // We're within 10%, which is fine.
                        break;
                    }

                    double deltaRatio = ((double)(targetFileSize - actualSize)) / Math.Abs((double)(actualSize - previousSize));
                    double newQuality = currentQuality + Math.Abs(currentQuality - previousQuality) * deltaRatio; // move distance proportional to error
                    newQuality = Math.Min(100.0, Math.Max(1.0, newQuality)); // clamp to 1...100

                    qualityEncoderParameters = new EncoderParameters(1);
                    qualityEncoderParameter = new EncoderParameter(qualityEncoder, (long)newQuality);
                    qualityEncoderParameters.Param[0] = qualityEncoderParameter;

                    previousQuality = currentQuality;
                    previousSize = actualSize;
                    currentQuality = newQuality;

                    using (MemoryStream destStream = new MemoryStream())
                    {
                        destBitmap.Save(destStream, codecInfo, qualityEncoderParameters);
                        actualSize = destStream.Length; // size of file may not be our target
                        destBytes = destStream.GetBuffer(); // bytes to go in file
                    }
                }
            }

            using (FileStream destFileStream = new FileStream(destinationPathName, FileMode.Create))
            {
                destFileStream.Write(destBytes, 0, (int)actualSize); // Write data to file
            }

        }

        /// <summary>
        /// Encoder for file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns></returns>
        private ImageCodecInfo EncoderForFile(string filename)
        {
            string requiredExtension = "*" + Path.GetExtension(filename).ToLower(); // e.g., *.jpg
            ImageCodecInfo[] encoders = ImageCodecInfo.GetImageEncoders();
            for (int i = 0; i < encoders.Length; i++)
            {
                string[] encoderExtensions = encoders[i].FilenameExtension.Split(';');
                for (int j = 0; j < encoderExtensions.Length; j++)
                {
                    if (encoderExtensions[j].ToLower() == requiredExtension)
                    {
                        return encoders[i];
                    }
                }
            }
            return null;
        }

    }
}
