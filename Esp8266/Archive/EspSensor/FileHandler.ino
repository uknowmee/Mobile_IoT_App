String getFile(String path) {
  String buf;
  if (SPIFFS.exists(path)) {
    //Serial.println("\tFileFound");// If the file exists
    File file = SPIFFS.open(path, "r");
    while (file.available()) 
      buf += char(file.read());
    file.close();
    return buf;  // Then close the file again
  } else {
    //Serial.println("\tFileNotFound");
    return "";
  }
}

void writeFile(String path, String data, bool clearFile = true) {
  String buf;
  File file;
  if (clearFile)
    file = SPIFFS.open(path, "w");
  else
    file = SPIFFS.open(path, "a");
  file.print(data);
  file.close();
}

void deleteFile(String path) {
  SPIFFS.remove(path);
}

bool existFile(String path) {
  return SPIFFS.exists(path);
}