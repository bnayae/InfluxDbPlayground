using InfluxDB.Collector;
using Metrics;
using Metrics.InfluxDB;
using Metrics.InfluxDB.Adapters;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebToInflux
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        private static readonly TimeSpan REPORT_INTERVAL = TimeSpan.FromSeconds(1);

        protected void Application_Start()
        {
            //InitMetricsNet();
            InitInflux();

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private static void InitInflux()
        {
            string version = ConfigurationManager.AppSettings["Metrics.GlobalContextName"];
            var collector = new CollectorConfiguration()
                 .Tag.With("ver", version)
                 .Tag.With("host", Environment.MachineName)
                 .Tag.With("user", Environment.UserName)
                 .Batch.AtInterval(TimeSpan.FromSeconds(1))
                 .WriteTo.InfluxDB("http://localhost:8086", database: "playground")
                 .CreateCollector();

            InfluxDB.Collector.Metrics.Collector = collector;
        }
        private static void InitMetricsNet()
        {
            Metric.Config
                //.WithHttpEndpoint("http://localhost:1234/")
                .WithReporting(report => report
                              .WithInfluxDbUdp("localhost", 8089,
                                        REPORT_INTERVAL,
                                        null /* filter */,
                                        cfg =>
                                        {
                                            cfg.Database = "playground";
                                            cfg /* configuration */
                                            .WithConverter(new DefaultConverter()
                                                    .WithGlobalTags("host=host-udp,env=dev-udp"))
                                            //.WithFormatter(new DefaultFormatter().WithLowercase(true))
                                            .WithWriter(new InfluxdbUdpWriter(cfg, batchSize: 100))
                                            ;
                                        })
                     //.WithInfluxDbHttp("localhost", 32768,
                     //     "playground" /* database name */,
                     //     REPORT_INTERVAL,
                     //     null /* filter */,
                     //     cfg => cfg /* configuration */
                     //        .WithConverter(new DefaultConverter().WithGlobalTags($"host={Environment.MachineName},env=Dev"))
                     //        .WithFormatter(new DefaultFormatter().WithLowercase(true))
                     //        .WithWriter(new InfluxdbHttpWriter(cfg, batchSize: 1000)))
                     );
        }
    }
}
