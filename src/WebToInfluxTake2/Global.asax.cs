using InfluxCollectManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WebToInfluxTake2
{
    public class WebApiApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            InfluxManager.Init(new InfluxConfig());

            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
        }

        private class InfluxConfig : IInfluxConfig
        {
            public TimeSpan BatchInterval => TimeSpan.FromSeconds(1);

            public string Url => "http://localhost:8086";

            public string DatabaseName => "playground";
        }
    }
}
