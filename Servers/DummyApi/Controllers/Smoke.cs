using Microsoft.AspNetCore.Mvc;

namespace DummyApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class Smoke : Controller
{
    [HttpGet("smoke/")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]    
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Get()
    {
        return Ok("Hello World");
    }
}