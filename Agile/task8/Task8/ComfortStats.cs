using System;

namespace Task8
{
    public class ComfortStats
    {
        private int _dryCount = 0;

        public void Subscribe(HumiditySensor sensor)
        {
            sensor.HumidityChanged += OnHumidityChanged;
        }

        private void OnHumidityChanged(HumiditySensor sender, int percent)
        {
            if (percent < sender.DryThreshold)
                _dryCount++;
        }

        public void Report()
        {
            Console.WriteLine($"\nСлишком сухо (<30%) было {_dryCount} раз(а).");
        }
    }
}
