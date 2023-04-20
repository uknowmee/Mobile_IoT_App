import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:iot_demo_app/models/iotDeviceModel.dart';
import 'package:iot_demo_app/models/topicDataModel.dart';
import 'package:iot_demo_app/models/topicModel.dart';
import 'apiConfig.dart';

// get registered devices list for user with session token
Future<List<IoTDevice>> getRegisteredIoTDevices(String sessionToken,
    {String ipAndPort = "$appServerIP:$port"}) async {
  // define API action
  const String action = "list";

  // Handle IoT Device fetch Action
  // Create get request query
  String getRequest =
      "$protocol://$ipAndPort/$apiRoot$iotDeviceApi$action?sessionToken=$sessionToken";

  print("API CALL URI: $getRequest");

  try {
    final response = await http.get(Uri.parse(getRequest));

    // if fetch successful
    if (response.statusCode == 200) {
      // Decode list of IoT Devices form response json
      Iterable l = json.decode(response.body);

      List<IoTDevice> newIoTDevicesList = List<IoTDevice>.from(
          l.map((device) => IoTDevice.fromAPIResponse(device)));

      return newIoTDevicesList;
    } else {
      return [];
    }
  } catch (e) {
    // MOST likely connection timeout => treat as timeout
    print("Request exception: \"${e.toString()}\", request: $getRequest");
    return [];
  }
}

// register new iot device for user with session token
Future<bool> registerIoTDevice(
    String sessionToken, String iotDeviceModel, String ioTDeviceMAC,
    {String ipAndPort = "$appServerIP:$port"}) async {
  // define API action
  const String action = "register";

  // Handle IoT Device fetch Action
  // Create get request query
  String postRequest =
      "$protocol://$ipAndPort/$apiRoot$iotDeviceApi$action?sessionToken=$sessionToken&deviceModel=$iotDeviceModel&deviceMac=$ioTDeviceMAC";

  print("API CALL URI: $postRequest");

  try {
    final response = await http.post(Uri.parse(postRequest));

    // if post successful
    if (response.statusCode == 200) {
      return true;
    } else {
      return false;
    }
  } catch (e) {
    // MOST likely connection timeout => treat as timeout
    print("Request exception: \"${e.toString()}\", request: $postRequest");
    return false;
  }
}

// get data for user registered device for mqtt topic with given name
Future<List<TopicData>> getTopicData(
    String sessionToken, String deviceMAC, String topicName,
    {String ipAndPort = "$appServerIP:$port"}) async {
  // define API action
  const String action = "data";

  // Handle Topic Data fetch Action
  // Create get request query
  String getRequest =
      "$protocol://$ipAndPort/$apiRoot$iotDeviceApi$action?sessionToken=$sessionToken&deviceMac=$deviceMAC&topicName=$topicName";

  print("API CALL URI: $getRequest");

  try {
    final response = await http.get(Uri.parse(getRequest));

    // if fetch successful
    if (response.statusCode == 200) {
      // Decode list of Topic Data form response json
      Iterable l = json.decode(response.body);

      List<TopicData> newTopicDataList = List<TopicData>.from(
          l.map((topicData) => TopicData.fromAPIResponse(topicData)));

      return newTopicDataList;
    } else {
      return [];
    }
  } catch (e) {
    // MOST likely connection timeout => treat as timeout
    print("Request exception: \"${e.toString()}\", request: $getRequest");
    return [];
  }
}

// get possible topics list for selected device and user
Future<List<Topic>> getTopics(String sessionToken, String deviceMAC,
    {String ipAndPort = "$appServerIP:$port"}) async {
  // define API action
  const String action = "topics";

  // Handle Topic Data fetch Action
  // Create get request query
  String getRequest =
      "$protocol://$ipAndPort/$apiRoot$iotDeviceApi$action?sessionToken=$sessionToken&deviceMac=$deviceMAC";

  print("API CALL URI: $getRequest");

  try {
    final response = await http.get(Uri.parse(getRequest));

    // if fetch successful
    if (response.statusCode == 200) {
      // Decode list of Topics form response json
      Iterable l = json.decode(response.body);

      List<Topic> newTopicsList =
          List<Topic>.from(l.map((topic) => Topic.fromAPIResponse(topic)));

      return newTopicsList;
    } else {
      return [];
    }
  } catch (e) {
    // MOST likely connection timeout => treat as timeout
    print("Request exception: \"${e.toString()}\", request: $getRequest");
    return [];
  }
}



// TODO: implement api wrapper function for IoTDevice led state change
