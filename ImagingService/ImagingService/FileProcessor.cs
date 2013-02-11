using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;


namespace ImagingService
{
    public class FileProcessor
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string ProcessedPath { get; set; }

        public FileProcessor (string sourcePath, string destinationPath, string processedPath)
        {
            SourcePath = sourcePath;
            DestinationPath = destinationPath;
            ProcessedPath = processedPath;
        }

        public void Process ()
        {
            var imageProcessors = LoadImageProcessors(SourcePath, DestinationPath, ProcessedPath);
            imageProcessors.AsParallel().ForAll(processor => processor.ProcessImage());
        }

        public static List<ImageProcessor> LoadImageProcessors(string sourcePath, string destinationPath, string processedPath)
        {
            var files = Directory.GetFiles(sourcePath, "*.*", SearchOption.AllDirectories);
            return files.Select(file =>new ImageProcessor(ImageFromFile(file), file, CreateTestVariants(), destinationPath, processedPath)).ToList();
        }

        private static Image ImageFromFile(string file)
        {
            try
            {
                var image = Image.FromStream(new MemoryStream(File.ReadAllBytes(file)));
                return image.Width > 0 && image.Height > 0 ? image : null;
            }
            catch
            {
                // TO Do: Logging File is invalid / incomplete
            }

            return null;
        }

        private static List<ImageVariantProperties> CreateTestVariants()
        {
            return new List<ImageVariantProperties>
                       {
                           new ImageVariantProperties
                               {
                                   Description = "Variant 1",
                                   Height = 50,
                                   Width = 50,
                                   NameAddition = "_p",
                                   ReplacementColour = -1,
                                   ScaleBy = ScaleBy.Width,
                                   TargetFormat = "jpg"
                               },
                           new ImageVariantProperties
                               {
                                   Description = "Variant 2",
                                   Height = 100,
                                   Width = 100,
                                   NameAddition = "_r",
                                   ReplacementColour = -1,
                                   ScaleBy = ScaleBy.Width,
                                   TargetFormat = "jpg"
                               },
                           new ImageVariantProperties
                               {
                                   Description = "Variant 3",
                                   Height = 200,
                                   Width = 200,
                                   NameAddition = "_s",
                                   ReplacementColour = -1,
                                   ScaleBy = ScaleBy.Width,
                                   TargetFormat = "jpg"
                               },
                           new ImageVariantProperties
                               {
                                   Description = "Variant 4",
                                   Height = 400,
                                   Width = 400,
                                   NameAddition = "_t",
                                   ReplacementColour = -1,
                                   ScaleBy = ScaleBy.Width,
                                   TargetFormat = "jpg"
                               },
                           new ImageVariantProperties
                               {
                                   Description = "Variant 5",
                                   Height = 800,
                                   Width = 800,
                                   NameAddition = "_u",
                                   ReplacementColour = -1,
                                   ScaleBy = ScaleBy.Width,
                                   TargetFormat = "jpg"
                               }
                       };

        }

    }
}
