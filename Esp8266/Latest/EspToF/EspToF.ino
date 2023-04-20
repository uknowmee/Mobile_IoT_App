#define DEVELOPING
#define VerbosePublish
//---------------CONFIG------------------//
#include <ESP8266WiFi.h>   // for wifi
#include <ESPAsyncTCP.h>   // for http server
#include <PubSubClient.h>  // for MQTT
#include <FS.h>            // for persistent data (onboard flash)
//#include <ESPAsyncWebServer.h>
#include <ESP8266WebServerSecure.h>
#include <Adafruit_VL53L0X.h>
#include <SimpleTerminal.h>

#define ledPin 2
#define altLedPin 16
//const char *mqtt_server = "91.121.93.94"; //test.mosquitto.com
const char *mqtt_server = "192.168.137.160";
String clientId = "";  //leave blank for autogeneration
String macAsId;

WiFiClient espClient;
PubSubClient client(espClient);
//AsyncWebServer server(80);
BearSSL::ESP8266WebServerSecure server(443);
BearSSL::ServerSessions serverCache(5);

Adafruit_VL53L0X lox = Adafruit_VL53L0X();

#ifdef DEVELOPING
SimpleTerminal terminal(&Serial,20);
void Test(String &name, String &line);
#endif

void Init();
void setup() {
  Init();
}

void MainTaskProcessor();
void ProcessFps();

void loop() {
  MainTaskProcessor();
  ProcessFps();
  terminal.run();
}
