using Metrics;
using Metrics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebToInflux
{
    public class TelemetryReporterAttribute : ActionFilterAttribute
    {
        private static readonly MetricTags DEFAULT_TAGS = new MetricTags("categoryZ=CategoryA", "categoryV=CategoryB");

        public override void OnActionExecuting(
            HttpActionContext actionContext)
        {
            var timer = Metric.Advanced.Timer(
                "Request",
                Unit.Requests, 
                () => new TimerMetric(SamplingType.Default),
                TimeUnit.Milliseconds,
                tags: new MetricTags(
                        $"request-method={actionContext.Request.Method.Method}",
                        $"request-uri={actionContext.Request.RequestUri}",
                        $"request-version={actionContext.Request.Version}"));
            IDisposable disp = timer.NewContext();

            actionContext.ActionArguments.Add("timer", disp);
            timer.StartRecording();
         }

        public override void OnActionExecuted(
            HttpActionExecutedContext actionExecutedContext)
        {
            var timer = (IDisposable)actionExecutedContext.ActionContext.ActionArguments["timer"];
            timer.Dispose();
        }
    }
}