using Metrics;
using Metrics.Core;
using Metrics.Sampling;
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

                Func<Reservoir> factory = () => new UniformReservoir(3);
                //Func<Reservoir> factory = () => new PlayReservoir();
                _timer = Metric
                    //.Context(key.GetHashCode().ToString())
                    .Context(actionName)
                    .Advanced
                    .Timer(
                     "request_timer",
                     Unit.Requests,
                     factory,
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

            private class PlayReservoir : Reservoir
            {
                public Snapshot GetSnapshot(bool resetReservoir = false)
                {
                    return new PlaySnapshot();
                }

                public void Reset()
                {
                }

                public void Update(long value, string userValue = null)
                {
                }
            }

            private class PlaySnapshot : Snapshot
            {
                private int _value = 0;
                

                public long Count => 0;

                public IEnumerable<long> Values => Enumerable.Range(0, 3).Select(i => (long)i);

                public long Max => 10;

                public string MaxUserValue => string.Empty;

                public double Mean => 5;

                public double Median => 6;

                public long Min => 2;

                public string MinUserValue => string.Empty;

                public double Percentile75 => 5;

                public double Percentile95 => 5;

                public double Percentile98 => 5;

                public double Percentile99 => 5;

                public double Percentile999 => 5;

                public double StdDev => 5;

                public int Size => 3;

                public double GetValue(double quantile) => quantile;
                
            }
        }
    }
}