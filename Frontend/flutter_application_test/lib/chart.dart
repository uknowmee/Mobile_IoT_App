import 'dart:async';

import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import 'dart:convert';
import 'package:iot_demo_app/models/topicModel.dart';
import 'package:shared_preferences/shared_preferences.dart';
import 'package:fl_chart/fl_chart.dart';

import 'API/appServerAPI/registeredIoTDeviceAPI.dart';
import 'API/appServerAPI/userAPI.dart';
import 'models/iotDeviceModel.dart';
import 'models/topicDataModel.dart';
import 'utils/toastMessages.dart';

class ChartWidget extends StatefulWidget {
  const ChartWidget({super.key, required this.title});

  final String title;

  @override
  State<ChartWidget> createState() => _ChartWidgetState();
}

class _ChartWidgetState extends State<ChartWidget> {
  // Get shared preferences reference
  final Future<SharedPreferences> _prefs = SharedPreferences.getInstance();

  // Shared preferences variables references
  late Future<String> _userSessionToken;
  late Future<String> _userEmail;
  late Future<List<IoTDevice>> _userIoTDevices;
  late Future<String> _apiIPandPort;

  late Timer _uiRefreshTimer;

  // dropdown lists values are initialy not initialized
  String? _selectedIoTDeviceMAC = null;
  String? _selectedTopic = null;

  Future<List<Topic>> _getPossibleTopics() async {
    final SharedPreferences prefs = await _prefs;

    // Get session token for curr user
    final String currSessionToken = await _userSessionToken;
    final String apiIPandPort = await _apiIPandPort;

    // get desired device mac
    final String selectedDeviceMAC = _selectedIoTDeviceMAC ?? "";

    // get list of possible topics for selected device and current user
    List<Topic> possibleTopics = await getTopics(
        currSessionToken, selectedDeviceMAC,
        ipAndPort: apiIPandPort);

    // Display fetch outcome status in toast message
    if (possibleTopics.isNotEmpty) {
      // make sure the future function is not executing after widget was disposed of
      if (mounted) {
        // initialize the dropdown button the moment new Topics list is fetched
        // (state #1) the previus _selectedTopic was not initialized
        // (state #2) the previus _selectedTopic was already initialized
        // and the new list does not contain a topic name that was previously selected
        if (_selectedTopic == null ||
            possibleTopics.every((topic) => topic.name != _selectedTopic)) {
          try {
            _selectedTopic = possibleTopics.first.name;
          } catch (e) {
            print(
                "Tried to set default dropdown value, but Topics List was empty!");
          }
        }
      }
      // Display toast message - successful fetch
      displayToastSuccess("Device topics fetched");
    } else {
      // Display toast message - no possible topics for device with user
      displayToastNeutral("No avaliable topics for device");
    }
    return possibleTopics;
  }

  Future<bool> _removeLocalSessionData() async {
    // Log logout action
    print("Trying to remove local session data");
    final SharedPreferences prefs = await _prefs;

    bool localLogoutSuccess = true;

    if (mounted) {
      // Remove shared preferences variables
      List<String> localSessionData = [
        "userSessionToken",
        "userEmail",
        "addedIoTDevices",
        "userIoTDevices"
      ];
      localSessionData.forEach((sessionData) => {
            prefs.remove(sessionData).then((bool success) => {
                  print("Removed $sessionData: $success"),
                  if (success == false) {localLogoutSuccess = false}
                })
          });
    }

    return localLogoutSuccess;
  }

  Future<bool> _terminateUserSession() async {
    // Log logout action
    print("Trying to logout user");
    final SharedPreferences prefs = await _prefs;

    // get variables from shared preferences
    final String currSessionToken = await _userSessionToken;
    final String apiIPandPort = await _apiIPandPort;

    // preform local logout

    // Handle Logout Action
    final bool status = await logout(currSessionToken, ipAndPort: apiIPandPort);

    // if logout successful
    if (status) {
      // remember the api IP and Port
      prefs.setString('apiIPandPort', apiIPandPort);

      // Display toast message - successful logout
      displayToastSuccess("Successful logout");
      // Return status = true
      return status;
    } else {
      // logout failed for some reason
      // Display toast message - logout filed, check interned connection and credentials
      displayToastError(
          "Logout filed! Session dangling!\n Performed local logout.\n Check interned connection and credentials.");
      // Return status = false
      return status;
    }
  }

  Future<List<IoTDevice>> _getIoTDevices() async {
    final SharedPreferences prefs = await _prefs;
    // define API action

    // Get session token for curr user
    final String currSessionToken = await _userSessionToken;
    final String apiIPandPort = await _apiIPandPort;

    // Handle IoT Device fetch Action
    List<IoTDevice> newIoTDevicesList = await getRegisteredIoTDevices(
        currSessionToken,
        ipAndPort: apiIPandPort);

    String jsonEncodedIoTDevicesList = jsonEncode(newIoTDevicesList);
    // Set new value for local and shared preferences variable

    // make sure the future function is not executing after widget was disposed of
    if (mounted) {
      _userIoTDevices = prefs
          .setString('userIoTDevices', jsonEncodedIoTDevicesList)
          .then((bool success) {
        return newIoTDevicesList;
      });

      // try to initialize the dropdown button the moment IoT Devices data is fetched
      if (_selectedIoTDeviceMAC == null) {
        try {
          List<IoTDevice> iotDevices = await _userIoTDevices;
          if (iotDevices.isNotEmpty) {
            _selectedIoTDeviceMAC = iotDevices.first.mac;
          }
        } catch (e) {
          print(
              "Tried to set default dropdown value, but IoTDevices List was empty!");
        }
      }
    }

    // Display toast message - successful fetch
    displayToastSuccess("Fetched user IoT Devices");
    // Return json list of IoT Devices
    return newIoTDevicesList;
  }

  Future<List<TopicData>> _getTopicData() async {
    final SharedPreferences prefs = await _prefs;

    // Get session token for curr user
    final String currSessionToken = await _userSessionToken;
    final String apiIPandPort = await _apiIPandPort;
    final List<IoTDevice> userIoTDevices = await _userIoTDevices;

    // if user has registered devices
    if (userIoTDevices.length > 0) {
      // get desired device mac
      String selectedDeviceMAC = _selectedIoTDeviceMAC ?? "";
      // get desired topic name
      String selectedTopicName = _selectedTopic ?? "";

      if (userIoTDevices.length > 0) {
        // Handle Topic Data fetch Action
        List<TopicData> newTopicDataList = await getTopicData(
            currSessionToken, selectedDeviceMAC, selectedTopicName,
            ipAndPort: apiIPandPort);

        // Display toast message - successful fetch
        displayToastSuccess("Topic data fetched");
        return newTopicDataList;
      } else {
        // Display toast message - no data for provided device on this topic
        displayToastError("Device has no data for selected topic!");
        return [];
      }
    } else {
      // Display toast message - no user registered devices
      displayToastError("No registered user devices!");
      return [];
    }
  }

  Future<bool> _removeRegisteredDevice(IoTDevice registeredDevice) async {
    displayToastNeutral(
        "Not implemented functionality. Should unregister device from user account.");
    // TODO: remove specific registered iot device from user account
    // call app server api to remove the device
    // get new updated devices list (use setState to update _userIoTDevices)

    // return operation success state as failed
    return false;
  }

  @override
  void initState() {
    super.initState();
    // set up widget refresh rate
    _uiRefreshTimer =
        Timer.periodic(const Duration(milliseconds: 5000), (Timer t) {
      if (mounted) {
        setState(() {});
      }
    });
    // overwrite initial state with values stored in shared preferences
    _userSessionToken = _prefs.then((SharedPreferences prefs) {
      return prefs.getString('userSessionToken') ?? "";
    });
    _userEmail = _prefs.then((SharedPreferences prefs) {
      return prefs.getString('userEmail') ?? "";
    });
    _apiIPandPort = _prefs.then((SharedPreferences prefs) {
      return prefs.getString('apiIPandPort') ?? "";
    });
    _userIoTDevices = _prefs.then((SharedPreferences prefs) {
      Iterable l =
          json.decode(prefs.getString('userIoTDevices') ?? jsonEncode([]));
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
        child: Column(
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
                              padding: EdgeInsets.all(10),
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
                              padding: EdgeInsets.all(10),
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
                              padding: EdgeInsets.all(10),
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
                              padding: EdgeInsets.all(10),
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
            // build drawer body
            // Build a ListView with ListTile items for every IoTDevice fetched
            Expanded(
              child: FutureBuilder<List<IoTDevice>>(
                  future: _getIoTDevices(),
                  builder: (context, snapshot) {
                    if (!snapshot.hasData) {
                      // returns null -> interprete as waiting for data
                      return const Center(child: CircularProgressIndicator());
                    }
                    List<IoTDevice> iotDevices =
                        snapshot.data as List<IoTDevice>;
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
                          trailing: IconButton(
                            icon: const Icon(
                              Icons.remove_circle,
                              color: Colors.red,
                              size: 35,
                            ),
                            onPressed: () => _removeRegisteredDevice(iotDevice),
                          ),
                          title: Text(iotDevice.model),
                          subtitle: Text(iotDevice.mac),
                          onTap: () {
                            // Update the state of the app.
                            // ...
                          },
                        );
                      },
                    );
                  }),
            ),

            Row(
              children: <Widget>[
                Expanded(
                    child: ElevatedButton(
                        style: ElevatedButton.styleFrom(
                          padding: const EdgeInsets.fromLTRB(5, 5, 5, 5),
                          shape: const RoundedRectangleBorder(
                            borderRadius: BorderRadius.only(
                              bottomLeft: Radius.circular(0.0),
                              bottomRight: Radius.circular(0.0),
                              topRight: Radius.circular(0.0),
                              topLeft: Radius.circular(0.0),
                            ),
                          ),
                        ),
                        child: const Icon(Icons.add, size: 40),
                        // Move to chart widget
                        onPressed: () => GoRouter.of(context)
                            .go('/home/registerIoTDevice'))),
              ],
            )
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
                  'Home / Chart page',
                  style: TextStyle(fontSize: 20),
                )),
            Container(
                alignment: Alignment.center,
                padding: const EdgeInsets.all(10),
                width: double.infinity,
                height: 300,
                child: FutureBuilder<List<TopicData>>(
                    future: _getTopicData(),
                    builder: (context, snapshot) {
                      if (!snapshot.hasData && snapshot.data == null) {
                        // returns null -> interprete as waiting for data
                        return const Center(child: CircularProgressIndicator());
                      } else {
                        List<TopicData> topicDataList =
                            snapshot.data as List<TopicData>;
                        List<FlSpot> chartData = [];
                        for (int idx = 0; idx < topicDataList.length; idx++) {
                          TopicData topicData = topicDataList[idx];
                          // create new chart data point
                          // if data does not contain valid float use fallback value
                          chartData.add(FlSpot(
                              idx.toDouble(),
                              double.parse(topicData.data, (String? error) {
                                return 0.0; // Fallback value for data not parsable to float
                              })));
                        }
                        return LineChart(
                          LineChartData(
                              borderData: FlBorderData(show: false),
                              lineBarsData: [
                                LineChartBarData(spots: chartData)
                              ]),
                        );
                      }
                    })),
            Container(
                alignment: Alignment.center,
                padding: const EdgeInsets.all(10),
                child: FutureBuilder<List<IoTDevice>>(
                    future: _userIoTDevices,
                    builder: (context, snapshot) {
                      if (!snapshot.hasData && snapshot.data == null) {
                        // returns null -> interprete as waiting for data
                        return const Center(child: CircularProgressIndicator());
                      } else {
                        List<IoTDevice> iotDevicesList =
                            snapshot.data as List<IoTDevice>;
                        return DropdownButton<String>(
                          items: iotDevicesList.map<DropdownMenuItem<String>>(
                              (IoTDevice ioTDevice) {
                            return DropdownMenuItem<String>(
                              value: ioTDevice.mac,
                              child:
                                  Text("${ioTDevice.model} ${ioTDevice.mac}"),
                            );
                          }).toList(),
                          value: _selectedIoTDeviceMAC, // initialy null
                          icon: const Icon(Icons.arrow_downward),
                          elevation: 16,
                          style: const TextStyle(color: Colors.blue),
                          underline: Container(
                            height: 2,
                            color: Colors.blue,
                          ),
                          onChanged: (String? selectedDeviceMAC) {
                            // This is called when the user selects an item.
                            setState(() {
                              _selectedIoTDeviceMAC = selectedDeviceMAC;

                              // if newly selected device exists in devices list
                              if (iotDevicesList.any((iotDevice) =>
                                  iotDevice.mac == _selectedIoTDeviceMAC)) {
                                // get newly selected device
                                IoTDevice selectedDevice =
                                    iotDevicesList.firstWhere((iotDevice) =>
                                        iotDevice.mac == _selectedIoTDeviceMAC);
                                // if newly selected device does not subscribe currently selected topic
                                if (!selectedDevice.topics.any(
                                    (topic) => topic.name == _selectedTopic)) {
                                  // change topic to first possible for new device
                                  if (selectedDevice.topics.isNotEmpty) {
                                    _selectedTopic =
                                        selectedDevice.topics.first.name;
                                  } else {
                                    // if newly selected device does not subscribe any topics
                                    // set selected topic to null
                                    _selectedTopic = null;
                                  }
                                }
                              }
                            });
                            // refetch Topic data with new parameters
                            _getTopicData();
                          },
                        );
                      }
                    })),
            Container(
                alignment: Alignment.center,
                padding: const EdgeInsets.all(10),
                child: FutureBuilder<List<IoTDevice>>(
                    future: _userIoTDevices,
                    builder: (context, snapshot) {
                      if (!snapshot.hasData ||
                          snapshot.data == null ||
                          _selectedIoTDeviceMAC == null) {
                        // returns null -> interprete as waiting for data
                        return const Center(child: CircularProgressIndicator());
                      } else {
                        List<Topic> topicsList = [];
                        List<IoTDevice> iotDevicesList =
                            snapshot.data as List<IoTDevice>;
                        if (iotDevicesList.any((iotDevice) =>
                            iotDevice.mac == _selectedIoTDeviceMAC)) {
                          topicsList = iotDevicesList
                              .firstWhere((iotDevice) =>
                                  iotDevice.mac == _selectedIoTDeviceMAC)
                              .topics;
                        }
                        return DropdownButton<String>(
                          items: topicsList
                              .map<DropdownMenuItem<String>>((Topic topic) {
                            return DropdownMenuItem<String>(
                              value: topic.name,
                              child: Text(topic.name),
                            );
                          }).toList(),
                          value: _selectedTopic, // initialy null
                          icon: const Icon(Icons.arrow_downward),
                          elevation: 16,
                          style: const TextStyle(color: Colors.blue),
                          underline: Container(
                            height: 2,
                            color: Colors.blue,
                          ),
                          onChanged: (String? selectedTopicName) {
                            // This is called when the user selects an item.
                            setState(() {
                              _selectedTopic = selectedTopicName;
                            });
                            // refetch Topic data with new parameters
                            // _getTopicData();
                            _getIoTDevices();
                          },
                        );
                      }
                    })),
          ],
        ),
      ),
      floatingActionButton: FloatingActionButton(
        onPressed: () {
          // Handle user logout action
          _terminateUserSession(); // destroy servers session
          // remove local session data
          _removeLocalSessionData().then((bool localLogoutStatus) => {
                if (localLogoutStatus)
                  {displayToastSuccess("Performed local logout successfuly")},
                // Move to login page
                GoRouter.of(context).go('/login'),
              });
        },
        tooltip: 'Logout',
        child: const Icon(Icons.logout),
      ), // This trailing comma makes auto-formatting nicer for build methods.
    );
  }
}
