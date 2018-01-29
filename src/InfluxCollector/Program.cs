using InfluxDB.Collector;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

// https://github.com/influxdata/influxdb-csharp
// https://docs.influxdata.com/influxdb/v0.9/query_language/schema_exploration/
// https://github.com/etishor/Metrics.NET/wiki/Configuration
// https://docs.influxdata.com/influxdb/v0.8/api/query_language/
// chronograf: http://localhost:8888/sources/1/chronograf/data-explorer

// influx-db command line:
// start with
// # influx
// # show databases
// # CREATE DATABASE {name}
// # DROP DATABASE {name}
// # use <database>
// # SHOW SERIES
// # SHOW SERIES [FROM <measurement_name> [WHERE <tag_key>='<tag_value>']]
// # SHOW MEASUREMENTS
// # SHOW MEASUREMENTS WITH MEASUREMENT =~ /v1\..*/ -- all fields from measurements that start with 'v1.' 
// # SHOW TAG KEYS
// # SHOW TAG KEYS FROM "v1.cos"
// # SHOW FIELD KEYS
// # SHOW FIELD KEYS FROM /v1\..*\.sin/   -- all fields from series that start with 'v1.' and end with '.sin'

namespace InfluxCollector
{
    class Program
    {
        //private static readonly Random _rnd = new Random();
        private static readonly Stopwatch _stopper = Stopwatch.StartNew();

        static void Main(string[] args)
        {
            MetricsCollector collector =
                Metrics.Collector = new CollectorConfiguration()
                     .Tag.With("version", "v1")
                     .Tag.With("host", Environment.MachineName)
                     .Tag.With("user", Environment.UserName)
                     .Batch.AtInterval(TimeSpan.FromSeconds(1))
                     //.WriteTo.InfluxDB("http://localhost:8086", database: "playground")
                     .WriteTo.InfluxDB("http://localhost:32770", database: "playground")
                     .CreateCollector();

            //while (true)
            //{
            //    Metrics.Increment("Call");

            //    Metrics.Write("Complex",
            //        new Dictionary<string, object>
            //        {
            //        { "Black", _rnd.Next(1, 100) },
            //        { "White", _rnd.Next(1, 100) }
            //        });

            //    Metrics.Measure("Data", _rnd.Next(1, 100));

            //    int delay = _rnd.Next(50, 3000);
            //    Thread.Sleep(delay);
            //    Console.Write(".");
            //}
            Task _ = ProduceModMetric();
            Console.ReadLine();
        }

        private static async Task ProduceModMetric()
        {
            while (true)
            {
                int delay = (int)(100 * (_stopper.Elapsed.TotalSeconds % 50)); // up to 5 second
                await Task.Delay(delay);
                string tag = "low";
                if (delay > 2500)
                    tag = "high";
                else if (delay > 1000)
                    tag = "mid";

                var tags = new Dictionary<string, string> { ["level"] = tag }; // better to reuse
                // # SELECT count FROM "mod" WHERE version = 'v1'  LIMIT 10
                //Metrics.Increment("mod", tags: tags); 

                // # SELECT value FROM "mod" WHERE version = 'v1'  LIMIT 10
                //Metrics.Measure("mod", 1, tags: tags); 

                //Metrics.Time("mod", tags: tags); // do nothing


                // # SELECT val, val10, val100 FROM "mod" WHERE version = 'v1'  LIMIT 10
                var fields = new Dictionary<string, object>
                    {
                        ["val"] = delay,
                        ["val10"] = delay % 10,
                        ["val100"] = delay % 100
                    }; // better to reuse
                Metrics.Write("mod", fields: fields, tags: tags);
                Console.Write(".");
            }
        }
    }
}
