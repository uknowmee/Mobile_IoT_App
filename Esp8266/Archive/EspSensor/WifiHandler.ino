void ConnectWiFi(String ssid, String pass) {  //later rename to WifiStaConnect
  Serial.println("Connecting to: SSID: " + ssid + " Password: " + pass);
  WiFi.disconnect();
  WiFi.mode(WIFI_STA);  //station (Wifi Station = client)
  WiFi.begin(ssid, pass);
}


IPAddress local_IP(192, 168, 0, 1);
IPAddress gateway(192, 168, 0, 1);
IPAddress subnet(255, 255, 255, 0);

void CreateWiFi() {  //later rename to WifiAPCreate
  WiFi.softAPdisconnect(true);
  Serial.print("Setting AP config: ");
  Serial.println(WiFi.softAPConfig(local_IP, gateway, subnet));

  Serial.print("Creating softAP: ");
  Serial.println(WiFi.softAP(GenerateSSID().c_str(), GeneratePassword().c_str()));
}

void TestWifiConn(String ssid, String pass) {  //later rename to WifiStaApConnect
  Serial.println("Transforming into STA+AP and Connecting to: SSID: " + ssid + " Password: " + pass);
  WiFi.mode(WIFI_AP_STA);
  WiFi.begin(ssid, pass);
}

void SwapWifiConn() {  //later rename to WifiKeepOnlyStaConnection
  Serial.println("Transforming into STA");
  WiFi.softAPdisconnect(true);
  WiFi.mode(WIFI_STA);
}

void ChangeTargetWifi(String ssid, String pass) {  //later rename to WifiChangeStaTarget
  Serial.println(WiFi.getMode());
  Serial.println(WiFi.getMode() == WIFI_STA);
  Serial.println(WiFi.getMode() == WIFI_AP_STA);
  if (WiFi.getMode() == WIFI_STA || WiFi.getMode() == WIFI_AP_STA) {
    Serial.print("Changing Target Wifi and Connecting to: SSID: " + ssid + " Password: " + pass);
    WiFi.disconnect();
    WiFi.begin(ssid, pass);
  }
}