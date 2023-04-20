void Init() {  
  InitSerial();
  InitGpio();
  InitSpiffs();
  if (wasFactoryReseted)
    return;
  InitWifi();
  InitMQTT();
  InitClientId();

#ifdef DEVELOPING
  //this will force to run webserver every time the device is booted
  terminal.addCommand("enter", (void *)PrintEnter);
  terminal.addCommand("ip", (void *)PrintIp);
  terminal.addCommand("wifi_status", (void *)PrintWifiStatus);
  terminal.addCommand("wifi_mode", (void *)PrintWifiMode);
  terminal.addCommand("mqtt", (void *)PrintIsMqttConnected);
  terminal.addCommand("creds", (void *)InjectWifiCreds);
  terminal.addCommand("hasConn", (void *)CheckForConnection);
  terminal.addCommand("getFps", (void *)PrintFps);
  terminal.addCommand("factoryReset", (void *)RevertToFactorySettings);
  InitHttpServer();
#endif

  deleteFile("/bootAttempts.txt");
  PrewarmFpsProcessor();
}

void InitSerial() {
  Serial.begin(115200);
  Serial.print("\n\n\n\n\n");
  Serial.println("<-> Esp is alive! Serial initialized");
}
void InitGpio() {
  Serial.print("<-> GPIO init... ");
  pinMode(ledPin, OUTPUT);
  pinMode(altLedPin, OUTPUT);
  digitalWrite(ledPin, HIGH);     //disable led (LED is LOW-active)
  digitalWrite(altLedPin, HIGH);  //disable led (LED is LOW-active)
  Serial.println("OK");
}
void InitSpiffs() {
  Serial.print("<-> SPIFFS init... ");
  if (!SPIFFS.begin()) {
    Serial.println("Not OK !");
    return;
  }
  spiffsInitialized = true;
  Serial.println("OK");
  ProcessBootAttempts();
}
void CreateAP();
void InitWifi() {
  Serial.print("<-> WiFi init... ");
  Serial.println("Current mode is: " + GetWifiMode());
  Serial.print("      ");
  if (WiFi.getMode() != 0) {
    Serial.println("Wifi already set, skipping stage");
    return;
  }
  if (existsFile(WIFI_SSID_FILE) && existsFile(WIFI_PASS_FILE)) {
    Serial.println(F("credientials detected, connecting"));
    Serial.print("      ");
    ConnectUsingCredientialsInFiles();
  } else {
    Serial.println(F("credientials Not detected, creating AP, setting up http panel"));
    CreateAP();
    setupHttpServer();
    httpServerUp = true;
  }
  Serial.print("      ");
  Serial.println("Current mode is: " + GetWifiMode());
}
void InitMQTT() {
  client.setServer(mqtt_server, 1883);
  macAsId = String(WiFi.macAddress());
  macAsId.replace(":", "");
}

void InitClientId() {
  Serial.print("<-> ClientId init... ");
  if (clientId != "") {
    Serial.print(" leaving as it is: ");
    Serial.println(clientId);
    return;
  }
  String mac = String(WiFi.macAddress());
  mac.replace(":", "");
 clientId = deviceName.substring(0,min((int)deviceName.length(),10)) + "-" + mac; //mac has 12 bytes, mqtt client max name is 23 chars, so it leaves up to 11 chars for name
  Serial.print(" set to: ");
  Serial.println(clientId);
}
void setupHttpServer();
void InitHttpServer() {
  Serial.print("<-> Http Server init... ");
  if (httpServerUp) {
    Serial.println("already up - OK");
    return;
  }
  setupHttpServer();
  Serial.println("OK");
}