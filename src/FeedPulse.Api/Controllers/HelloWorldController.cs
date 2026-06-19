using Microsoft.AspNetCore.Mvc;

namespace HelloWorld.Controllers
{   
    [ApiController]
    public class HelloWorldController : ControllerBase
    {
        [Route("health")]
        [HttpGet]
        public string Gethealth()
        {
            return "health";
        }

        [Route("version")]
        [HttpGet]
        public string Getversion()
        {
            return "version";
        }
    }
}