#define VerboseWiFi

//called only via endpoint
void ConnectWiFi(String ssid, String pass) {
#ifdef VerboseWiFi
  Serial.println("Connecting to: SSID: " + ssid + " Password: " + pass);
#endif
  WiFi.mode(WIFI_STA);  //station (Wifi Station = client)
  WiFi.disconnect();
  WiFi.begin(ssid, pass);
}

IPAddress local_IP(192, 168, 0, 1);
IPAddress gateway(192, 168, 0, 1);
IPAddress subnet(255, 255, 255, 0);

//called only via code
void CreateAP() {
  WiFi.mode(WIFI_AP);
  WiFi.softAPdisconnect(true);
#ifdef VerboseAP
  Serial.print("Setting AP config: ");
  Serial.println(WiFi.softAPConfig(local_IP, gateway, subnet));

  Serial.print("Creating softAP: ");
  Serial.println(WiFi.softAP(GenerateSSID().c_str(), GeneratePassword().c_str()));
#else
  WiFi.softAPConfig(local_IP, gateway, subnet);
  WiFi.softAP(GenerateSSID().c_str(), GeneratePassword().c_str());
#endif
}

String GenerateSSID() {
  String mac = String(WiFi.macAddress());
  mac.replace(":", "");
  return "IoT_ISI " + mac.substring(4, 7) + "-" + mac.substring(7, 12);  // Max ssid name is 32 chars, ending is 9 (7 chars + '-' + ' ') + 23 chars for name
}

String GeneratePassword() {
  String mac = String(WiFi.macAddress());
  mac.replace(":", "");
  return mac.substring(4, 12).c_str();
}
