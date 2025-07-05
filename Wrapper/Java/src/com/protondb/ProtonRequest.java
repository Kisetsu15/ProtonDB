package com.protondb;

public class ProtonRequest {
    private String Command;
    private String Data;

    public ProtonRequest() {}

    public ProtonRequest(String command, String data) {
        this.Command = command;
        this.Data = data;
    }

    public String getCommand() {
        return Command;
    }

    public void setCommand(String command) {
        this.Command = command;
    }

    public String getData() {
        return Data;
    }

    public void setData(String data) {
        this.Data = data;
    }

    public String toJson() {
        StringBuilder json = new StringBuilder();
        json.append("{");
        json.append("\"Command\":\"").append(escapeJson(Command)).append("\"");
        if (Data != null) {
            json.append(",\"Data\":\"").append(escapeJson(Data)).append("\"");
        }
        json.append("}");
        return json.toString();
    }

    private String escapeJson(String str) {
        if (str == null) return "";
        return str.replace("\\", "\\\\")
                 .replace("\"", "\\\"")
                 .replace("\n", "\\n")
                 .replace("\r", "\\r")
                 .replace("\t", "\\t");
    }
}
