using Microsoft.AspNetCore.Mvc;
using Servers.DbManagers;
using Servers.ProxyMaker.ViewModels;

namespace Servers.Controllers;

[ApiController]
[Route("/")]
public class Smoke : Controller
{
    private readonly ILogger<Smoke> _logger;
    private readonly SrvDbManager _srvDbManager;

    private string? GetRoute()
    {
        var ari = ControllerContext.ActionDescriptor.AttributeRouteInfo;
        var route = ari?.Name ?? ari?.Template;
        return route;
    }

    public Smoke(ILogger<Smoke> logger, SrvDbManager srvDbManager)
    {
        _logger = logger;
        _srvDbManager = srvDbManager;
    }

    [HttpGet("smoke/")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Get()
    {
        _logger.LogInformation($"{GetRoute()}");
        return Ok("Hello World");
    }
}