void MainTaskProcessor() {
  if (httpServerUp) {
    server.handleClient();
    //MDNS.update(); //<- this was in the HelloServerBearSSL example
    //https://github.com/esp8266/Arduino/blob/master/libraries/ESP8266WebServer/examples/HelloServerBearSSL/HelloServerBearSSL.ino
  }

  if ((*tolf) == 0 || millis() - (*tolf) > (*mpt)) {
    if (HasConnection())
      SendSensorData();
    (*tolf) = millis();
  }
}

void SendSensorData() {
  String content = String((micros()%6)+1);
  String topic = String(WiFi.macAddress()) + "/dice";
  client.publish(topic.c_str(), content.c_str(), content.length());
#ifdef VerbosePublish
  Serial.println("Published: " + content);
#endif
}


bool HasConnection() {
  if (WiFi.status() != WL_CONNECTED)
    return false;

  if (client.connected()) {
    client.loop();  //handy place to put this line here
    return true;
  }
  if ((*tolatctms) == 0 || millis() - (*tolatctms) > (*mmcat)) {
    client.connect(clientId.c_str());
    *tolatctms = millis();
    if (client.connected())
      return true;
  }
  return false;
}
