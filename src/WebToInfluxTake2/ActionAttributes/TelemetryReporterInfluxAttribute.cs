using InfluxCollectManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;

namespace WebToInflux
{
    public class TelemetryReporterInfluxAttribute : ActionFilterAttribute
    {
        //private static ObjectCache _cache = MemoryCache.Default;
        private const string COMPONENT_NAME = "webapi";

        public override void OnActionExecuting(
            HttpActionContext actionContext)
        {
            //actionContext.RequestContext.Principal.Identity.Name
            var request = actionContext.Request;
            string actionName = request.GetActionDescriptor().ActionName;
            var tags = new Dictionary<string, string>
            {
                ["method"] = request.Method.Method,
                ["uri"] = request.RequestUri.ToString(),
                ["version"] = request.Version.ToString()
            };

            InfluxManager.Default.TryAddTagRange(tags);

            InfluxManager.Default.Volatile(COMPONENT_NAME, actionName);
            var operation = InfluxManager.Default.Duration(COMPONENT_NAME, actionName);
            actionContext.ActionArguments.Add("end-action", operation);
        }

        public override void OnActionExecuted(
            HttpActionExecutedContext actionExecutedContext)
        {

            var operation = (IDisposable)actionExecutedContext.ActionContext.ActionArguments["end-action"];
            operation.Dispose();
        }

    }
}