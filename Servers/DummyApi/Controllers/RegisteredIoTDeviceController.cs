using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace DummyApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RegisteredIoTDeviceController : Controller
    {
        // new mock DB reference
        MockDB mockDB = MockDB.GetInstance();

        // create logger
        private readonly ILogger<RegisteredIoTDeviceController> _logger;

        public RegisteredIoTDeviceController(ILogger<RegisteredIoTDeviceController> logger)
        {
            // register logger
            _logger = logger;
        }


        [HttpPost("register/")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Post([FromQuery] string sessionToken, [FromQuery] string deviceModel, [FromQuery] string deviceMac)
        {
            // prevent data race on DB
            lock (this.mockDB)
            {
                // TODO: implement format validation for deviceMac and maybe deviceName

                // try to find the session with provided sessionToken (tokenHash)
                var session_matched = mockDB.getSessions().FirstOrDefault(
                    s => s.TokenHash == sessionToken
                );

                // if there exists a session with sessionToken provided
                if (session_matched != null)
                {
                    // get session associated user
                    var user_matched = mockDB.getUsers().FirstOrDefault(
                        u => u.Id == session_matched.UserId
                    );
                    // try to find a registered device with a provided mac
                    var device_matched = mockDB.getDevices().FirstOrDefault(d => d.Mac == deviceMac);

                    // if user is in db and the device is not (not registered)
                    if (user_matched != null && device_matched == null)
                    {
                        RegisteredIoTDevice newDevice = new RegisteredIoTDevice
                        {
                            Id = mockDB.getNewDeviceIdx(),
                            UserId = user_matched.Id,
                            RegistrationDate = DateTime.UtcNow,
                            UniqueName = deviceModel,
                            Mac = deviceMac,
                            LEDState = false
                        };
                        // register newly created device
                        mockDB.getDevices().Add(newDevice);
                        // return status code: successfuly registered new device
                        return Ok();
                    }
                }

                // No user with provided sessionToken in db
                // A device with provided mac is already registered
                return StatusCode(403); // TODO: return more verbose error code
            }
        }

        [HttpGet("list")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<RegisteredIoTDevice>> Get([FromQuery] string sessionToken)
        {
            // prevent data race on DB
            lock (this.mockDB)
            {
                // TODO: implement format validation for the url variables sessionToken

                // try to find the session with provided sessionToken (tokenHash)
                var session_matched = mockDB.getSessions().FirstOrDefault(
                    s => s.TokenHash == sessionToken
                );

                // if there exists a session with sessionToken provided
                if (session_matched != null)
                {
                    // get session associated user
                    var user_matched = mockDB.getUsers().FirstOrDefault(
                        u => u.Id == session_matched.UserId
                    );

                    // if user is in db
                    if (user_matched != null)
                    {
                        // try to find all registered devices for this user
                        var devices_matched = mockDB.getDevices().FindAll(d => d.UserId == user_matched.Id);

                        // return status code: successfuly changed Device LED state
                        return Ok(devices_matched); // TODO: return more verbose error code
                    }
                }

                // No user with provided sessionToken in db
                return StatusCode(403); // TODO: return more verbose error code
            }
        }

        [HttpPut("led/")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public IActionResult Put([FromQuery] string sessionToken, [FromQuery] string deviceMac, [FromQuery] bool state)
        {
            // prevent data race on DB
            lock (this.mockDB)
            {
                // TODO: implement format validation for the url variables deviceMac and state

                // try to find the session with provided sessionToken (tokenHash)
                var session_matched = mockDB.getSessions().FirstOrDefault(
                    s => s.TokenHash == sessionToken
                );

                // if there exists a session with sessionToken provided
                if (session_matched != null)
                {
                    // get session associated user
                    var user_matched = mockDB.getUsers().FirstOrDefault(
                        u => u.Id == session_matched.UserId
                    );
                    // try to find a registered device with a provided mac
                    var device_matched = mockDB.getDevices().FirstOrDefault(d => d.Mac == deviceMac);

                    // if user is in db and has the device with provided mac registered
                    if (user_matched != null && device_matched != null && user_matched.Id == device_matched.UserId)
                    {
                        // set new led state for the device
                        device_matched.LEDState = state;
                        // return status code: successfuly changed Device LED state
                        return Ok(); // TODO: return more verbose error code
                    }
                }

                // No user with provided sessionToken in db
                return StatusCode(403); // TODO: return more verbose error code
            }
        }

        [HttpGet("data")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public ActionResult<IEnumerable<TopicData>> Get([FromQuery] string sessionToken, [FromQuery] string deviceMac, [FromQuery] string topicName)
        {
            // prevent data race on DB
            lock (this.mockDB)
            {
                // TODO: implement format validation for the url variables sessionToken

                // try to find the session with provided sessionToken (tokenHash)
                var session_matched = mockDB.getSessions().FirstOrDefault(
                    s => s.TokenHash == sessionToken
                );

                // if there exists a session with sessionToken provided
                if (session_matched != null)
                {
                    // get session associated user
                    var user_matched = mockDB.getUsers().FirstOrDefault(
                        u => u.Id == session_matched.UserId
                    );

                    // if user is in db
                    if (user_matched != null)
                    {
                        // try to find a registered device with a provided mac
                        var userRegisteredDevice = mockDB.getDevices().FirstOrDefault(d => d.UserId == user_matched.Id && d.Mac == deviceMac);
                        // get topic with name provided
                        var topic = mockDB.getTopics().FirstOrDefault(t => t.Name == topicName);

                        if (userRegisteredDevice != null && topic != null)
                        {
                            // get all topic data for the device since user login
                            var data = mockDB
                                .GetTopicData()
                                .FindAll(d => d.DeviceId == userRegisteredDevice.Id && d.TopicId == topic.Id && d.CreatedAt >= session_matched.CreationDate);

                            // return status code: successfuly changed Device LED state
                            return Ok(data); // TODO: return more verbose error code        
                        }
                    }
                }

                // No user with provided sessionToken in db
                return StatusCode(403); // TODO: return more verbose error code
            }
        }
    }
}
