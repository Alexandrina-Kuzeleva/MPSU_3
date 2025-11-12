using System;

namespace Task8
{
    internal class Program
    {
        static void Main()
        {
            var sensor = new HumiditySensor
            {
                MoldThreshold = 80,
                DryThreshold = 30, 
                IntervalMs = 300    
            };

            var panel = new ConsolePanel();
            var stats = new ComfortStats();

            panel.Subscribe(sensor);
            stats.Subscribe(sensor);

            sensor.Start();

            panel.Unsubscribe(sensor);
            stats.Report();

            Console.WriteLine("\nРабота завершена. Данные сохранены в humidity_log.txt");
        }
    }
}
