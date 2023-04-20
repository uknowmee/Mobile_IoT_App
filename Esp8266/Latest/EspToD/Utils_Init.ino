void ProcessBootAttempts() {
  Serial.print("      Checking for interrupted boot's... ");
  writeFile("/bootAttempts.txt", ".", false);            // add dot to the file
  bootAttempts = getFile("/bootAttempts.txt").length();  // get number of dots in file
  Serial.print("  this is attempt no." + String(bootAttempts));
  delay(400);  //this is time to fail a boot;

  if (bootAttempts >= 7) {
    Serial.println("\n  absolute failed boot limit reached. Factory Reset");
    RevertToFactorySettings();
    wasFactoryReseted = true;
    return;
  }

  if (bootAttempts >= 3) {
    Serial.print("\n  failed boot limit reached, creating AP... ");
    DoBootAttemptErrorBlink();
    deleteFile("/bootAttempts.txt");  //order intentional. If innterrupted during blinking 2 times -> factory reset
    CreateAP();
    Serial.println("AP created");
  } else {
    Serial.println(", all OK");
  }
}
void DoBootAttemptErrorBlink() {
  for (int i = 0; i < 10; i++) {
    digitalWrite(ledPin, LOW);  //enable led
    delay(50);
    digitalWrite(ledPin, HIGH);  //disable led
    delay(50);
  }
}
void PrewarmFpsProcessor() {
  frames = 1000;
}

String GetWifiMode() {
  const String stateStr[] = { "WIFI_OFF", "WIFI_STA", "WIFI_AP", "WIFI_AP_STA" };
  return stateStr[WiFi.getMode()];
}
String GetWifiStatus() {
  const String stateStr[] = { "WL_IDLE_STATUS", "WL_NO_SSID_AVAIL", "WL_SCAN_COMPLETED", "WL_CONNECTED", "WL_CONNECT_FAILED", "WL_CONNECTION_LOST", "WL_WRONG_PASSWORD", "WL_DISCONNECTED" };
  return stateStr[WiFi.status()];
}

void ConnectUsingCredientialsInFiles() {
  ConnectWiFi(getFile(WIFI_SSID_FILE), getFile(WIFI_PASS_FILE));
}
void PrintEnter() {
  Serial.println();
}
void PrintIp() {
  Serial.println(WiFi.localIP());
}
void PrintWifiStatus() {
  Serial.println(GetWifiStatus());
}
void PrintWifiMode() {
  Serial.println(GetWifiMode());
}
void PrintIsMqttConnected() {
  Serial.println(client.connected() ? "Mqtt connected" : "Mqtt not connected");
}
void InjectWifiCreds(){
  writeFile(WIFI_SSID_FILE,"Uknowme");
  writeFile(WIFI_PASS_FILE,"12345678");
  Serial.println("Injected");
}
void CheckForConnection(){
  Serial.println(HasConnection()?"there is a connection":"No connection");
}
void PrintFps() {
  Serial.println(lastFrameFPS);
}
