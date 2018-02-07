using Metrics;
using Metrics.Core;
using Metrics.InfluxDB;
using Metrics.InfluxDB.Adapters;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static System.Math;

// SCHEMA: https://docs.influxdata.com/influxdb/v1.4/query_language/schema_exploration/
// QUERY: https://docs.influxdata.com/influxdb/v1.4/query_language/data_exploration/

// Influx QL:  https://docs.influxdata.com/influxdb/v1.4/query_language/spec/
// https://docs.influxdata.com/influxdb/v1.4/query_language/data_download/
// https://github.com/etishor/Metrics.NET/wiki/Configuration
// https://docs.influxdata.com/influxdb/v0.8/api/query_language/
// chronograf: http://localhost:8888/sources/1/chronograf/data-explorer

// influx-db command line:
// start with
// # influx
// # show databases
// # CREATE DATABASE {name}
// # DROP DATABASE {name}
// # precision rfc3339
// # use <database>
// # SHOW SERIES
// # SHOW SERIES [FROM <measurement_name> [WHERE <tag_key>='<tag_value>']]
// # DROP SERIES FROM /v1.*\.end/
// # SHOW MEASUREMENTS
// # SHOW MEASUREMENTS WITH MEASUREMENT =~ /v1\..*/ -- all fields from measurements that start with 'v1.' 
// # SHOW TAG KEYS
// # SHOW TAG KEYS FROM "v1.cos"
// # SHOW FIELD KEYS
// # SHOW FIELD KEYS FROM /v1\..*\.sin/   -- all fields from series that start with 'v1.' and end with '.sin'


namespace HelloInfluxMetrics
{
    class Program
    {
        private static readonly MetricTags DEFAULT_TAGS = new MetricTags("categoryZ=CategoryA", "categoryV=CategoryB");
        private static readonly TimeSpan REPORT_INTERVAL = TimeSpan.FromSeconds(1);
        private static readonly Counter _modTesting =
                Metric
                    
                    //.Context("TheContext")
                    //.Meter("mod", Unit.Events, TimeUnit.Nanoseconds, tags: DEFAULT_TAGS);
                    .Counter("mod", Unit.Events, tags: DEFAULT_TAGS);
        private static readonly Meter _sinTesting =
                Metric//.Context("TheContext")
                    .Meter("sin", Unit.Events, tags: DEFAULT_TAGS);
        private static readonly Meter _cosTesting =
                Metric//.Context("TheContext")
                    .Meter("cos", Unit.Events, tags: DEFAULT_TAGS);
        private static readonly Meter _sinRndTesting =
                Metric//.Context("TheContext")
                    .Meter("sin-rnd", Unit.Events, TimeUnit.Seconds, DEFAULT_TAGS);
        private static readonly Meter _cosRndTesting =
                Metric//.Context("TheContext")
                    .Meter("cos-rnd", Unit.Events, TimeUnit.Seconds, DEFAULT_TAGS);
        private static readonly Metrics.Timer _timerTesting =
                Metric.Timer("Scope", Unit.Events, tags: new MetricTags("categoryX=CategoryC", "categoryY=CategoryB"));
        private static readonly Random _rnd = new Random();
        private static readonly Stopwatch _stopper = Stopwatch.StartNew();

        static void Main(string[] args)
        {
            //Metric.Context("class name").
               var configuration = Metric.Config;
            configuration
                //.WithDefaultSamplingType(SamplingType.SlidingWindow)                
                .WithHttpEndpoint("http://localhost:1234/")
                .WithReporting(report => report
                    //.WithInfluxDbUdp("localhost", 8089,
                    //.WithInfluxDbUdp("localhost", 32771,
                    //                REPORT_INTERVAL,
                    //                null /* filter */,
                    //                cfg =>
                    //                {
                    //                    cfg.Database = "playground";
                    //                    cfg /* configuration */
                    //                    .WithConverter(new DefaultConverter()
                    //                            .WithGlobalTags("host=host-udp,env=dev-udp"));
                    //                    //.WithFormatter(new DefaultFormatter().WithLowercase(true))
                    //                    //.WithWriter(new InfluxdbUdpWriter(cfg, batchSize: 100))
                    //                    //.WithWriter(new InfluxdbUdpWriter(cfg, batchSize: 10))
                    //                    ;
                    //                })
                    .WithInfluxDbHttp("localhost", 32768,
                                     "playground" /* database name */,
                                     REPORT_INTERVAL,
                                     null /* filter */,
                                     cfg => cfg /* configuration */
                                        .WithConverter(new DefaultConverter().WithGlobalTags("host=BnayaPC,env=QA"))
                                        .WithFormatter(new DefaultFormatter().WithLowercase(true))
                                        .WithWriter(new InfluxdbHttpWriter(cfg, batchSize: 1000)))
                     );
            //.WithAllCounters();
            //MetricTags tags = new MetricTags("Good", "Bad", "Ugly");
            //while (true)
            //{
            //    int delay = _rnd.Next(50, 160);
            //    _volatileTesting.Mark(delay / 10);
            //    Metric.Gauge("Absolute", () => delay / 10 - 200, Unit.Requests, tags);
            //    using (var scope = _timerTesting.NewContext(Environment.UserName))
            //    {
            //        Thread.Sleep(delay);
            //        Console.Write(".");
            //    }
            //}
            //Task _;
            ConsoleMetric().Wait() ;
            // _ = ProduceModMetric();
            //_ = ProduceModMetric();
            //_ = ProduceMetricRnd();
            //_ = ProduceMetric();
            Console.ReadLine();
        }

        private static async Task ConsoleMetric()
        {
            await Task.Delay(1);
            do
            {
                char c = Console.ReadKey(true).KeyChar;
                int s = Abs(c - '1' + 1);
                var timer = Metric.Timer(
                    "Request",
                    Unit.Requests,
                    tags: new MetricTags($"duration={s}"));
                //var timer = Metric.Advanced.Timer(
                //    "Request",
                //    Unit.Requests,
                //    () => new TimerMetric(SamplingType.Default),
                //    TimeUnit.Milliseconds,
                //    tags: new MetricTags($"duration={s}"));
                Task _ = Task.Run(async () =>
                {
                    using (timer.NewContext())
                    {
                        Console.Write($"{s},");
                        await Task.Delay(s * 1000);
                    }
                });
            } while (true);
        }

        private static async Task ProduceModMetric()
        {

            while (true)
            {
                using (_timerTesting.NewContext("X"))
                {
                    int delay = (int)(100 * (_stopper.Elapsed.TotalSeconds % 50)); // up to 5 second
                    await Task.Delay(delay);
                    string tag = "low";
                    if (delay > 2500)
                        tag = "high";
                    else if (delay > 100)
                        tag = "mid";

                    await Task.Delay(delay);
                    //_modTesting.Mark(tag, delay);
                    //_modTesting.Mark(tag, 1);
                    _modTesting.Increment(tag);
                    Console.Write(".");
                }

            }
        }

        private static async Task ProduceMetric()
        {
            while (true)
            {
                await Task.Delay(50);
                var s = (long)Abs(Sin(_stopper.Elapsed.TotalSeconds) * 100);
                var c = (long)Abs(Cos(_stopper.Elapsed.TotalSeconds) * 100);
                _sinRndTesting.Mark(s);
                _cosRndTesting.Mark(c);
                Console.Write("x");
            }
        }
        private static async Task ProduceMetricRnd()
        {
            while (true)
            {
                int delay = _rnd.Next(50, 400);
                await Task.Delay(delay);
                var s = (long)Abs(Sin(_stopper.Elapsed.TotalSeconds) * delay);
                var c = (long)Abs(Cos(_stopper.Elapsed.TotalSeconds) * delay);
                _sinRndTesting.Mark(s);
                _cosRndTesting.Mark(c);
                Console.Write("#");
            }
        }
    }
}
