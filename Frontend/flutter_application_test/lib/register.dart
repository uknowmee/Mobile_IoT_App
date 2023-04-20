import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'dart:convert';
import 'package:iot_demo_app/API/appServerAPI/apiConfig.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'API/appServerAPI/registeredIoTDeviceAPI.dart';
import 'API/appServerAPI/userAPI.dart';
import 'models/iotDeviceModel.dart';
import 'utils/toastMessages.dart';

class RegisterWidget extends StatefulWidget {
  const RegisterWidget({super.key, required this.title});

  final String title;

  @override
  State<RegisterWidget> createState() => _RegisterWidgetState();
}

class _RegisterWidgetState extends State<RegisterWidget> {
  // Get shared preferences reference
  final Future<SharedPreferences> _prefs = SharedPreferences.getInstance();

  // Shared preferences variables references
  late Future<String> _userSessionToken;
  late Future<String> _userEmail;
  late Future<List<IoTDevice>> _addedIoTDevices;

  // Define UI controllers
  // Set default text for server ip
  TextEditingController _apiIPandPortController =
      TextEditingController(text: "$appServerIP:$port");
  // Set default data for user login (email) and password
  TextEditingController _emailController =
      TextEditingController(text: "test.register@email.com");
  TextEditingController _passwordController =
      TextEditingController(text: "Password");
  TextEditingController _passwordCheckController =
      TextEditingController(text: "Password");

  bool isPasswordMatch(String password, String passwordCheck) {
    return password == passwordCheck;
  }

  Future<bool> _register() async {
    final SharedPreferences prefs = await _prefs;
    // Remove leading and trailing white spaces from input fields
    String apiIPandPort = _apiIPandPortController.text.trim();
    String email = _emailController.text.trim();
    String password = _passwordController.text;
    String passwordCheck = _passwordCheckController.text;

    // make sure passwords match before registering new user
    if (isPasswordMatch(password, passwordCheck)) {
      // Handle Registering Action
      String? newUserSessionToken =
          await register(email, password, ipAndPort: apiIPandPort);

      // if registration successfull
      if (newUserSessionToken != null) {
        // Set new value for local and shared preferences variable
        setState(() {
          _userSessionToken = prefs
              .setString('userSessionToken', newUserSessionToken)
              .then((bool success) {
            return newUserSessionToken;
          });
        });
        setState(() {
          _userEmail = prefs.setString('userEmail', email).then((bool success) {
            return email;
          });
        });
        // remember the api IP and Port
        prefs.setString('apiIPandPort', apiIPandPort);

        // Display toast message - successful registration
        displayToastSuccess("Successful registration");
        // Return success = true
        return true;
      } else {
        // Registration failed for some reason
        // Display toast message - Registration filed, check interned connection and credentials
        displayToastError(
          "Registration filed!\n Check interned connection and credentials.",
        );
        // Return success = false
        return false;
      }
    } else {
      // Display toast message - passwords must match
      displayToastError("Passwords do not match!");
      // Return success = false
      return false;
    }
  }

  @override
  void initState() {
    // initialize initial state
    super.initState();
    // overwrite initial state with values stored in shared preferences
    _userSessionToken = _prefs.then((SharedPreferences prefs) {
      return prefs.getString('userSessionToken') ?? "";
    });
    _userEmail = _prefs.then((SharedPreferences prefs) {
      return prefs.getString('userEmail') ?? "";
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(
        // Here we take the value from the HomePage object that was created by
        // the App.build method, and use it to set our appbar title.
        title: Text(widget.title),
      ),
      body: Padding(
        padding: const EdgeInsets.all(10),
        child: ListView(
          children: <Widget>[
            Container(
                alignment: Alignment.center,
                padding: const EdgeInsets.all(10),
                child: const Text(
                  'IoT Demo App',
                  style: TextStyle(
                      color: Colors.blue,
                      fontWeight: FontWeight.w500,
                      fontSize: 30),
                )),
            Container(
                alignment: Alignment.center,
                padding: const EdgeInsets.all(10),
                child: const Text(
                  'Register page',
                  style: TextStyle(fontSize: 20),
                )),
            Container(
              padding: const EdgeInsets.all(10),
              child: TextField(
                controller: _apiIPandPortController,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  labelText: 'App Server IP',
                ),
              ),
            ),
            Container(
              padding: const EdgeInsets.all(10),
              child: TextField(
                controller: _emailController,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  labelText: 'Email',
                ),
              ),
            ),
            Container(
              padding: const EdgeInsets.fromLTRB(10, 10, 10, 20),
              child: TextField(
                obscureText: true,
                controller: _passwordController,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  labelText: 'Password',
                ),
              ),
            ),
            Container(
              padding: const EdgeInsets.fromLTRB(10, 10, 10, 20),
              child: TextField(
                obscureText: true,
                controller: _passwordCheckController,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  labelText: 'Repeat Password',
                ),
              ),
            ),
            Container(
                height: 50,
                padding: const EdgeInsets.fromLTRB(10, 0, 10, 0),
                child: ElevatedButton(
                  child: const Text('Register'),
                  onPressed: () {
                    // register new user
                    _register().then((bool status) {
                      if (status) {
                        // Move to chart / user home page
                        GoRouter.of(context).go('/home');
                      }
                    });
                  },
                )),
          ],
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () => GoRouter.of(context).go('/login'),
        tooltip: 'Move back to login page',
        child: const Icon(Icons.arrow_back),
      ), // This trailing comma makes auto-formatting nicer for build methods.
    );
  }
}
