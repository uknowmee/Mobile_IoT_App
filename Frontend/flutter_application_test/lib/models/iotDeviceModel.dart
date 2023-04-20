import 'package:flutter/foundation.dart';
import 'package:iot_demo_app/models/topicModel.dart';

class IoTDevice {
  late final int id;
  late final DateTime registrationDate;
  late final String model;
  late final List<Topic> topics; // list of topic names
  late final String mac;
  late bool stateLED; // led state for device has mutable value

  IoTDevice({
    required this.id,
    required this.registrationDate,
    required this.model,
    required this.topics,
    required this.mac,
    required this.stateLED,
  });

  IoTDevice.unregistered({required this.model, required this.mac}) {
    // initialize all unregistered iot devices with the same data
    id = 0;
    registrationDate = DateTime.parse("1970-01-01 12:00:00.000000Z");
    topics = [];
    stateLED = false;
  }

  factory IoTDevice.fromAPIResponse(Map<String, dynamic> json) {
    var topicObjsJson = json['topics'] as List;
    List<Topic> _topics = [];

    if (topicObjsJson.isNotEmpty) {
      _topics = topicObjsJson
          .map((topicJsonObj) => Topic.fromAPIResponse(topicJsonObj))
          .toList();
    }

    return IoTDevice(
        id: json["deviceId"] as int,
        registrationDate: DateTime.parse(json['registrationDate']),
        model: json['model'] as String,
        topics: _topics,
        mac: json['mac'] as String,
        stateLED: json['ledState'] as bool);
  }

  factory IoTDevice.fromJson(Map<String, dynamic> json) {
    var topicObjsJson = json['topics'] as List;
    List<Topic> _topics = [];

    if (topicObjsJson.isNotEmpty) {
      _topics = topicObjsJson
          .map((topicJsonObj) => Topic.fromJson(topicJsonObj))
          .toList();
    }

    return IoTDevice(
        id: json["id"] as int,
        registrationDate: DateTime.parse(json['registrationDate']),
        model: json['model'] ?? "",
        topics: _topics,
        mac: json['mac'] ?? "",
        stateLED: json['ledState'] as bool);
  }

  Map<String, dynamic> toJson() => {
        "id": id,
        "registrationDate": registrationDate.toString(),
        "model": model,
        "topics": topics.map((topic) => topic.toJson()).toList(),
        "mac": mac,
        "ledState": stateLED
      };

  @override
  bool operator ==(Object other) {
    if (other is IoTDevice) {
      return (mac == other.mac && id == other.id);
    } else {
      return false;
    }
  }

  @override
  int get hashCode {
    return id;
  }
}
