using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using ImagingService.Configuration;
using ImagingService.ImageProcessing;

namespace ImagingService
{
    public class FileProcessor
    {
        public ClientConfiguration ClientConfiguration { get; set; }

        public FileProcessor(ClientConfiguration clientConfiguration)
        {
            ClientConfiguration = clientConfiguration;
        }

        public int Process ()
        {
            var imageProcessors = LoadImageProcessors(ClientConfiguration);
            return imageProcessors.AsParallel().Count(processor => processor.ProcessImage());
        }

        public static IEnumerable<IImageProcessor> LoadImageProcessors(ClientConfiguration clientConfiguration)
        {
            var imageProcessorType = Type.GetType(clientConfiguration.ImageProcessorType);

            if (imageProcessorType == null)
                return new List<IImageProcessor>();

            var files = Directory.GetFiles(clientConfiguration.SourcePath, clientConfiguration.SourceFilesSearchPattern, SearchOption.AllDirectories);
            
            Trace.WriteLine(string.Format("{0} files found", files.Count()));
            
            return files.Select(filePath => CreateImageProcessor(filePath, imageProcessorType, clientConfiguration));
        }

        private static Image ImageFromFile(string file)
        {
            try
            {
                var image = Image.FromStream(new MemoryStream(File.ReadAllBytes(file)));
                return image.Width > 0 && image.Height > 0 ? image : null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Unable to parse an image from file: {0}, exception: {1}", file, ex);
            }

            return null;
        }

        private static IImageProcessor CreateImageProcessor (string filePath, Type imageProcessorType, ClientConfiguration clientConfiguration)
        {
            var imageProcessor = (IImageProcessor)Activator.CreateInstance(imageProcessorType);

            imageProcessor.SourceImageFilePath = filePath;
            imageProcessor.SourceImage = ImageFromFile(filePath);
            imageProcessor.ClientConfiguration = clientConfiguration;

            return imageProcessor;
        }
    }
}
