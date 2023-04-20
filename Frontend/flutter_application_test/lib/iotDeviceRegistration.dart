import 'dart:async';

import 'package:go_router/go_router.dart';
import 'package:iot_demo_app/models/iotDeviceModel.dart';
import 'package:flutter/material.dart';
import 'package:fluttertoast/fluttertoast.dart';
import 'dart:convert';
import 'package:http/http.dart' as http;
import 'package:shared_preferences/shared_preferences.dart';
import 'API/appServerAPI/registeredIoTDeviceAPI.dart';
import 'API/iotDeviceAPI/iotDeviceAPI.dart';
import 'API/appServerAPI/apiConfig.dart' as appServerAPIConfig;
import 'API/iotDeviceAPI/apiConfig.dart' as iotDeviceAPIConfig;
import 'utils/toastMessages.dart';

class IoTDeviceRegistrationWidget extends StatefulWidget {
  const IoTDeviceRegistrationWidget({super.key, required this.title});

  final String title;

  @override
  State<IoTDeviceRegistrationWidget> createState() =>
      _IoTDeviceRegistrationWidgetState();
}

class _IoTDeviceRegistrationWidgetState
    extends State<IoTDeviceRegistrationWidget> {
  // Get shared preferences reference
  final Future<SharedPreferences> _prefs = SharedPreferences.getInstance();

  // Shared preferences variables references
  late Future<String> _userSessionToken;
  late Future<String> _userEmail;
  late Future<List<IoTDevice>> _addedIoTDevices;

  late Timer _uiRefreshTimer;

  // Define UI controllers
  // Set default text for server ip
  TextEditingController _apiIPandPortController = TextEditingController(
      text: "${appServerAPIConfig.appServerIP}:${appServerAPIConfig.port}");
  // Set default text for server ip
  TextEditingController _httpServerIPandPortController = TextEditingController(
      text: "${iotDeviceAPIConfig.iotDeviceIP}:${iotDeviceAPIConfig.port}");
  // Set default data for network SSID and password
  TextEditingController _SSIDController =
      TextEditingController(text: "Uknowme");
  TextEditingController _NetworkPasswordController =
      TextEditingController(text: "12345678");

  Future<bool> _isIoTDeviceAlive() async {
    bool isIoTDeviceAlive =
        await isAlive(ipAndPort: _httpServerIPandPortController.text.trim());
    // if network is IoT Device AP return it's name
    return isIoTDeviceAlive;
  }

  Future<bool> _addNewIoTDevice() async {
    final SharedPreferences prefs = await _prefs;
    List<IoTDevice> addedIoTDevices = await _addedIoTDevices;

    const String logMessage = "Add new IoT device button pressed";
    print(logMessage);

    // Remove leading and trailing white spaces from input fields
    String ipAndPort = _httpServerIPandPortController.text.trim();
    String ssid = _SSIDController.text.trim();
    String password = _NetworkPasswordController.text;

    // get current device model from network SSID
    String? currDeviceModel = await getIoTDeviceModel();
    // get current network MAC address for ip from ipAndPort
    String? currDeviceMAC = await getIoTDeviceMAC();

    bool success = await setWiFiData(ssid, password, ipAndPort: ipAndPort);

    print("SetWiFiData API call ended, success: ${success.toString()}");
    print(
        "Curr Network SSID: $currDeviceModel, Curr Gateway MAC: $currDeviceMAC");

    // append new device data to local variable _addedIoTDevices
    // update preferences variable _addedIoTDevices
    if (success == true && currDeviceModel != null && currDeviceMAC != null) {
      IoTDevice newIoTDevice =
          IoTDevice.unregistered(model: currDeviceModel, mac: currDeviceMAC);

      if (addedIoTDevices.contains(newIoTDevice)) {
        displayToastError("Error! Device already added");
        return false;
      } else {
        // add newly added iot device to added (yet unregistered) devices list
        addedIoTDevices.add(newIoTDevice);
        setState(() {
          _addedIoTDevices = prefs
              .setString('addedIoTDevices', jsonEncode(addedIoTDevices))
              .then((bool success) {
            return addedIoTDevices;
          });
        });
        displayToastSuccess("Success! Added new IoT Device");
        return true;
      }
    } else {
      displayToastError("Error! Could not add IoT Device");
      return false;
    }
  }

  Future<bool> _registerNewIoTDevice(IoTDevice ioTDevice) async {
    final SharedPreferences prefs = await _prefs;

    // Get session token for curr user
    final String currSessionToken = await _userSessionToken;
    final String apiIPandPort = _apiIPandPortController.text.trim();

    // Handle IoT Device registration Action
    bool status = await registerIoTDevice(
        currSessionToken, ioTDevice.model, ioTDevice.mac,
        ipAndPort: apiIPandPort);
    // if registration successful
    if (status) {
      // Display toast message - successful iot device registration
      displayToastSuccess("Registered new IoT device to current User");
      // Return success = true
      return true;
    } else {
      // Registration failed for some reason
      // Display toast message - Registration filed, check interned connection
      displayToastError(
        "New IoT Device registration filed!\n Check interned connection and credentials.",
      );
      // Return success = false
      return false;
    }
  }

  Future<bool> _removeAddedDevice(IoTDevice device) async {
    final SharedPreferences prefs = await _prefs;
    List<IoTDevice> addedIoTDevices = await _addedIoTDevices;

    if (addedIoTDevices.contains(device)) {
      var result = addedIoTDevices.remove(device);
      if (result != false) {
        setState(() {
          _addedIoTDevices = prefs
              .setString('addedIoTDevices', jsonEncode(addedIoTDevices))
              .then((bool success) {
            return addedIoTDevices;
          });
        });
        displayToastSuccess("Removed device from list");
        return true;
      } else {
        displayToastError("Could not remove device from added devices list");
        return false;
      }
    } else {
      displayToastError("No such device on added devices list!");
      return false;
    }
  }

  @override
  void initState() {
    // initialize initial state
    super.initState();
    // TODO: Remove ghost code (this timer does not work as intended)
    // _uiRefreshTimer =
    //     Timer.periodic(const Duration(milliseconds: 3000), (Timer t) {
    //   if (mounted) {
    //     setState(() {});
    //   }
    // });

    // overwrite initial state with values stored in shared preferences
    _userSessionToken = _prefs.then((SharedPreferences prefs) {
      return prefs.getString('userSessionToken') ?? "";
    });
    _userEmail = _prefs.then((SharedPreferences prefs) {
      return prefs.getString('userEmail') ?? "";
    });
    _addedIoTDevices = _prefs.then((SharedPreferences prefs) {
      Iterable l =
          json.decode(prefs.getString('addedIoTDevices') ?? jsonEncode([]));
      List<IoTDevice> newIoTDevicesList =
          List<IoTDevice>.from(l.map((device) => IoTDevice.fromJson(device)));
      return newIoTDevicesList;
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
      drawer: Drawer(
        // Add a ListView to the drawer. This ensures the user can scroll
        // through the options in the drawer if there isn't enough vertical
        // space to fit everything.
        child: ListView(
          // Important: Remove any padding from the ListView.
          padding: EdgeInsets.zero,
          // Try to populate drawer
          children: [
            // build drawer header
            FutureBuilder<String>(
                future: _userEmail,
                builder: (context, snapshot) {
                  if (snapshot.hasData) {
                    return DrawerHeader(
                        decoration: const BoxDecoration(
                          color: Colors.blue,
                        ),
                        child: ListView(
                          padding: EdgeInsets.zero,
                          children: <Widget>[
                            Container(
                              alignment: Alignment.center,
                              padding: const EdgeInsets.all(10),
                              child: const Text(
                                'Paired IoT Devices',
                                style: TextStyle(
                                    color: Colors.white,
                                    fontWeight: FontWeight.w500,
                                    fontSize: 30),
                              ),
                            ),
                            Container(
                              alignment: Alignment.center,
                              padding: const EdgeInsets.all(10),
                              child: Text(
                                'User: ${snapshot.data}',
                                style: const TextStyle(
                                    color: Colors.white,
                                    fontWeight: FontWeight.w900,
                                    fontSize: 20),
                              ),
                            )
                          ],
                        ));
                  } else {
                    return DrawerHeader(
                        decoration: const BoxDecoration(
                          color: Colors.blue,
                        ),
                        child: ListView(
                          padding: EdgeInsets.zero,
                          children: <Widget>[
                            Container(
                              alignment: Alignment.center,
                              padding: const EdgeInsets.all(10),
                              child: const Text(
                                'Paired IoT Devices',
                                style: TextStyle(
                                    color: Colors.white,
                                    fontWeight: FontWeight.w500,
                                    fontSize: 30),
                              ),
                            ),
                            Container(
                              alignment: Alignment.center,
                              padding: const EdgeInsets.all(10),
                              child: const Text(
                                'No user logged in!',
                                style: TextStyle(
                                    color: Colors.grey,
                                    fontWeight: FontWeight.w400,
                                    fontSize: 20),
                              ),
                            )
                          ],
                        ));
                  }
                }),
            FutureBuilder<List<IoTDevice>>(
                future: _addedIoTDevices,
                builder: (context, snapshot) {
                  if (!snapshot.hasData) {
                    // returns null -> interprete as waiting for data
                    return const Center(child: CircularProgressIndicator());
                  }
                  List<IoTDevice> iotDevices = snapshot.data as List<IoTDevice>;
                  return ListView.builder(
                    shrinkWrap: true,
                    itemCount: iotDevices.length,
                    itemBuilder: (context, index) {
                      IoTDevice iotDevice = iotDevices[index];
                      return ListTile(
                        leading: const IconButton(
                          icon: Icon(
                            Icons.device_hub_rounded,
                            color: Colors.grey,
                            size: 25,
                          ),
                          onPressed: null,
                        ),
                        title: Text(iotDevice.model),
                        subtitle: Text(iotDevice.mac),
                        trailing: IconButton(
                          icon: const Icon(
                            Icons.remove_circle,
                            color: Colors.red,
                            size: 35,
                          ),
                          onPressed: () => _removeAddedDevice(iotDevice),
                        ),
                        onTap: () {
                          // Update the state of the app.
                          // ...
                        },
                      );
                    },
                  );
                }),
          ],
        ),
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
                  'Device Registration page',
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
                controller: _httpServerIPandPortController,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  labelText: 'IoT Device HTTP Server IP',
                ),
              ),
            ),
            Container(
              padding: const EdgeInsets.all(10),
              child: TextField(
                controller: _SSIDController,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  labelText: 'SSID',
                ),
              ),
            ),
            Container(
              padding: const EdgeInsets.fromLTRB(10, 10, 10, 20),
              child: TextField(
                obscureText: true,
                controller: _NetworkPasswordController,
                decoration: const InputDecoration(
                  border: OutlineInputBorder(),
                  labelText: 'Network Password',
                ),
              ),
            ),
            Container(
                height: 50,
                padding: const EdgeInsets.fromLTRB(10, 0, 10, 0),
                child: FutureBuilder<bool>(
                    future: _isIoTDeviceAlive(),
                    builder: (context, snapshot) {
                      if (!snapshot.hasData || snapshot.data == false) {
                        // current network is not IoT Device AP
                        // show disabled button
                        return const ElevatedButton(
                          onPressed: null,
                          child: Text('Add device'),
                        );
                      }
                      // current network is IoT Device AP
                      // show enabled button
                      return ElevatedButton(
                        onPressed: () => _addNewIoTDevice(),
                        child: const Text('Add device'),
                      );
                    })),
            Row(
              mainAxisAlignment: MainAxisAlignment.center,
              children: const <Widget>[
                Text(
                    'Make sure you are connected to the device AP\n You might need to switch on GPS'),
              ],
            ),
          ],
        ),
      ),
      floatingActionButton: FloatingActionButton(
        // on press move to chart widget (user home)
        onPressed: () => {
          // TODO: We should be checking here if user has connection to app server
          // register all iot devices in drawer
          _addedIoTDevices.then((List<IoTDevice> addedDevices) {
            addedDevices.forEach((IoTDevice addedDevice) {
              _registerNewIoTDevice(addedDevice);
            });
          }),
          // empty the added devices list (and remove value from shared preferences)
          setState(() {
            _addedIoTDevices = _prefs.then((SharedPreferences prefs) {
              return prefs
                  .setString('addedIoTDevices', jsonEncode([]))
                  .then((bool success) {
                return [];
              });
            });
          }),
          GoRouter.of(context).go('/home')
        },
        tooltip: 'Go to user home page',
        child: const Icon(Icons.arrow_back),
      ), // This trailing comma makes auto-formatting nicer for build methods.
    );
  }
}
