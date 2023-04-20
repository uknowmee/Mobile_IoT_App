AsyncWebServer server(80);

const char index_fileUploader[] PROGMEM = R"rawliteral(
<html><head><title>ESP Upload Form</title></head>
<body>
  <form action="/UploadFile">
    <div>FileName: <input type="text" name="fileName"></div>
    <div>FileContent: <textarea name="fileContent" cols="100" rows="40" style="width=100%;height=auto;"></textarea></div>
    <input type="submit" value="Submit">
  </form>
  <!-- INFO -->
</body></html>)rawliteral";

//below is index that somehow works, while the current one does not. Reqires analysys
const char index_html[] PROGMEM = R"rawliteral(
<!DOCTYPE HTML><html><head>
  <title>ESP Input Form</title>
  <meta name="viewport" content="width=device-width, initial-scale=1">
  </head><body>
  <form action="/SetWifiData">
    Target wifi SSID: <input type="text" name="input1">
    Target wifi Password: <input type="password" name="input2">
    <input type="submit" value="Submit">
    <!-- INFO -->
  </form>
</body></html>)rawliteral";

String GetIndexPage() {
  return GetIndexPage("").c_str();
}

String GetIndexPage(String info) {
  String indexTemp = getFile("/index.html");
  indexTemp.replace("WIFI_SSID", getFile("WiFi_ssid"));
  indexTemp.replace("WIFI_PASSWORD", getFile("WiFi_password"));
  indexTemp.replace("CLEAR_DATA_DISABLE", conditionalString((existFile("/WiFi_ssid") || existFile("/WiFi_password")), "", "disabled"));
  indexTemp.replace("<!-- INFO -->", info);
  return indexTemp;
}

void notFound(AsyncWebServerRequest *request) {
  request->send(404, "text/plain", "Not found");
}

void SetWifiSettingsRequest(AsyncWebServerRequest *request) {
  String inputSsid;
  String inputPass;
  // GET input1 value on <ESP_IP>/get?input1=<inputMessage>
  if (request->hasParam("input1") && request->hasParam("input2")) {
    inputSsid = request->getParam("input1")->value();
    inputPass = request->getParam("input2")->value();
    writeFile("/WiFi_ssid", inputSsid);
    writeFile("/WiFi_password", inputPass);
    ChangeTargetWifi(inputSsid, inputPass);
    request->send_P(200, "text/html", GetIndexPage("New Wifi Config Set").c_str());
  }
  request->send_P(200, "text/html", GetIndexPage().c_str());
}

void IndexRequest(AsyncWebServerRequest *request) {
  request->send_P(200, "text/html", GetIndexPage().c_str());
}

void ListFilesRequest(AsyncWebServerRequest *request) {
  String body = GetFilesWithContent();
  body += GetDelButtons();
  request->send_P(200, "text/html", body.c_str());
}

void RestartRequest(AsyncWebServerRequest *request) {
  request->send_P(200, "text/html", "Restarting");
  ESP.restart();
}

void UploadFileRequest(AsyncWebServerRequest *request) {
  String fileName;
  String fileContent;
  String ending = "";

  if (request->hasParam("fileName") && request->hasParam("fileContent")) {
    fileName = request->getParam("fileName")->value();
    fileContent = request->getParam("fileContent")->value();
    writeFile("/" + fileName, fileContent);
    ending = "File uploaded\n" + fileName + ":\n<xmp>" + fileContent + "</xmp>";  //xmp -> do not interpret content to html document
  }

  String temp = index_fileUploader;
  temp.replace("<!-- INFO -->", ending);
  request->send_P(200, "text/html", temp.c_str());
}

//------------------------------------------SETUP----------------------------------------//
void setupHttpServer() {

  server.on("/", HTTP_GET, IndexRequest);
  server.on("/SetWifiData", HTTP_GET, SetWifiSettingsRequest);
  server.on("/ClearData", HTTP_GET, [](AsyncWebServerRequest *request) {
    Debug_ClearConfigFiles();
    request->send_P(200, "text/html", GetIndexPage().c_str());
  });
  server.on("/reboot", HTTP_GET, RestartRequest);
  server.on("/restart", HTTP_GET, RestartRequest);
  server.onNotFound(notFound);

#ifdef Developing
  //------------------------------------------------------------------//file stuff------------------------------------------------------------------//
  server.on("/UploadFile", HTTP_GET, UploadFileRequest);
  server.on("/ListFiles", HTTP_GET, ListFilesRequest);
  server.on("/DeleteFiles", HTTP_GET, [](AsyncWebServerRequest *request) {
    DeleteAllFiles();
    request->send_P(200, "text/html", GetIndexPage("All files deleted").c_str());
  });
  server.on("/DeleteFile", HTTP_GET, [](AsyncWebServerRequest *request) {
    if (request->hasParam("FileToDelete")) {
      String fileName = request->getParam("FileToDelete")->value();
      deleteFile(fileName);
      ListFilesRequest(request);
      return;
    }
    request->send_P(200, "text/html", GetIndexPage("Not deleted anything").c_str());
  });
#endif
  //------------------------------------------------------------------end of file stuff------------------------------------------------------------------//

  server.begin();
}