
using System.Collections.Generic;

namespace ImagingService.Configuration
{
    public class ClientConfiguration
    {
        public string SourcePath { get; set; }
        public string DestinationPath { get; set; }
        public string ProcessedPath { get; set; }
        public string SourceFilesSearchPattern { get; set; }
        public string ImageProcessorType { get; set; }
        public bool CopyImagesToProcessedFolder { get; set; }
        public bool DeleteProcessedImages { get; set; }
        public List<ImageVariantProperties> ImageVariants { get; set; }
    }
}
