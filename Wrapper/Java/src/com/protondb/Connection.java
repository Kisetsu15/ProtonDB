package com.protondb;

import java.io.*;
import java.net.Socket;
import java.nio.charset.StandardCharsets;

public class Connection {
    private final Socket socket;
    private final BufferedReader reader;
    private final BufferedWriter writer;

    public Connection(String host, int port, String username, String password) throws IOException {
        socket = new Socket(host, port);
        reader = new BufferedReader(new InputStreamReader(socket.getInputStream(), StandardCharsets.UTF_8));
        writer = new BufferedWriter(new OutputStreamWriter(socket.getOutputStream(), StandardCharsets.UTF_8));

        // First, receive the welcome message
        String welcomeMessage = receiveInternal();
        System.out.println("Welcome: " + welcomeMessage);

        // Then authenticate using proper JSON protocol
        String loginData = username + "," + password;
        String loginRequest = String.format("{\"Command\":\"LOGIN\",\"Data\":\"%s\"}", loginData);
        sendInternal(loginRequest);
        
        String authResponse = receiveInternal();
        System.out.println("Auth Response: " + authResponse);

        if (!isSuccessResponse(authResponse)) {
            throw new IOException("Authentication failed: " + authResponse);
        }
    }

    private boolean isSuccessResponse(String response) {
        if (response == null) return false;
        // Simple check for "ok" status in JSON response
        return response.contains("\"Status\":\"ok\"") || response.contains("\"status\":\"ok\"");
    }

    private void sendInternal(String jsonLine) throws IOException {
        writer.write(jsonLine);
        writer.newLine();
        writer.flush();
    }

    private String receiveInternal() throws IOException {
        String line = reader.readLine();
        System.out.println("💬 Raw Received: " + line);
        return line;
    }

    public void send(String jsonLine) throws IOException {
        writer.write(jsonLine);
        writer.newLine(); // required for TCP JSON line-based protocol
        writer.flush();
    }

    public ProtonResponse sendRequest(String command, String data) throws IOException {
        ProtonRequest request = new ProtonRequest(command, data);
        send(request.toJson());
        String responseJson = receive();
        return ProtonResponse.fromJson(responseJson);
    }

    public String receive() throws IOException {
        String line = reader.readLine();
        System.out.println("💬 Raw Received: " + line);
        return line;
    }

    public void close() throws IOException {
        if (reader != null) reader.close();
        if (writer != null) writer.close();
        if (socket != null) socket.close();
    }
}
