using Metrics;
using Metrics.Core;
using Metrics.InfluxDB;
using Metrics.InfluxDB.Adapters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;

namespace WebToInflux.Controllers
{
    //[TelemetryReporterInflux]
    [TelemetryReporterMetrics]
    //[RoutePrefix("api/values"]
    public class ValuesController : ApiController
    {
        // GET api/values
        //[Route()]
        public async Task<IEnumerable<string>> Get()
        {
            int duration = await DAL();
            return new string[] { "duration", duration.ToString() };
        }

        private async Task<int> DAL()
        {
            int duration = (Environment.TickCount % 10);
            await Task.Delay(duration * 1000);
            return duration;
        }

        // GET api/values/5
        public async Task<string> Get(int id)
        {
            await Task.Delay(id * 1000);
            return $"duration = {id}";
        }

        // POST api/values
        public void Post([FromBody]string value)
        {
        }

        // PUT api/values/5
        public void Put(int id, [FromBody]string value)
        {
        }

        // DELETE api/values/5
        public void Delete(int id)
        {
        }
    }
}
