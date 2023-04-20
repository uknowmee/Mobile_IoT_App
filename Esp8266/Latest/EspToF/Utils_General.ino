void RevertToFactorySettings() {
  DeleteAllFiles();
  WiFi.mode(WIFI_OFF);
  WiFi.disconnect();
  WiFi.softAPdisconnect(true);
}

void DeleteAllFiles() {
  Dir dir = SPIFFS.openDir("");
  while (dir.next()) {
    deleteFile(dir.fileName());
  }
}

ulong frames = 0;
ulong lastSec = 0;
void ProcessFps() {
  frames++;
  uint sec = millis() / 1000;
  if (sec != lastSec) {
    lastSec = sec;
    if (frames < 100) {
      Serial.println("Throttle warning: fps = " + String(frames));
    }
    lastFrameFPS=frames;
    frames = 0;
  }
}

//Typical:
// 18000 fps for simple led
// 10000 fps for sin() based led

long long_abs(long x) {
  if (x > 0) return x;
  return -1 * x;
}

String conditionalString(bool b, String ifTrue, String ifFalse) {
  if (b) {
    return ifTrue;
  }
  return ifFalse;
}
