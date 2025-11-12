using System;

namespace Task8
{
    public class ConsolePanel
    {
        public void Subscribe(HumiditySensor sensor)
        {
            sensor.HumidityChanged += OnHumidityChanged;
            sensor.MoldRiskReached += OnMoldRiskReached;
        }

        public void Unsubscribe(HumiditySensor sensor)
        {
            sensor.MoldRiskReached -= OnMoldRiskReached;
        }

        private void OnHumidityChanged(HumiditySensor sender, int percent)
        {
            Console.WriteLine($"Влажность: {percent}%");
        }

        private void OnMoldRiskReached(object? sender, int percent)
        {
            Console.WriteLine($"Высокая влажность: {percent}% — проветрите помещение");
        }
    }
}
