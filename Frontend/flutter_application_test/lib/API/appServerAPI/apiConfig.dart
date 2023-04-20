// TODO: Use config address only when invalid appServerIP provided
const String appServerIP = "192.168.43.29"; // default AP HTTP server IP
const String protocol = "https";
const String port = "7123"; // default IoT Device HTTP server listening port
const String apiRoot = "api/";

// TODO: Implement API call timeout interval (1200 ms) - change of http client library needed

// min time to wait between api calls
const int minCallTimeout = 1000;

// API endpoints
const String userApi = "User/";
const String iotDeviceApi = "RegisteredIoTDevice/";
