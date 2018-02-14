using Metrics;
using Metrics.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebToInflux
{
    public class TelemetryReporterMetrics : ActionFilterAttribute
    {
        private static ObjectCache _cache = MemoryCache.Default;

        public override void OnActionExecuting(
            HttpActionContext actionContext)
        {
            OperationMonitor monitor = 
                GetCompletion(actionContext.Request);
            Action end = monitor.Start();
            actionContext.ActionArguments.Add("end-action", end);
        }

        public override void OnActionExecuted(
            HttpActionExecutedContext actionExecutedContext)
        {
            var end = (Action)actionExecutedContext.ActionContext.ActionArguments["end-action"];
            end();
        }

        private static OperationMonitor GetCompletion(
            HttpRequestMessage request)
        {
            string actionName = request.GetActionDescriptor().ActionName;

            var monitor = _cache[actionName] as OperationMonitor;

            if (monitor != null)
                return monitor;

            var policy = new CacheItemPolicy()
            {
                SlidingExpiration = TimeSpan.FromMinutes(10)
            };

            monitor = new OperationMonitor(request);
            _cache.Set(actionName, monitor, policy);
            return monitor;
        }

        private class OperationMonitor
        {
            private readonly Timer _timer;
            private readonly Counter _counter;

            public OperationMonitor(HttpRequestMessage request)
            {
                string actionName = request.GetActionDescriptor().ActionName;
                var tags = new MetricTags(
                             $"method={request.Method.Method}",
                             $"uri={request.RequestUri}",
                             $"version={request.Version}");
                _timer = Metric
                    //.Context(key.GetHashCode().ToString())
                    .Context(actionName)
                    .Timer(
                     "request_timer",
                     Unit.Requests,
                     tags: tags);

                _counter = Metric
                    .Context(actionName)
                    .Counter(
                     "request_counter",
                     Unit.Requests,
                     tags: tags);
            }

            public Action Start()
            {
                IDisposable timerEnd = _timer.NewContext();
                _counter.Increment();
                return () =>
                {
                    timerEnd.Dispose();
                    _counter.Decrement();
                };
            }            
        }
    }
}