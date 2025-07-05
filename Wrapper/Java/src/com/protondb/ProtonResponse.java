package com.protondb;

import java.util.regex.Matcher;
import java.util.regex.Pattern;

public class ProtonResponse {
    private String Status;
    private String Message;
    private String[] Result;

    public ProtonResponse() {}

    public String getStatus() {
        return Status;
    }

    public void setStatus(String status) {
        this.Status = status;
    }

    public String getMessage() {
        return Message;
    }

    public void setMessage(String message) {
        this.Message = message;
    }

    public String[] getResult() {
        return Result;
    }

    public void setResult(String[] result) {
        this.Result = result;
    }

    public boolean isSuccess() {
        return "ok".equalsIgnoreCase(Status);
    }

    public static ProtonResponse fromJson(String json) {
        ProtonResponse response = new ProtonResponse();
        
        if (json == null) return response;
        
        try {
            // Simple JSON parsing using regex (not recommended for production)
            Pattern statusPattern = Pattern.compile("\"Status\"\\s*:\\s*\"([^\"]+)\"");
            Pattern messagePattern = Pattern.compile("\"Message\"\\s*:\\s*\"([^\"]+)\"");
            
            Matcher statusMatcher = statusPattern.matcher(json);
            if (statusMatcher.find()) {
                response.setStatus(statusMatcher.group(1));
            }
            
            Matcher messageMatcher = messagePattern.matcher(json);
            if (messageMatcher.find()) {
                response.setMessage(messageMatcher.group(1));
            }
            
        } catch (Exception e) {
            response.setStatus("error");
            response.setMessage("Failed to parse response: " + e.getMessage());
        }
        
        return response;
    }

    @Override
    public String toString() {
        return String.format("ProtonResponse{Status='%s', Message='%s'}", Status, Message);
    }
}
