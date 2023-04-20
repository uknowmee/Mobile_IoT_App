import 'package:flutter/foundation.dart';

class TopicData {
  late final int id;
  late final String topicName;
  late final DateTime createdAt;
  // we are using this value so it has to be initialized at object creation
  late final String data;

  TopicData({
    required this.id,
    required this.topicName,
    required this.createdAt,
    required this.data,
  });

  factory TopicData.fromAPIResponse(Map<String, dynamic> json) {
    return TopicData(
      id: json["topicDataId"] as int,
      topicName: json["topicName"] as String,
      createdAt: DateTime.parse(json["createdAt"]),
      data: json["data"] as String,
    );
  }

  factory TopicData.fromJson(Map<String, dynamic> json) {
    return TopicData(
      id: json["id"] as int,
      topicName: json["topicName"] as String,
      createdAt: DateTime.parse(json["createdAt"]),
      data: json["data"] as String,
    );
  }

  Map<String, dynamic> toJson() => {
        "id": id,
        "topicName": topicName,
        "createdAt": createdAt.toString(),
        "data": data,
      };
}
