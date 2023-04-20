void MainTaskProcessor() {
  if (httpServerUp) {
    server.handleClient();
    //MDNS.update(); //<- this was in the HelloServerBearSSL example
    //https://github.com/esp8266/Arduino/blob/master/libraries/ESP8266WebServer/examples/HelloServerBearSSL/HelloServerBearSSL.ino
  }
  if (!sensorInitialized)
    return;

  if ((*tolf) == 0 || millis() - (*tolf) > (*mpt)) {
    if (HasConnection())
      SendSensorData();
    (*tolf) = millis();
  }
  if (verboseSensor)
    PrintSensorValue();
}

void PrintSensorValue() {
  Serial.println("SensorValue\t" + String(GetSensorData()));
}

void SendSensorData() {
  int distance = GetSensorData();
  String content = String(distance);
  String topic = String(WiFi.macAddress()) + "/distance";
  client.publish(topic.c_str(), content.c_str(), content.length());
#ifdef VerbosePublish
  Serial.println("Published: " + content);
#endif
}

int GetSensorData() {
  static VL53L0X_RangingMeasurementData_t measure;
  static int distance;

  lox.rangingTest(&measure, false);  // pass in 'true' to get debug data printout!
  if (measure.RangeStatus != 4) {    // phase failures have incorrect data
    distance = measure.RangeMilliMeter;
  } else {
    distance = 0;
  }
  return distance;
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
