using Metrics;
using Metrics.InfluxDB;
using Metrics.InfluxDB.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// chronograf: http://localhost:8888/sources/1/chronograf/data-explorer

namespace HelloInfluxMetrics
{
    class Program
    {
        private static readonly TimeSpan REPORT_INTERVAL = TimeSpan.FromSeconds(3);
        private static readonly Meter _volatileTesting =
                Metric.Meter("Volatile", Unit.Requests, TimeUnit.Seconds);
        private static readonly Metrics.Timer _timerTesting =
                Metric.Timer("Scope", Unit.Requests);

        private static readonly Random _rnd = new Random();

        static void Main(string[] args)
        {
            var configuration = Metric.Config;
            configuration.WithReporting(report => report
                    .WithInfluxDbUdp("localhost", 8089,
                                    REPORT_INTERVAL,
                                    null /* filter */,
                                    cfg =>
                                    {
                                        //cfg.Database = "playground";
                                        //cfg /* configuration */
                                        //.WithConverter(new DefaultConverter())
                                        //.WithGlobalTags("host=web1,env=dev"))
                                        //.WithFormatter(new DefaultFormatter().WithLowercase(true))
                                        //.WithWriter(new InfluxdbUdpWriter(cfg, batchSize: 100))
                                        //.WithWriter(new InfluxdbUdpWriter(cfg, batchSize: 10))
                                        ;
                                    })
                    //.WithInfluxDbHttp("localhost", 8086,
                    //                 "playground" /* database name */,
                    //                 REPORT_INTERVAL,
                    //                 null /* filter */,
                    //                 cfg => cfg /* configuration */
                    //                    .WithConverter(new DefaultConverter().WithGlobalTags("host=web1,env=dev"))
                    //                    .WithFormatter(new DefaultFormatter().WithLowercase(true))
                    //                    .WithWriter(new InfluxdbHttpWriter(cfg, batchSize: 1000)))
                     );
            //.WithAllCounters();
            MetricTags tags = new MetricTags("Good", "Bad", "Ugly");
            while (true)
            {
                int delay = _rnd.Next(50, 6000);
                _volatileTesting.Mark(delay / 10);
                Metric.Gauge("Absolute", () => delay / 10 - 200, Unit.Requests, tags);
                using (var scope = _timerTesting.NewContext(Environment.UserName))
                {
                    Thread.Sleep(delay);
                    Console.Write(".");
                }
            }

        }
    }
}
