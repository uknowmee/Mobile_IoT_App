const char* mqtt_server = "91.121.93.94";

String clientId = "SmartDevice" + String(random(0xffff), HEX);

void configureMQTT(){
  client.setServer(mqtt_server, 1883);
}