using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using AtlasX.Engine.Connector;
using System.Linq;

namespace AtlasX.Web.Service.Controllers;

[Route("api/[controller]")] // /api/apphoro
[ApiController]

public class AppHoroController : ControllerBase
{
    public AppHoroController() { }

    [HttpGet]
    public IActionResult Index()
    {
        QueryParameter queryParams = new QueryParameter(Request);

        string name = queryParams["name"]?.ToString();

        if (string.IsNullOrEmpty(name))
        {
            return BadRequest("Missing Parameter name");
        }

        // คำนวณผลรวม ASCII ของอักขระแต่ละตัวใน name
        int sumAscii = name.Sum(c => (int)c);

        // คำนวณ mod (%) ของผลรวมด้วย 10
        int score = sumAscii % 10;

        // ตีความคะแนนเป็นเกรด
        string grade;
        int stars;

        if (score >= 0 && score <= 3)
        {
            grade = "bad";
            stars = 1;
        }
        else if (score >= 4 && score <= 6)
        {
            grade = "so so";
            stars = 2;
        }
        else
        {
            grade = "good";
            stars = 3;
        }

        var result = new Dictionary<string, object>
        {
            { "grade", grade },
            { "stars", stars }
        };

        return Ok(result);
    }
}
