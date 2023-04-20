//comment following line if releasing the code
#define Developing

#ifdef ESP32
#include <WiFi.h>      // for wifi
#include <AsyncTCP.h>  // for http server
#include "SPIFFS.h"

#elif ESP8266
#include <ESP8266WiFi.h>  // for wifi
#include <ESPAsyncTCP.h>  // for http server
#endif

#include <PubSubClient.h>  //for MQTT
#include <FS.h>            //for persistent data (onboard flash)
#include <ESPAsyncWebServer.h>

#define ledPin 2
int sm_state = 0;  //StateMachine state
void SetState(int newState);

String page_generic = "";
String module_notConnected = "";

WiFiClient espClient;
PubSubClient client(espClient);
long lastMqttAction = 0;


String getFile(String path);
void writeFile(String path, String data, bool clearFile);
void deleteFile(String path);
void CreateWiFi();

void setup() {
  pinMode(ledPin, OUTPUT);
  digitalWrite(ledPin, HIGH);  //disable led (LED is LOW-active)
  Serial.begin(115200);
  Serial.println();

  WiFi.disconnect();            //this is for debug purposes, to avoid misinterpreting states
  WiFi.softAPdisconnect(true);  // this should stay here

  if (!SPIFFS.begin()) {
    Serial.println("SPIFFS.begin problem");
    SetState(-20);
    return;
  }

  InitWebsites();

  writeFile("/bootAttempts.txt", ".", false);                //add dot to the file
  int bootAttempts = getFile("/bootAttempts.txt").length();  // get number of dots in file
  if (bootAttempts >= 4) {
    Serial.println("Boot problem");
    deleteFile("/bootAttempts.txt");
    for (int i = 0; i < 10; i++) {
      digitalWrite(ledPin, LOW);  //enable led
      delay(50);
      digitalWrite(ledPin, HIGH);  //disable led
      delay(50);
    }
    CreateWiFi();
    setupHttpServer();
    SetState(-1);
    return;
  }

#ifdef Developing
  //this will force to run webserver every time the device is booted
  setupHttpServer();
#endif
}

//remove that later:
void InitWebsites() {
  page_generic = getFile("/generic.html");
  module_notConnected = getFile("/notConnected.html");
}

ulong frames = 0;
ulong lastSec = 0;
void ProcessFps() {
  frames++;
  uint sec = millis() / 1000;
  if (sec != lastSec) {  //overflow protection
    lastSec = sec;
    if(frames<100){
      Serial.println("Throttle warning: fps = " + String(frames));
    }
    frames = 0;
  }
}
//Typical:
// 18000 fps for simple led
// 10000 fps for sin() based led  

void loop() {
  ProcessStateMachine();
  ProcessFps();
  Debug_GetEyeModeFromSerial();
}

void Debug_GetStateFromSerial() {
  if (Serial.available()) {
    SetState(Serial.parseInt());
    while (Serial.read() != -1) {}
  }
}

int debug_ledLevel = 500;
void Debug_GetLedLevelFromSerial() {
  if (Serial.available()) {
    debug_ledLevel = Serial.parseInt();
    while (Serial.read() != -1) {}
  }
}

int eyeMode = 0;
void Debug_GetEyeModeFromSerial() {
  if (Serial.available()) {
    eyeMode = Serial.parseInt();
    while (Serial.read() != -1) {}
  }
}

void Debug_ClearConfigFiles() {
  deleteFile("/bootAttempts.txt");
  deleteFile("/WiFi_ssid");
  deleteFile("/WiFi_password");
}