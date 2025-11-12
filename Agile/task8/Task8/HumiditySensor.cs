using System;
using System.IO;
using System.Threading;

namespace Task8
{
    public class HumiditySensor
    {
        public delegate void HumidityEventHandler(
            HumiditySensor sender, int percent
        );
        public event HumidityEventHandler? HumidityChanged;

        private EventHandler<int>? _moldRiskReached;

        public event EventHandler<int> MoldRiskReached
        {
            add
            {
                _moldRiskReached += value;
                Console.WriteLine("Подписчик добавлен на MoldRiskReached");
            }
            remove
            {
                _moldRiskReached -= value;
                Console.WriteLine("Подписчик удалён с MoldRiskReached");
            }
        }

        public int MoldThreshold { get; set; } = 80;
        public int DryThreshold { get; set; } = 30;
        public int IntervalMs { get; set; } = 300;

        public void Start()
        {
            var rand = new Random();
            string logPath = "humidity_log.txt";

            File.WriteAllText(logPath, "Сессия измерений\n");

            int count = rand.Next(8, 13);
            for (int i = 0; i < count; i++)
            {
                int humidity = rand.Next(20, 96);

                string status = string.Empty;
                if (humidity >= MoldThreshold)
                    status = "ВЫСОКАЯ ВЛАЖНОСТЬ";
                else if (humidity < DryThreshold)
                    status = "СЛИШКОМ СУХО";

                string logLine = $"{DateTime.Now:HH:mm:ss} → {humidity}% {status}";
                File.AppendAllText(logPath, logLine + Environment.NewLine);

                HumidityChanged?.Invoke(this, humidity);

                if (humidity >= MoldThreshold)
                    _moldRiskReached?.Invoke(this, humidity);

                Thread.Sleep(IntervalMs);
            }
        }
    }
}
