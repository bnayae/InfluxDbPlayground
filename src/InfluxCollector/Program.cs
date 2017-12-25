using InfluxDB.Collector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// https://github.com/influxdata/influxdb-csharp

namespace InfluxCollector
{
    class Program
    {
        private static readonly Random _rnd = new Random();

        static void Main(string[] args)
        {
            MetricsCollector collector =
                Metrics.Collector = new CollectorConfiguration()
                     .Tag.With("host", Environment.MachineName)
                     .Tag.With("user", Environment.UserName)
                     .Batch.AtInterval(TimeSpan.FromSeconds(2))
                     .WriteTo.InfluxDB("http://localhost:8086", database: "playground")
                     .CreateCollector();

            while (true)
            {
                Metrics.Increment("Call");

                Metrics.Write("Complex",
                    new Dictionary<string, object>
                    {
                    { "Black", _rnd.Next(1, 100) },
                    { "White", _rnd.Next(1, 100) }
                    });

                Metrics.Measure("Data", _rnd.Next(1, 100));

                int delay = _rnd.Next(50, 3000);
                Thread.Sleep(delay);
                Console.Write(".");
            }
        }
    }
}
