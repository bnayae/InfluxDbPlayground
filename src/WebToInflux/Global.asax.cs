using Metrics;
using Metrics.InfluxDB;
using Metrics.InfluxDB.Adapters;
using System;
using System.Collections.Generic;
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
        private static readonly TimeSpan REPORT_INTERVAL = TimeSpan.FromMilliseconds(30);

        protected void Application_Start()
        {
            Metric.Config
                .WithHttpEndpoint("http://localhost:1234/")
                .WithReporting(report => report
                    .WithInfluxDbHttp("localhost", 32768,
                                     "playground" /* database name */,
                                     REPORT_INTERVAL,
                                     null /* filter */,
                                     cfg => cfg /* configuration */
                                        .WithConverter(new DefaultConverter().WithGlobalTags($"host={Environment.MachineName},env=Dev"))
                                        .WithFormatter(new DefaultFormatter().WithLowercase(true))
                                        .WithWriter(new InfluxdbHttpWriter(cfg, batchSize: 1000)))
                     );

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }
    }
}
