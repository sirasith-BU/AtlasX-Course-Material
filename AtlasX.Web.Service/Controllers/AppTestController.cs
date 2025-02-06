using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AtlasX.Engine.Connector;

namespace AtlasX.Web.Service.Controllers;

[Route("api/[controller]")] // /api/apptest
[ApiController]

public class AppTestController : ControllerBase
{
    public AppTestController() { }

    [HttpGet]
    public IActionResult Index()
    {
        var result = new Dictionary<string, object>();
        result.Add("success", true);
        result.Add("message", "Test");

        return Ok(result);
    }

    [HttpPost]
    [Route("hello")] // /api/apptest/hello
    public IActionResult Hello()
    {
        QueryParameter queryParams = new QueryParameter(Request);

        if (queryParams["name"] is null)
        {
            return BadRequest("Missing Parameter name");
        }

        var result = new Dictionary<string, object>();
        result.Add("success", true);
        result.Add("message", "Hello, " + queryParams["name"].ToString() + "!");
        return Ok(result);
    }
}