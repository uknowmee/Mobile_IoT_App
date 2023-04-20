using Database.ServerDatabase.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Servers.DbManagers;
using Servers.Listeners;
using Servers.ProxyMaker;
using Servers.ProxyMaker.ViewModels;
using Device = Database.ServerDatabase.Models.Device;
using TopicData = Database.ServerDatabase.Models.TopicData;

namespace Servers.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RegisteredIoTDeviceController : Controller
{
    private readonly ILogger<RegisteredIoTDeviceController> _logger;
    private readonly ListenersManager _listenersManager;
    private readonly SrvDbManager _srvDbManager;
    private readonly ProxyManager _proxyManager;

    public RegisteredIoTDeviceController(
        ILogger<RegisteredIoTDeviceController> logger,
        ListenersManager listenersManager,
        SrvDbManager srvDbManager,
        ProxyManager proxyManager)
    {
        _logger = logger;
        _srvDbManager = srvDbManager;
        _listenersManager = listenersManager;
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
    public IActionResult Post([FromQuery] string sessionToken, [FromQuery] string deviceModel,
        [FromQuery] string deviceMac)
    {
        _logger.LogInformation($"{GetRoute()}: {deviceModel} {deviceMac}");

        deviceMac = deviceMac.ToUpper();

        if (_srvDbManager.GetToken(sessionToken) is not { } token)
        {
            _logger.LogInformation($"{sessionToken} is not valid");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetUser(token.UserId) is not { } user)
        {
            _logger.LogInformation($"{token.UserId} there is no user with token");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetDevice(deviceMac) is { } device)
        {
            _logger.LogInformation($"{device} already registered");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetDeviceModel(deviceModel) is not { } model)
        {
            _logger.LogInformation($"{deviceModel} doesnt exist");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.AddDevice(user.UserId, deviceModel, deviceMac) is not { } newDevice)
        {
            _logger.LogInformation($"couldn't add new device");
            return StatusCode(403); // TODO: return more verbose error code
        }

        try
        {
            _listenersManager.AddListenerToDevice(newDevice);
            return Ok(LogReturned(newDevice));
        }
        catch (Exception e)
        {
            return StatusCode(403); // TODO: return more verbose error code
        }
    }

    [HttpGet("list")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IEnumerable<Device>> Get([FromQuery] string sessionToken)
    {
        _logger.LogInformation($"{GetRoute()}");

        if (_srvDbManager.GetToken(sessionToken) is not { } token)
        {
            _logger.LogInformation($"{sessionToken} doesnt exist");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetUser(token.UserId) is not { } user)
        {
            _logger.LogInformation($"{token.UserId} user with Id doesnt exists");
            return StatusCode(403); // TODO: return more verbose error code
        }

        return Ok(LogReturned(_proxyManager.RestDevicesWithTopics(user.UserId)));
    }

    [HttpPut("led/")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public IActionResult Put([FromQuery] string sessionToken, [FromQuery] string deviceMac, [FromQuery] bool state)
    {
        _logger.LogInformation($"{GetRoute()}: {deviceMac} {state}");

        if (_srvDbManager.GetToken(sessionToken) is not { } token)
        {
            _logger.LogInformation($"{sessionToken} doesnt exist");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetUser(token.UserId) is not { } user)
        {
            _logger.LogInformation($"{token.UserId} user with Id doesnt exists");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetDevice(deviceMac) is not { } device)
        {
            _logger.LogInformation($"{deviceMac} doesnt exists");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (user.UserId != device.UserId)
        {
            _logger.LogInformation($"{user} doesnt have access to device {device}");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (!_srvDbManager.ChangeDeviceLedState(state, device))
        {
            _logger.LogInformation($"couldn't change state of device {device}");
            return StatusCode(403); // TODO: return more verbose error code
        }

        return Ok(LogReturned(device)); // TODO: return more verbose error code
    }

    [HttpGet("data")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IEnumerable<TopicData>> Get([FromQuery] string sessionToken, [FromQuery] string deviceMac,
        [FromQuery] string topicName)
    {
        _logger.LogInformation($"{GetRoute()}: {deviceMac} {topicName}");

        if (_srvDbManager.GetToken(sessionToken) is not { } token)
        {
            _logger.LogInformation($"{sessionToken} doesnt exist");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetUser(token.UserId) is not { } user)
        {
            _logger.LogInformation($"{token.UserId} user with Id doesnt exists");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetDevice(deviceMac) is not { } device)
        {
            _logger.LogInformation($"{deviceMac} doesnt exists");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (user.UserId != device.UserId)
        {
            _logger.LogInformation($"{user} doesnt have access to device {device}");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetTopic(topicName) is not { } topic)
        {
            _logger.LogInformation($"{topicName} doesnt exists");
            return StatusCode(403); // TODO: return more verbose error code
        }

        return Ok(
            LogReturned(
                _proxyManager.RestTopicDataFromTopicData(
                    _srvDbManager.GetTopicData(device.DeviceId, topic.TopicId, token.CreationDate)
                )
            )
        );
    }

    [HttpGet("topics")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public ActionResult<IEnumerable<Topic>> Get([FromQuery] string sessionToken, [FromQuery] string deviceMac)
    {
        _logger.LogInformation($"{GetRoute()}: {deviceMac}");

        if (_srvDbManager.GetToken(sessionToken) is not { } token)
        {
            _logger.LogInformation($"{sessionToken} doesnt exist");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetUser(token.UserId) is not { } user)
        {
            _logger.LogInformation($"{token.UserId} user with Id doesnt exists");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (_srvDbManager.GetDevice(deviceMac) is not { } device)
        {
            _logger.LogInformation($"{deviceMac} doesnt exists");
            return StatusCode(403); // TODO: return more verbose error code
        }

        if (user.UserId != device.UserId)
        {
            _logger.LogInformation($"{user} doesnt have access to device {device}");
            return StatusCode(403); // TODO: return more verbose error code
        }

        return Ok(LogReturned(_srvDbManager.GetAllDeviceTopics(device.DeviceId)));
    }
}