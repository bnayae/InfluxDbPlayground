using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebToInflux;

namespace WebToInfluxTake2.Controllers
{
    [TelemetryReporterInflux]
    public class ValuesController : ApiController
    {
        // GET api/values
        public Task<string> Get()
        {
            return Task.FromResult("Hi");
        }

        // GET api/values/5
        public Task<string> Get(int id)
        {
            return Task.FromResult($"value {id}");
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
