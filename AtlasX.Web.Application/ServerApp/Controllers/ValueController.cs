using Microsoft.AspNetCore.Mvc;

namespace AtlasX.Web.Application.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ValueController : ControllerBase
    {

        public ValueController()
        { }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("ValueController");
        }

    }
}