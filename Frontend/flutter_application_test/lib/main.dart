import 'dart:io';
import 'package:flutter/material.dart';
import 'package:go_router/go_router.dart';
import './login.dart';
import './register.dart';
import './chart.dart';
import './iotDeviceRegistration.dart';
import './certs/httpsCerts.dart';

void main() {
  HttpOverrides.global = CertifiedHttpOverrides();

  runApp(const MyApp());
}

final GoRouter _router = GoRouter(
  initialLocation: '/login',
  routes: [
    GoRoute(
      path: '/',
      builder: (context, state) =>
          const LoginWidget(title: 'Logged-out Home / Login Page'),
    ),
    GoRoute(
      path: '/login',
      builder: (context, state) =>
          const LoginWidget(title: 'Logged-out Home / Login Page'),
    ),
    GoRoute(
      path: '/register',
      builder: (context, state) => const RegisterWidget(title: 'Signup Page'),
    ),
    GoRoute(
      path: '/home',
      builder: (context, state) => const ChartWidget(title: 'Chart Page'),
    ),
    GoRoute(
      path: '/home/registerIoTDevice',
      builder: (context, state) => const IoTDeviceRegistrationWidget(
          title: 'IoT Device Registration Page'),
    )
  ],
);

class MyApp extends StatelessWidget {
  const MyApp({super.key});

  // This widget is the root of your application.
  @override
  Widget build(BuildContext context) {
    return MaterialApp.router(
        title: 'IoT Demo Application',
        theme: ThemeData(
          // This is the theme of your application.
          //
          // Try running your application with "flutter run". You'll see the
          // application has a blue toolbar. Then, without quitting the app, try
          // changing the primarySwatch below to Colors.green and then invoke
          // "hot reload" (press "r" in the console where you ran "flutter run",
          // or simply save your changes to "hot reload" in a Flutter IDE).
          // Notice that the counter didn't reset back to zero; the application
          // is not restarted.
          primarySwatch: Colors.blue,
        ),
        routerConfig: _router);
  }
}
