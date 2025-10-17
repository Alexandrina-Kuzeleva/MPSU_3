using System;
using Task7.Models;

namespace Task7
{
    public static class ImageInput
    {
        public static ImageContext ReadImage()
        {
            var image = new ImageContext();

            image.Brightness = ReadInt("Яркость", 50);
            image.Contrast = ReadInt("Контраст", 10);
            image.IsBlurred = ReadBool("Размытие (true/false): ", false);

            return image;
        }

        private static int ReadInt(string message, int defaultValue)
        {
            Console.WriteLine(message);
            string? input = Console.ReadLine();
            return int.TryParse(input, out int result) ? result : defaultValue;
        }

        private static bool ReadBool(string message, bool defaultValue)
        {
            Console.WriteLine(message);
            string? input = Console.ReadLine();
            return bool.TryParse(input, out bool result) ? result : defaultValue;
        }
    }
}
