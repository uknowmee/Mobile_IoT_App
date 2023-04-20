import 'package:flutter/material.dart';
import 'dart:convert';
import 'package:iot_demo_app/models/iotDeviceModel.dart';
import 'package:iot_demo_app/utils/toastMessages.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:go_router/go_router.dart';
import 'API/appServerAPI/apiConfig.dart';
import 'API/appServerAPI/registeredIoTDeviceAPI.dart';
import 'API/appServerAPI/userAPI.dart';

class LoginWidget extends StatefulWidget {
  const LoginWidget({super.key, required this.title});

  final String title;

  @override
  State<LoginWidget> createState() => _LoginWidgetState();
}

class _LoginWidgetState extends State<LoginWidget> {
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

  Future<bool> _login() async {
    final SharedPreferences prefs = await _prefs;

    // Remove leading and trailing white spaces from input fields
    String apiIPandPort = _apiIPandPortController.text.trim();
    String email = _emailController.text.trim();
    String password = _passwordController.text;

    // Handle Login Action
    final String? newUserSessionToken =
        await login(email, password, ipAndPort: apiIPandPort);

    // if login successful
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

      // Display toast message - successful login
      displayToastSuccess("Successful login");
      // Return success = false
      return true;
    } else {
      // login failed for some reason
      // Display toast message - login filed, check interned connection and credentials
      displayToastError(
          "Login filed!\n Check interned connection and credentials.");
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
                  'Login page',
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
                height: 50,
                padding: const EdgeInsets.fromLTRB(10, 0, 10, 0),
                child: ElevatedButton(
                  child: const Text('Log In'),
                  onPressed: () {
                    _login().then((success) => {
                          // if login was successfull
                          if (success == true)
                            {
                              // Move to chart widget
                              GoRouter.of(context).go('/home')
                            }
                        });
                  },
                )),
            Row(
              children: <Widget>[
                const Text('No account yet?'),
                TextButton(
                  child: const Text(
                    'Sign in',
                    style: TextStyle(fontSize: 20),
                  ),
                  onPressed: () => GoRouter.of(context).go('/register'),
                )
              ],
              mainAxisAlignment: MainAxisAlignment.center,
            ),
          ],
        ),
      ), // This trailing comma makes auto-formatting nicer for build methods.
    );
  }
}
