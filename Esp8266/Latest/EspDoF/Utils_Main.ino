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
}

void SendSensorData() {
  Serial.println("Sending sensor data");
  String macRaw = String(WiFi.macAddress());
  if (imu.gyroAvailable()) {
    imu.readGyro();
    PublishSensorData(macRaw + "/gyro x", imu.calcGyro(imu.gx));
    PublishSensorData(macRaw + "/gyro y", imu.calcGyro(imu.gy));
    PublishSensorData(macRaw + "/gyro z", imu.calcGyro(imu.gz));
#ifdef VerbosePublish
    Serial.println("Published gyroscope: \tx:" + String(imu.calcAccel(imu.gx)) + "\ty:" + String(imu.calcAccel(imu.gy)) + "\tz:" + String(imu.calcAccel(imu.gz)));
#endif
  }
  if (imu.accelAvailable()) {
    imu.readAccel();
    PublishSensorData(macRaw + "/acc x", imu.calcAccel(imu.ax));
    PublishSensorData(macRaw + "/acc y", imu.calcAccel(imu.ay));
    PublishSensorData(macRaw + "/acc z", imu.calcAccel(imu.az));
#ifdef VerbosePublish
    Serial.println("Published acceleration: \tx:" + String(imu.calcAccel(imu.ax)) + "\ty:" + String(imu.calcAccel(imu.ay)) + "\tz:" + String(imu.calcAccel(imu.az)));
#endif
  }
  if (imu.magAvailable()) {
    imu.readMag();
    PublishSensorData(macRaw + "/mag x", imu.calcMag(imu.mx));
    PublishSensorData(macRaw + "/mag y", imu.calcMag(imu.my));
    PublishSensorData(macRaw + "/mag z", imu.calcMag(imu.mz));
#ifdef VerbosePublish
    Serial.println("Published magnitude: \tx:" + String(imu.calcAccel(imu.mx)) + "\ty:" + String(imu.calcAccel(imu.my)) + "\tz:" + String(imu.calcAccel(imu.mz)));
#endif
  }
}

void PublishSensorData(String topic, float data1) {
  String content = String(data1);
  client.publish(topic.c_str(), content.c_str(), content.length());
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
