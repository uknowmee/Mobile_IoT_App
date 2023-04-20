using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Servers.DbManagers;
using Servers.ProxyMaker;
using Servers.Validators;

namespace Servers.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : Controller
{
    private readonly ILogger<UserController> _logger;
    private readonly SrvDbManager _srvDbManager;
    private readonly ProxyManager _proxyManager;
    private readonly SessionTokenValidator _tokenValidator;

    public UserController(
        ILogger<UserController> logger,
        SrvDbManager srvDbManager,
        ProxyManager proxyManager,
        SessionTokenValidator tokenValidator)
    {
        _srvDbManager = srvDbManager;
        _proxyManager = proxyManager;
        _tokenValidator = tokenValidator;
        _logger = logger;
    }
    
    private string? GetRoute()
    {
        var ari = ControllerContext.ActionDescriptor.AttributeRouteInfo;
        var route = ari?.Name ?? ari?.Template;
        return route;
    }
    
    private T LogReturned<T>(T toLog)
    {
        _logger.LogInformation($"Returned: {JsonConvert.SerializeObject(toLog, Formatting.Indented)}");

        return toLog;
    }
    
    [HttpPost("login/")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Post([FromQuery] string email, [FromQuery] string password)
    {
        _logger.LogInformation($"{GetRoute()}: {email} {password}");
        
        if (_srvDbManager.GetUser(email) is not { } user)
        {
            _logger.LogInformation($"{email} not recognized");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
        {
            _logger.LogInformation($"{password} not recognized");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.NewSessionToken(user.UserId, _tokenValidator.GetValidToken()) is not { } token)
        {
            _logger.LogInformation($"there is no user with id: {user.UserId}");
            return StatusCode(403); // TODO: return more verbose error code
        }

        // return only the tokenHash
        return Ok(LogReturned(_proxyManager.RestTokenFromToken(token)));
    }

    [HttpPut("logout/")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put([FromQuery] string sessionToken)
    {
        _logger.LogInformation($"{GetRoute()}");

        if (_srvDbManager.GetToken(sessionToken) is not { } token)
        {
            _logger.LogInformation($"{sessionToken} is not valid");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetUser(token.UserId) is not { } user)
        {
            _logger.LogInformation($"there is no user with id: {token.UserId}");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (!_srvDbManager.RemoveToken(token))
        {
            _logger.LogInformation($"{token} doesnt exists");
            return StatusCode(403); // TODO: return more verbose error code
        }

        return Ok(); // TODO: return more verbose error code
    }
}