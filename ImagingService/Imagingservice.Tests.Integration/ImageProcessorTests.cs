
using System.Collections.Generic;
using ImagingService;
using ImagingService.Configuration;
using Xunit;

namespace Imagingservice.Tests
{
    public class ImageProcessorTests
    {
        private const string WpfImageProcessorType = "ImagingService.ImageProcessing.WpfImageProcessor, ImagingService";
        private const string ImageResizerImageProcessorType = "ImagingService.ImageProcessing.ImageResizerImageProcessor, ImagingService";
        private const string LegacyImagingProcessorType = "ImagingService.ImageProcessing.LegacyImagingProcessor, ImagingService";

        [Fact]
        public void SuccessfullyProcessLargeFilesWithImageResizer ()
        {
            const int numberOfLargeFiles = 4;
            var configuration = GetClientConfiguration(1, ImageResizerImageProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfLargeFiles, successfullyProcessedFilesCount);
        }

        [Fact]
        public void SuccessfullyProcessLargeFilesWithWpfProcessor()
        {
            const int numberOfLargeFiles = 4;
            var configuration = GetClientConfiguration(1, WpfImageProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfLargeFiles, successfullyProcessedFilesCount);
        }

        [Fact]
        public void SuccessfullyProcessLargeFilesWithLegacyImagingProcessor()
        {
            const int numberOfLargeFiles = 4;
            var configuration = GetClientConfiguration(1, LegacyImagingProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfLargeFiles, successfullyProcessedFilesCount);
        }
        
        [Fact]
        public void SuccessfullyProcessWierdShapedFilesWithImageResizer()
        {
            const int numberOfWierdShapedFiles = 3;
            var configuration = GetClientConfiguration(2, WpfImageProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfWierdShapedFiles, successfullyProcessedFilesCount);
        }

        [Fact]
        public void SuccessfullyProcessWierdShapedFilesWithWpfProcessor()
        {
            const int numberOfWierdShapedFiles = 3;
            var configuration = GetClientConfiguration(2, WpfImageProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfWierdShapedFiles, successfullyProcessedFilesCount);
        }

        [Fact(Skip = "Legacy processor doesn't handle images where width or hight is 1.")]
        public void SuccessfullyProcessWierdShapedFilesWithLegacyImagingProcessor()
        {
            const int numberOfWierdShapedFiles = 3;
            var configuration = GetClientConfiguration(2, LegacyImagingProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfWierdShapedFiles, successfullyProcessedFilesCount);
        }

        [Fact]
        public void SuccessfullyProcessLotsOfSmallImagesWithImageResizer()
        {
            const int numberOfWierdShapedFiles = 1080;
            var configuration = GetClientConfiguration(3, ImageResizerImageProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfWierdShapedFiles, successfullyProcessedFilesCount);
        }

        [Fact(Skip="It appears this is very innefficient when it comes to small files.")]
        public void SuccessfullyProcessLotsOfSmallImagesWithWpfProcessor()
        {
            const int numberOfWierdShapedFiles = 1080;
            var configuration = GetClientConfiguration(3, WpfImageProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfWierdShapedFiles, successfullyProcessedFilesCount);
        }

        [Fact(Skip = "Legacy processor doesn't handle images where width or hight is 1.")]
        public void SuccessfullyProcessLotsOfSmallImagesWithLegacyImagingProcessorType()
        {
            const int numberOfWierdShapedFiles = 1080;
            var configuration = GetClientConfiguration(3, LegacyImagingProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfWierdShapedFiles, successfullyProcessedFilesCount);
        }

        [Fact]
        public void DoNotProcessNonImageImageFile()
        {
            const int numberOfFiles = 0;
            var configuration = GetClientConfiguration(4, ImageResizerImageProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfFiles, successfullyProcessedFilesCount);
        }

        [Fact]
        public void DoNotProcessPartialImageFile()
        {
            const int numberOfFiles = 0;
            var configuration = GetClientConfiguration(5, ImageResizerImageProcessorType);
            var successfullyProcessedFilesCount = new FileProcessor(configuration).Process();
            Assert.Equal(numberOfFiles, successfullyProcessedFilesCount);
        }
        
        private static ClientConfiguration GetClientConfiguration (int testNumber, string imageProcessoryType)
        {
            var configuration = new ClientConfiguration
                                    {
                                        SourcePath = string.Format("..\\..\\ImageSource\\Test{0}", testNumber),
                                        DestinationPath = string.Format("..\\..\\ImageDestination\\Test{0}", testNumber),
                                        ProcessedPath = string.Format("..\\..\\ImageProcessed\\Test{0}", testNumber),
                                        SourceFilesSearchPattern = "*.*",
                                        CopyImagesToProcessedFolder = false,
                                        DeleteProcessedImages = false,
                                        ImageProcessorType = imageProcessoryType,
                                        ImageVariants = new List<ImageVariantProperties>
                                                            {
                                                                new ImageVariantProperties
                                                                    {
                                                                        Description = "Variant 1",
                                                                        Height = 50,
                                                                        Width = 50,
                                                                        NameAddition = "_p",
                                                                        ReplacementColour = -1,
                                                                        ScaleBy = ScaleBy.Both,
                                                                        TargetFormat = "jpg"
                                                                    },
                                                                new ImageVariantProperties
                                                                    {
                                                                        Description = "Variant 2",
                                                                        Height = 1280,
                                                                        Width = 1280,
                                                                        NameAddition = "_t",
                                                                        ReplacementColour = -1,
                                                                        ScaleBy = ScaleBy.Both,
                                                                        TargetFormat = "jpg"
                                                                    },
                                                                new ImageVariantProperties
                                                                    {
                                                                        Description = "Variant 3",
                                                                        Height = 4320,
                                                                        Width = 7680,
                                                                        NameAddition = "_u",
                                                                        ReplacementColour = -1,
                                                                        ScaleBy = ScaleBy.Both,
                                                                        TargetFormat = "jpg"
                                                                    }
                                                            }
                                    };

            return configuration;
        }
    }
}
