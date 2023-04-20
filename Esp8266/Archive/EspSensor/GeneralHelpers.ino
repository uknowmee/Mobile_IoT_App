static const double globalLedDim = 0.40;

void printLed(double curve) {          //curve should be a value between 0 and 1
  double c2 = eyeCompensation(curve);  //this is to compensate for the human eye,
  //makes difference between 0.8 and 0.9 same as between 0.1 and 0.2
  //double c3=avoidTooLowValues(c2);  //fixed by manualing the oscilator
  c2 *= globalLedDim;
  digitalWrite(ledPin, c2 <= oscilator());  //led is negated, so to capture
  //the surface under the curve, we have to actually ask if sampler is above it
  // '<=' not '=' cause if c3==0, led should be off => condition always true
}
void printLedRaw(double curve) {  //used for warning blinks
  digitalWrite(ledPin, curve <= oscilator());
}
void Debug_printLedRaw(double curve) {  //test the oscilator
  digitalWrite(ledPin, curve <= manualOscilator());
}
double eyeCompensation(double x) {
  if (globalLedDim >= 0.7)
    return x * x * x;
  if (globalLedDim >= 0.15)
    return x * x;
  return x;
  /*if(eyeMode==0)
  return x*x*x;
  if(eyeMode==1)
  return x*x;
  return x;*/
}
/*double avoidTooLowValues(double x) { // obsolete function
  return 0.1 + x * 0.9;
}*/
//first gen oscilator. Decent, but jitters when too low values are given for comparison
double oscilator() {
  return ((micros() % 1000) / 1000.0);  //this returns 0-1 value that repeats itself
  //every millisecond, used with comparator (like '>') we can drive LED with PWM.
  //Concept stolen from D-type audio ampiflier :)
}

//Manual time counting that allows for even dimmer values, but may jitter all the time
unsigned int manualOscilatorVal = 0;

double manualOscilator() {
  manualOscilatorVal=(manualOscilatorVal+1)%200;
  return manualOscilatorVal / 200.0;
}

String GetFilesWithContent() {
  String filesWithContent = "";
  filesWithContent = "<xmp>";

  fs::Dir dir = SPIFFS.openDir("");
  while (dir.next()) {
    File f = dir.openFile("r");
    filesWithContent += String(dir.fileName()) + " size:" + String(f.size()) + "\n";
    filesWithContent += String(getFile(dir.fileName())) + "\n\n";
  }

  filesWithContent += "</xmp>";
  return filesWithContent;
}

void DeleteAllFiles() {
  Dir dir = SPIFFS.openDir("");
  while (dir.next()) {
    deleteFile(dir.fileName());
  }
}

String GetDelButtons() {
  String buttonsTemplate = R"rawliteral(<form action="/DeleteFile"><input type="submit" name="FileToDelete" value="FILENAME"></form>)rawliteral";
  String buttons = "";

  Dir dir = SPIFFS.openDir("");
  while (dir.next()) {
    File f = dir.openFile("r");
    String buttonsTemplateCpy = buttonsTemplate;
    buttonsTemplateCpy.replace("FILENAME", dir.fileName());
    buttons += buttonsTemplateCpy;
  }
  return buttons;
}

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

String GenerateSSID() {
  String mac = String(WiFi.macAddress());
  mac.replace(":", "");
  return "SmartDevice " + mac.substring(4, 7) + "-" + mac.substring(7, 12);
}
String GeneratePassword() {
  String mac = String(WiFi.macAddress());
  mac.replace(":", "");
  return mac.substring(4, 12).c_str();
}