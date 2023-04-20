import 'package:flutter/foundation.dart';

class Topic {
  late final int id;
  late final String name;

  Topic({
    required this.id,
    required this.name,
  });

  factory Topic.fromAPIResponse(Map<String, dynamic> json) {
    return Topic(
      id: json["topicId"] as int,
      name: json["name"] as String,
    );
  }

  factory Topic.fromJson(Map<String, dynamic> json) {
    return Topic(
      id: json["id"] as int,
      name: json["name"] as String,
    );
  }

  Map<String, dynamic> toJson() => {
        "id": id,
        "name": name,
      };
}
