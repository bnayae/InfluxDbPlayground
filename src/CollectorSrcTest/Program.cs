using InfluxDB.Collector;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectorSrcTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Metrics.Collector = new CollectorConfiguration()
             .Tag.With("version", "v1")
             .Tag.With("host", Environment.MachineName)
             .Tag.With("user", Environment.UserName)
             .Batch.AtInterval(TimeSpan.FromSeconds(5))
             .WriteTo.InfluxDB("http://localhost:8086", database: "playground")
             .CreateCollector();

            var tags = new Dictionary<string, string> { ["level"] = "1" };

            var fields = new Dictionary<string, object>
            {
                ["fix"] = 1,
                ["val"] = 10,
            }; // better to reuse
            Metrics.Write("test", fields: fields, tags: tags);

            Console.ReadLine();
        }
    }
}
