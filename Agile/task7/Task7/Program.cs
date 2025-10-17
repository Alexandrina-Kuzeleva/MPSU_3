using System;
using Task7.Models;
using Task7.Delegates;
namespace Task7
{
    class Program {
        static void Main()
        {
            Console.WriteLine("TESTS\n");

            var processor = new ImageProcessor();
            var image = new ImageContext
            {
                Brightness = 50,
                Contrast = 10,
                IsBlurred = false,
            };

            FilterHandler blurFilter = (context) => context.IsBlurred = true;
                        
            processor.FilterHandler += processor.IncreaseBrightness;
            processor.FilterHandler += processor.IncreaseContrast;
            processor.FilterHandler += blurFilter;
            
            Console.WriteLine($"До обработки.\nЯркость: {image.Brightness}, Контраст: {image.Contrast}, Размытие: {image.IsBlurred}");

            processor.Run(image);

            Console.WriteLine($"После.\nЯркость: {image.Brightness}, Контраст: {image.Contrast}, Размытие: {image.IsBlurred}");
            Console.WriteLine($"\nCброс значений.");

            image.Brightness = 50;
            image.Contrast = 10;
            image.IsBlurred = false;

            Console.WriteLine($"До обработки.\nЯркость: {image.Brightness}, Контраст: {image.Contrast}, Размытие: {image.IsBlurred}");

            processor.FilterHandler -= processor.IncreaseContrast;
            processor.FilterHandler -= blurFilter;
            processor.Run(image);

            Console.WriteLine($"После удаления фильтров Contrast, Blur.\nЯркость: {image.Brightness}, Контраст: {image.Contrast}, Размытие: {image.IsBlurred}");

            Console.WriteLine($"\nUSER INPUT\n");

            var userImage = ImageInput.ReadImage();

            processor.FilterHandler = null;
            processor.FilterHandler += processor.IncreaseBrightness;
            processor.FilterHandler += processor.IncreaseContrast;
            processor.FilterHandler += blurFilter;

            Console.WriteLine($"\nДо обработки:\nЯркость: {userImage.Brightness}, Контраст: {userImage.Contrast}, Размытие: {userImage.IsBlurred}");
            processor.Run(userImage);
            Console.WriteLine($"После обработки:\nЯркость: {userImage.Brightness}, Контраст: {userImage.Contrast}, Размытие: {userImage.IsBlurred}\n");
        }
    }
}