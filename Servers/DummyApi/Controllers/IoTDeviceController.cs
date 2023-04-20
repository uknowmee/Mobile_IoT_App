using Microsoft.AspNetCore.Mvc;

namespace DummyApi.Controllers
{
    [ApiController]
    [Route("")]
    public class IoTDeviceController : Controller
    {
        // new mock DB reference
        MockDB mockDB = MockDB.GetInstance();
        // create logger
        private readonly ILogger<RegisteredIoTDeviceController> _logger;

        public IoTDeviceController(ILogger<RegisteredIoTDeviceController> logger)
        {
            // register logger
            _logger = logger;
        }

        // A terrible mock of the ESP32 Http Server index.html page form request
        [HttpGet("index.html")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(String))]
        public IActionResult Get([FromQuery] string ssid, [FromQuery] string pass)
        {
            // prevent data race on DB
            lock (this.mockDB)
            {
                // TODO: implement format validation for the url variables ssid and pass 

                return Ok(); // TODO: return more verbose error code

                // Subsequent IoT Device behaviour
                // try to connect to the given WiFi with provided password

                // if no wifi with ssid provided detected or bad provided password
                // reset remembered ssid and pass

                // Connected to the WiFi specidied
                // try to submit MQTT data to the default topic ("temperature")
            }
        }
    }
}
