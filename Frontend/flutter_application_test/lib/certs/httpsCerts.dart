import 'dart:io';

class CertifiedHttpOverrides extends HttpOverrides {
  // skip SSL handshake for development, self-signed certificates
  bool CustomBadCerificateCallback(
      X509Certificate cert, String host, int port) {
    return true;
  }

  // override bad certificete callback in returned HttpClient
  @override
  HttpClient createHttpClient(SecurityContext? context) {
    return super.createHttpClient(context)
      ..badCertificateCallback = CustomBadCerificateCallback;
  }
}
