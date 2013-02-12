﻿
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ImagingService.Configuration;
using Color = System.Drawing.Color;
using Size = System.Drawing.Size;

namespace ImagingService.ImageProcessing
{
    public class WpfImageProcessor : ImageProcessorBase
    {
        private const double Epsilon = 0.00001;
     
        public WpfImageProcessor ()
        {
        }

        public WpfImageProcessor(Image sourceImage, string sourceImagePath, ClientConfiguration clientConfiguration)
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
            
                // Transparent colour replacement is unlikely to differ between image variants. So for efficiency we are processing it once.
                var sourceImageBytes = ConvertImageToByteArray(SourceImage, ClientConfiguration.ImageVariants.First().ReplacementColour);

                var filePath = string.Format(FilePathFormat, ClientConfiguration.DestinationPath, sourceImageFileName[0], sourceImageFileName[1], sourceImageFileName[2]);
                Directory.CreateDirectory(filePath);

                foreach (var imageVariant in ClientConfiguration.ImageVariants)
                {
                    var processedImageBytes = ResizeAndCrop(sourceImageBytes, SourceImage.RawFormat.ToString().ToLower(), SourceImage.Size, imageVariant);
                    WriteImageToDisk(processedImageBytes, filePath, sourceImageFileName, imageVariant);
                }

                SourceImage.Dispose();
                MoveImageToProcessedFolder(SourceImageFilePath, ClientConfiguration.ProcessedPath);
            }
            catch (Exception  ex)
            {
                Trace.TraceError("{0}: Unable to process image {1}, exception: {2} ", GetType(), SourceImageFilePath, ex.Message);
                return false;
            }

            return true;
        }

        private static void WriteImageToDisk(byte[] processedImageBytes, string destinationPath, string sourceImageFileName, ImageVariantProperties imageVariant)
        {
            var destinationFileName = string.Format(FileNameFormat, destinationPath, sourceImageFileName, imageVariant.NameAddition, imageVariant.TargetFormat);
            File.WriteAllBytes(destinationFileName.ToLowerInvariant(), processedImageBytes);
        }

        private static byte[] ConvertImageToByteArray(Image sourceImage, int replacementColour)
        {
            var memoryStream = new MemoryStream();

            if (replacementColour >= 0)
            {
                var bitmap = ReplaceTransparentColour(sourceImage, replacementColour);
                bitmap.Save(memoryStream, sourceImage.RawFormat);
            }
            else
            {
                sourceImage.Save(memoryStream, sourceImage.RawFormat);
            }

            return memoryStream.ToArray();
        }

        private static Bitmap ReplaceTransparentColour (Image sourceImage, int replacementColour)
        {
            var newColour = Color.FromArgb(replacementColour);
            
            var bitmap = new Bitmap(sourceImage);

            for (var i = 0; i < bitmap.Width; i++)
                for (var j = 0; j < bitmap.Height; j++)
                    if (bitmap.GetPixel(i, j).A == 0)
                        bitmap.SetPixel(i, j, newColour);

            return bitmap;
        }

        private static byte[] ResizeAndCrop(byte[] image, string fileNameExtension, Size sourceImageSize, ImageVariantProperties imageVariant)
        {
            var widthConstrained = false;
            var heightConstrained = false;
            var isLandscape = sourceImageSize.Width / sourceImageSize.Height >= 1;
            var destinationImageSize = new Size(imageVariant.Width, imageVariant.Height);

            switch (imageVariant.ScaleBy)
            {
                case ScaleBy.Height:
                    heightConstrained = true;
                    break;
                case ScaleBy.Width:
                    widthConstrained = true;
                    break;
                case ScaleBy.Both:
                    widthConstrained = isLandscape;
                    heightConstrained = !isLandscape;
                    break;
                default:
                    throw new ApplicationException("Unexpected ScaleBy enumeration value.");
            }

            var scaleSize = GetScaleSize(sourceImageSize, destinationImageSize, widthConstrained, heightConstrained);

            var scaledImage = ScaleImage(image, scaleSize);
            var scaledImageSize = new Size(scaledImage.PixelWidth, scaledImage.PixelHeight);
            var rectangleToCrop = GetCropRectangle(scaledImageSize, destinationImageSize, widthConstrained, heightConstrained);

            var croppedImage = CropImage(scaledImage, rectangleToCrop);
            return GetEncodedImage(croppedImage, fileNameExtension, imageVariant.TargetFormat);
        }

        private static Size GetScaleSize(Size sourceImageSize, Size destinationImageSize, bool widthConstrained, bool heightConstrained)
        {
            if (widthConstrained)
                return new Size(destinationImageSize.Width, 0);

            if (heightConstrained)
                return new Size(0, destinationImageSize.Height);

            var heightRatio = (double)sourceImageSize.Height / destinationImageSize.Height;
            var widthRatio = (double)sourceImageSize.Width / destinationImageSize.Width;

            if (Math.Abs(heightRatio - widthRatio) < Epsilon)
                return new Size(destinationImageSize.Width, destinationImageSize.Height);

            return heightRatio < widthRatio ? new Size(0, destinationImageSize.Height) : new Size(destinationImageSize.Width, 0);
        }

        private static Int32Rect GetCropRectangle(Size sourceImageSize, Size destinationImageSize, bool widthConstrained, bool heightConstrained)
        {
            var x = 0;

            if (!widthConstrained && !heightConstrained)
                x = Math.Max(Convert.ToInt32(sourceImageSize.Width / 2 - destinationImageSize.Width / 2), 0);

            var width = heightConstrained ? sourceImageSize.Width : destinationImageSize.Width;
            var height = widthConstrained ? sourceImageSize.Height : destinationImageSize.Height;

            return new Int32Rect(x, 0, width, height);
        }

        private static BitmapImage ScaleImage(byte[] image, Size decodedPixelSize)
        {
            var bitmapImage = new BitmapImage();

            bitmapImage.BeginInit();

            if (decodedPixelSize.Width > 0)
                bitmapImage.DecodePixelWidth = decodedPixelSize.Width;

            if (decodedPixelSize.Height > 0)
                bitmapImage.DecodePixelHeight = decodedPixelSize.Height;

            bitmapImage.StreamSource = new MemoryStream(image);
            bitmapImage.CreateOptions = BitmapCreateOptions.PreservePixelFormat;
            bitmapImage.CacheOption = BitmapCacheOption.Default;

            bitmapImage.EndInit();

            return bitmapImage;
        }

        private static ImageSource CropImage(BitmapSource scaledBitmapImage, Int32Rect rectangle)
        {
            var croppedBitmap = new CroppedBitmap();

            croppedBitmap.BeginInit();

            croppedBitmap.Source = scaledBitmapImage;
            croppedBitmap.SourceRect = rectangle;

            croppedBitmap.EndInit();

            return croppedBitmap;
        }

        private static byte[] GetEncodedImage(ImageSource image, string fileExtension, string targetFormat)
        {
            byte[] result = null;
            BitmapEncoder encoder;

            var imageFormat = string.IsNullOrEmpty(targetFormat) ? fileExtension : targetFormat;

            switch (imageFormat.ToLower())
            {
                case "jpeg":
                case "jpg":
                case "pjpeg":
                    encoder = new JpegBitmapEncoder();
                    break;
                case "png":
                    encoder = new PngBitmapEncoder();
                    break;
                case "tif":
                case "tiff":
                    encoder = new TiffBitmapEncoder();
                    break;
                case "gif":
                    encoder = new GifBitmapEncoder();
                    break;
                default:
                    throw new ApplicationException("Content type not supported");
            }

            if (image is BitmapSource)
            {
                using (var memoryStream = new MemoryStream())
                {
                    encoder.Frames.Add(BitmapFrame.Create(image as BitmapSource));
                    encoder.Save(memoryStream);
                    memoryStream.Seek(0, SeekOrigin.Begin);
                    result = new byte[memoryStream.Length];

                    using (var binaryReader = new BinaryReader(memoryStream))
                    {
                        binaryReader.Read(result, 0, (int)memoryStream.Length);
                    }
                }
            }

            return result;
        }
    }
}