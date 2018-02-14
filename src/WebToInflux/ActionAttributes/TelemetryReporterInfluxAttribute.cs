using System;
using InfluxDB.Collector;
using influx = InfluxDB.Collector.Metrics;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebToInflux
{
    public class TelemetryReporterInfluxAttribute : ActionFilterAttribute
    {
        private static ObjectCache _cache = MemoryCache.Default;

        public override void OnActionExecuting(
            HttpActionContext actionContext)
        {
            var request = actionContext.Request;
            string actionName = request.GetActionDescriptor().ActionName;
            var tags = new Dictionary<string, string>
            {
                ["method"] = request.Method.Method,
                ["uri"] = request.RequestUri.ToString(),
                ["version"] = request.Version.ToString()
            };

            influx.Increment($"{actionName}_count", tags: tags);
            //influx.Measure($"{actionName}_time", 1, tags:tags);
            var operation = influx.Collector.Time($"{actionName}_time", tags);
            actionContext.ActionArguments.Add("end-action", operation);
        }

        public override void OnActionExecuted(
            HttpActionExecutedContext actionExecutedContext)
        {
            //var request = actionExecutedContext.ActionContext.Request;
            //string actionName = request.GetActionDescriptor().ActionName;

            //var tags = new Dictionary<string, string>
            //{
            //    ["method"] = request.Method.Method,
            //    ["uri"] = request.RequestUri.ToString(),
            //    ["version"] = request.Version.ToString()
            //};

            //influx.Increment($"{actionName}_time", -1, tags: tags);
            var operation = (IDisposable)actionExecutedContext.ActionContext.ActionArguments["end-action"];
            operation.Dispose();
        }

    }
}