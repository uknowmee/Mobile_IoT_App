import 'dart:convert';
import 'package:http/http.dart' as http;
import 'apiConfig.dart';

// create new session token for login
Future<String?> login(String email, String password,
    {String ipAndPort = "$appServerIP:$port"}) async {
  const String action = "login";
  final postRequest =
      "$protocol://$ipAndPort/$apiRoot$userApi$action?email=$email&password=$password";

  print("API CALL URI: $postRequest");

  try {
    final response = await http.post(Uri.parse(postRequest));
    // if login successful
    if (response.statusCode == 200) {
      String responseData = utf8.decode(response.bodyBytes);
      final String newUserSessionToken = json.decode(responseData)["tokenHash"];

      return newUserSessionToken;
    } else {
      return null;
    }
  } catch (e) {
    // MOST likely connection timeout => treat as timeout
    print("Request exception: \"${e.toString()}\", request: $postRequest");
    return null;
  }
}

// create new session token for register
Future<String?> register(String email, String password,
    {String ipAndPort = "$appServerIP:$port"}) async {
  // define API action
  const String action = "register";
  final postRequest =
      "$protocol://$ipAndPort/$apiRoot$userApi$action?email=$email&password=$password";

  print("API CALL URI: $postRequest");

  try {
    final response = await http.post(Uri.parse(postRequest));
    // if registering successful
    if (response.statusCode == 200) {
      String responseData = utf8.decode(response.bodyBytes);
      final String newUserSessionToken = json.decode(responseData)["tokenHash"];

      return newUserSessionToken;
    } else {
      return null;
    }
  } catch (e) {
    // MOST likely connection timeout => treat as timeout
    print("Request exception: \"${e.toString()}\", request: $postRequest");
    return null;
  }
}

// logout user given session
Future<bool> logout(String sessionToken,
    {String ipAndPort = "$appServerIP:$port"}) async {
  // define API action
  const String action = "logout";
  final putRequest =
      "$protocol://$ipAndPort/$apiRoot$userApi$action?sessionToken=$sessionToken";

  print("API CALL URI: $putRequest");

  try {
    final response = await http.put(Uri.parse(putRequest));
    // if registering successful
    if (response.statusCode == 200) {
      return true;
    } else {
      return false;
    }
  } catch (e) {
    // MOST likely connection timeout => treat as timeout
    print("Request exception: \"${e.toString()}\", request: $putRequest");
    return false;
  }
}
