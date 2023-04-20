const String iotDeviceIP = "192.168.0.1"; // default AP HTTP server IP
const String protocol = "https";
const String port = "443"; // default IoT Device HTTP server listening port

const String apiRoot = "";
const String setWiFiDataEndpoint = "SetWifiData";
const String aliveEndpoint = "isAlive";
const String getMacEndpoint = "GetMac";

// TODO: Implement API call timeout interval (1200 ms) - change of http client library needed

// min time to wait between api calls
const int minCallTimeout = 1000;
