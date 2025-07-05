package com.protondb;

/**
 * Simple test to verify ProtonDBDriver functionality
 */
public class DriverQuickTest {
    public static void main(String[] args) {
        try {
            System.out.println("=== ProtonDBDriver Quick Test ===");
            
            // Test connection
            ProtonDBDriver driver = new ProtonDBDriver("admin123", "welcome");
            System.out.println("[SUCCESS] Connected to ProtonDB");
            
            // Test database operations
            ProtonResponse response = driver.createDatabase("QuickTestDB");
            System.out.println("Create DB: " + response.getStatus() + " - " + response.getMessage());
            
            response = driver.useDatabase("QuickTestDB");
            System.out.println("Use DB: " + response.getStatus() + " - " + response.getMessage());
            System.out.println("Current DB: " + driver.getCurrentDatabase());
            
            // Test collection operations
            response = driver.createCollection("TestCollection");
            System.out.println("Create Collection: " + response.getStatus() + " - " + response.getMessage());
            
            // Test document operations
            response = driver.insertDocument("TestCollection", "{\"name\": \"Test Item\", \"value\": 123}");
            System.out.println("Insert Document: " + response.getStatus() + " - " + response.getMessage());
            
            response = driver.findAllDocuments("TestCollection");
            System.out.println("Find Documents: " + response.getStatus() + " - " + response.getMessage());
            
            // Test utility
            response = driver.setDebugMode(true);
            System.out.println("Debug Mode: " + response.getStatus() + " - " + response.getMessage());
            
            driver.close();
            System.out.println("[SUCCESS] All tests completed and connection closed");
            
        } catch (Exception e) {
            System.err.println("[ERROR] Test failed: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
