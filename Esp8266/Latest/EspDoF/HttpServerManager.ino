void notFound() {
  String message = "Page not found";
  message += "URI: ";
  message += server.uri();
  message += "\nMethod: ";
  message += (server.method() == HTTP_GET) ? "GET" : "POST";
  message += "\nArguments: ";
  message += server.args();
  if (verboseHttpServer)
    Serial.println(message);
  server.send(404, "text/plain", "Not found");
}
void SetWifiSettingsRequest() {
  if (verboseHttpServer)
    Serial.println("SetWifiSettingsRequest");
  String inputSsid;
  String inputPass;

  if (server.arg(WIFI_SSID_SERVER_INPUT_NAME) && server.arg(WIFI_PASS_SERVER_INPUT_NAME)) {
    if (verboseHttpServer) {
      Serial.print("New SSID: ");
      Serial.println(server.arg(WIFI_SSID_SERVER_INPUT_NAME));
      Serial.print("New Password: ");
      Serial.println(server.arg(WIFI_PASS_SERVER_INPUT_NAME));
    }
    inputSsid = server.arg(WIFI_SSID_SERVER_INPUT_NAME);
    inputPass = server.arg(WIFI_PASS_SERVER_INPUT_NAME);
    writeFile(WIFI_SSID_FILE, inputSsid);
    writeFile(WIFI_PASS_FILE, inputPass);
    server.send(200, "text/html", "SSID and Password set");
    ESP.restart();
    return;
  }
  server.send(200, "text/html", "Missing parameters");
}
void GetMac() {
  server.send(200, "text/plain", String(WiFi.macAddress()));
}
void GetModel() {
  server.send(200, "text/plain", deviceName);
}
void GetModel(){
  server.send(200, "text/plain", deviceName );
}
void setupHttpServer() {
  server.on("/isAlive", HTTP_GET, []() {
    server.send(200, "text/html", "Yup");
  });
  server.on("/FactoryReset", HTTP_GET, []() {
    server.send(200, "text/html", "Reverted to factory settings");
    RevertToFactorySettings();
  });
  server.on("/GetMac", HTTP_GET, GetMac);
  server.on("/GetModel", HTTP_GET, GetModel);
  server.onNotFound(notFound);
  //poniższe dwie linijki zostały przeniesione w związku z ustaleniami na temat
  //podsłuchiwania eteru. Zostały one tylko do debugowania.
  server.on("/SetWifiData", HTTP_POST, SetWifiSettingsRequest);
  server.getServer().setRSACert(new BearSSL::X509List(serverCert), new BearSSL::PrivateKey(serverKey));
  httpServerUp = true;
  server.begin();
}
