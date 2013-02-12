
namespace ImagingService.Configuration
{
    public class ImageVariantProperties
    {
        public string Description { get; set; }
        public string NameAddition { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string TargetFormat { get; set; }
        public int ReplacementColour { get; set; }
        public ScaleBy ScaleBy { get; set; }
    }
}
