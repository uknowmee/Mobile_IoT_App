import 'package:flutter/material.dart';
import 'package:fluttertoast/fluttertoast.dart';

void displayToastNeutral(String message,
    {Toast toastLength = Toast.LENGTH_SHORT}) {
  displayToastGeneric(message, Colors.white,
      backgroundColor: Colors.grey.shade500, toastLength: toastLength);
}

void displayToastSuccess(String message,
    {Toast toastLength = Toast.LENGTH_SHORT}) {
  displayToastGeneric(message, Colors.green, toastLength: toastLength);
}

void displayToastError(String message,
    {Toast toastLength = Toast.LENGTH_LONG}) {
  displayToastGeneric(message, Colors.red, toastLength: toastLength);
}

void displayToastGeneric(String message, Color textColor,
    {Color? backgroundColor = const Color(0xFFEEEEEE),
    Toast toastLength = Toast.LENGTH_SHORT,
    double? fontSize = 15}) {
  Fluttertoast.showToast(
    msg: message,
    toastLength: toastLength,
    fontSize: fontSize,
    backgroundColor: backgroundColor,
    textColor: textColor,
  );
}
