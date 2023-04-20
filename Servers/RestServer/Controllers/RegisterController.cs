using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Servers.DbManagers;
using Servers.ProxyMaker;
using Servers.Validators;

namespace Servers.Controllers;

[ApiController]
[Route("api/User")]
public class RegisterController : Controller
{
    private readonly ILogger<RegisterController> _logger;
    private readonly SrvDbManager _srvDbManager;
    private readonly SessionTokenValidator _tokenValidator;
    private readonly PassValidator _passValidator;
    private readonly EmailValidator _emailValidator;
    private readonly ProxyManager _proxyManager;

    public RegisterController(
        ILogger<RegisterController> logger,
        SrvDbManager srvDbManager,
        SessionTokenValidator sessionTokenValidator,
        PassValidator passValidator,
        EmailValidator emailValidator,
        ProxyManager proxyManager)
    {
        _logger = logger;
        _srvDbManager = srvDbManager;
        _tokenValidator = sessionTokenValidator;
        _passValidator = passValidator;
        _emailValidator = emailValidator;
        _proxyManager = proxyManager;
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
    
    [HttpPost("register/")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Post([FromQuery] string email, [FromQuery] string password)
    {
        _logger.LogInformation($"{GetRoute()}: {email} {password}");

        if (_srvDbManager.GetUser(email) is { } user)
        {
            _logger.LogInformation($"{user} already exists");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (!_emailValidator.IsEmailValid(email))
        {
            _logger.LogInformation($"{email} is not valid");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (!_passValidator.IsPassValid(password))
        {
            _logger.LogInformation($"{password} is not valid");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.CreateUser(
                email,
                BCrypt.Net.BCrypt.HashPassword(password, PassValidator.GetPasswordSalt())) is not { } newUser)
        {
            _logger.LogInformation($"couldn't create new user");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.NewSessionToken(newUser.UserId, _tokenValidator.GetValidToken()) is not { } token)
        {
            _logger.LogInformation($"couldn't create valid token");
            return StatusCode(403); // TODO: return more verbose error code
        }

        return Ok(LogReturned(_proxyManager.RestTokenFromToken(token)));
    }
}