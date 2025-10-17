using Task7.Models;
using Task7.Delegates;
using System.Security.Cryptography.X509Certificates;
namespace Task7
{
    public class ImageProcessor
    {
        public FilterHandler FilterHandler { get; set; }

        private const int BrightnessStep = 10;
        private const int ContrastStep = 5;

        public void Run(ImageContext ctx)
        {
            Check(ctx);
            FilterHandler?.Invoke(ctx);
        }

        public void IncreaseBrightness(ImageContext context)
        {
            Check(context);
            context.Brightness += BrightnessStep;
        }
        public void IncreaseContrast(ImageContext context)
        {
            Check(context);
            context.Contrast += ContrastStep;
        }

        public void Check(ImageContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
        }
        
    }
}