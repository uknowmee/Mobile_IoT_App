//-20 - SpiFFS error
//-1 - setting credientials
// 0 - bootup
// 1 - connecting to wifi
// 2 - connecting to mqtt
// 3 - working mode
//*4 - debug

//TODO: separate core state machine from stateled visualization !!

void SetState(int newState) {
  if (newState != sm_state)
    Serial.println("State: " + String(newState));
  sm_state = newState;
}

void ProcessStateMachine() {
  if (sm_state == -20) {
    ProcessSpiFfsError();
  } else if (sm_state == -1) {
    ProcessWifiCredsEntering();
  } else if (sm_state == -2) {
    ProcessWifiCredsTesting();
  } else if (sm_state == 0) {
    ProcessBootup();
  } else if (sm_state == 1) {
    ProcessConnectingToWifi();
  } else if (sm_state == 2) {
    ProcessConnectingToMQTT();
  } else if (sm_state == 3) {
    ProcessWorkingState();
  } else if (sm_state == 4) {
    ProcessDebug();
  }
}

// 4 state
void ProcessDebug() {
  const static int debugOsc = 2500;
  const static int debugOscHalf = 1250;
  double s_val = long_abs(millis() % debugOsc - debugOscHalf) * 1.0 / debugOscHalf;
  printLed(1 - abs(s_val));  // inverse of sin, since i want more blinks, less light
}


// -20 state
void ProcessSpiFfsError() {
  printLed(millis() % 1400 < 1000);
}

// -1 state
uint stateNeg1Limiter = 0;
void ProcessWifiCredsEntering() {
  double s_val = sin(millis() / 1000.0 * 3.1415);

  double x = 1 - abs(s_val);

  printLed(x);  // inverse of sin, since i want more blinks, less light
  //printLed(0.5);
  uint now = millis();
  if(now - stateNeg1Limiter > 1200) { //this improves loop fps to 8k fps from 21fps
    digitalWrite(ledPin,HIGH);//disable led for this hang
    stateNeg1Limiter=now;
    if (existFile("/WiFi_ssid") && existFile("/WiFi_password")) {
      TestWifiConn(getFile("/WiFi_ssid"), getFile("/WiFi_password"));
      SetState(-2);
    }

  }
}

// -2 state
void ProcessWifiCredsTesting() {
  double s_val = sin(millis() / 750.0 * 3.1415);
  printLed(1 - abs(s_val));  // inverse of sin, since i want more blinks, less light

  if (WiFi.status() == WL_CONNECTED) {
    SwapWifiConn();
    SetState(1);
  }
}


// 0 state
void ProcessBootup() {
  double val = (millis() - 100) / 2000.0;
  printLed(abs(val));
  if (millis() > 3000) {
    deleteFile("/bootAttempts.txt");
    if (existFile("/WiFi_ssid") && existFile("/WiFi_password")) {
      ConnectWiFi(getFile("/WiFi_ssid"), getFile("/WiFi_password"));
      SetState(1);
    } else {
      CreateWiFi();
      SetState(-1);
    }
  }
}

//  1 state
void ProcessConnectingToWifi() {
  printLed((millis() % 500) > 400);

  if (WiFi.status() == WL_CONNECTED) {
    SetState(2);
    configureMQTT();
  }
}

// 2 state
void ProcessConnectingToMQTT() {
  Serial.println("Processing state 2");  // client.connect freezes code for ~2 secs

  if (WiFi.status() != WL_CONNECTED)
    SetState(1);

  long now = millis();
  if (now - lastMqttAction > 5000) {
    client.connect(clientId.c_str());
    lastMqttAction = now;
  }
  if (client.connected()) {
    SetState(3);
  }
}

// 3 state
void ProcessWorkingState() {
  printLed(millis() % 2100 > 2050);

  if (WiFi.status() != WL_CONNECTED)
    SetState(1);
  if (!client.connected())
    SetState(2);

  long now = millis();
  if (now - lastMqttAction > 5000) {
    lastMqttAction = now;
    client.publish("test/12345/led", "On");
    //Serial.println("Publishing");
  }
}