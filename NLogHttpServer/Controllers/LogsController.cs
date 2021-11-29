using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis;
using Newtonsoft.Json;

namespace NLogHttpServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogsController : ControllerBase
    {
        [HttpPost]
        public void Post([FromBody] object value)
        {
            var str = JsonConvert.SerializeObject(value);
            Debug.WriteLine(str);
        }
    }
}
