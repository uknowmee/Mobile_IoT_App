import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:network_info_plus/network_info_plus.dart';
import 'package:permission_handler/permission_handler.dart';
import 'apiConfig.dart';

Future<bool> setWiFiData(String ssid, String password,
    {String ipAndPort = "$iotDeviceIP:$port"}) async {
  final getRequest =
      "$protocol://$ipAndPort/$apiRoot$setWiFiDataEndpoint?wifi_ssid=$ssid&wifi_pass=$password";

  print("API CALL URI: $getRequest");

  try {
    final response = await http.get(Uri.parse(getRequest));

    // if IoT Device accepted network data
    if (response.statusCode == 200) {
      return true;
    } else {
      return false;
    }
  } catch (e) {
    // MOST likely connection timeout => treat as timeout
    print("Request exception: \"${e.toString()}\", request: $getRequest");
    return false;
  }
}

Future<bool> isAlive({String ipAndPort = "$iotDeviceIP:$port"}) async {
  final getRequest = "$protocol://$ipAndPort/$apiRoot$aliveEndpoint";

  print("API CALL URI: $getRequest");

  try {
    final response = await http.get(Uri.parse(getRequest));
    // if IoT Device is alive
    if (response.statusCode == 200) {
      return true;
    } else {
      print(
          "API CALL URI: $getRequest, request status code: ${response.statusCode}");
      return false;
    }
  } catch (e) {
    // MOST likely connection timeout => treat as timeout
    print("Request exception: \"${e.toString()}\", request: $getRequest");
    return false;
  }
}

Future<String?> getIoTDeviceModel({String ip = iotDeviceIP}) async {
  final info = NetworkInfo();
  var locationStatus = await Permission.location.status;
  if (locationStatus.isDenied) {
    await Permission.locationWhenInUse.request();
  }
  if (await Permission.location.isRestricted) {
    openAppSettings();
  }

  if (await Permission.location.isGranted) {
    String? wifiName = await info.getWifiName();
    // if got valid string
    if (wifiName != null) {
      // remove quotation marks from WiFi network name
      wifiName = wifiName.substring(1, wifiName.length - 1);
      // remove everyting past the last occurance of a space
      List<String> spaceSplitWifiName = wifiName.split(' ');
      String deviceModel = spaceSplitWifiName
          .sublist(0, spaceSplitWifiName.length - 1)
          .join(' ');

      return deviceModel;
    } else {
      // MOST likely connection timeout => treat as timeout
      print("Could not get device model from AP");
      return null; // return null if could not get wifi network name
    }
  }
}

Future<String?> getIoTDeviceMAC(
    {String ipAndPort = "$iotDeviceIP:$port"}) async {
  final getRequest = "$protocol://$ipAndPort/$apiRoot$getMacEndpoint";

  print("API CALL URI: $getRequest");

  try {
    final response = await http.get(Uri.parse(getRequest));

    // if mac fetched successfuly
    if (response.statusCode == 200) {
      String mac = response.body;
      return mac.toUpperCase();
    } else {
      print(
          "API CALL URI: $getRequest, request status code: ${response.statusCode}");
      return null;
    }
  } catch (e) {
    // MOST likely connection timeout => treat as timeout
    print("Request exception: \"${e.toString()}\", request: $getRequest");
    return null;
  }
}
