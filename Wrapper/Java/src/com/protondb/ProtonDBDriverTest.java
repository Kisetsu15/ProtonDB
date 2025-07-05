package com.protondb;

import java.util.HashMap;
import java.util.Map;

/**
 * Comprehensive test for ProtonDBDriver
 * Demonstrates all database operations including database management,
 * collection operations, and document CRUD operations.
 */
public class ProtonDBDriverTest {
    public static void main(String[] args) {
        ProtonDBDriver driver = null;
        
        try {
            // Connect to ProtonDB
            System.out.println("=== Connecting to ProtonDB ===");
            driver = new ProtonDBDriver("admin123", "welcome");
            System.out.println("[SUCCESS] Connected to ProtonDB");
            
            // Test Database Operations
            testDatabaseOperations(driver);
            
            // Test Collection Operations
            testCollectionOperations(driver);
            
            // Test Document Operations
            testDocumentOperations(driver);
            
            // Test Profile Operations
            testProfileOperations(driver);
            
            // Test Utility Operations
            testUtilityOperations(driver);
            
            System.out.println("\n=== All Tests Completed Successfully! ===");
            
        } catch (Exception e) {
            System.err.println("[ERROR] Test failed: " + e.getMessage());
            e.printStackTrace();
        } finally {
            if (driver != null) {
                try {
                    driver.close();
                    System.out.println("[INFO] Connection closed");
                } catch (Exception e) {
                    System.err.println("[ERROR] Failed to close connection: " + e.getMessage());
                }
            }
        }
    }
    
    private static void testDatabaseOperations(ProtonDBDriver driver) throws Exception {
        System.out.println("\n=== Testing Database Operations ===");
        
        // Create database
        ProtonResponse response = driver.createDatabase("TestDB");
        System.out.println("Create Database: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        System.out.println("  Message: " + response.getMessage());
        
        // Use database
        response = driver.useDatabase("TestDB");
        System.out.println("Use Database: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        System.out.println("  Current DB: " + driver.getCurrentDatabase());
        
        // List databases
        response = driver.listDatabases();
        System.out.println("List Databases: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
    }
    
    private static void testCollectionOperations(ProtonDBDriver driver) throws Exception {
        System.out.println("\n=== Testing Collection Operations ===");
        
        // Create collection
        ProtonResponse response = driver.createCollection("Users");
        System.out.println("Create Collection: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        System.out.println("  Message: " + response.getMessage());
        
        // Create another collection
        response = driver.createCollection("Products");
        System.out.println("Create Products Collection: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // List collections
        response = driver.listCollections();
        System.out.println("List Collections: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
    }
    
    private static void testDocumentOperations(ProtonDBDriver driver) throws Exception {
        System.out.println("\n=== Testing Document Operations ===");
        
        // Insert document using JSON string
        ProtonResponse response = driver.insertDocument("Users", 
            "{\"name\": \"John Doe\", \"age\": 30, \"email\": \"john@example.com\"}");
        System.out.println("Insert Document (JSON): " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        System.out.println("  Message: " + response.getMessage());
        
        // Insert document using Map
        Map<String, Object> userDoc = new HashMap<>();
        userDoc.put("name", "Jane Smith");
        userDoc.put("age", 25);
        userDoc.put("email", "jane@example.com");
        userDoc.put("active", true);
        
        response = driver.insertDocument("Users", userDoc);
        System.out.println("Insert Document (Map): " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Insert product documents
        response = driver.insertDocument("Products", 
            "{\"name\": \"Laptop\", \"price\": 999.99, \"category\": \"Electronics\"}");
        System.out.println("Insert Product: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        response = driver.insertDocument("Products", 
            "{\"name\": \"Book\", \"price\": 29.99, \"category\": \"Education\"}");
        System.out.println("Insert Book: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Find all documents
        response = driver.findAllDocuments("Users");
        System.out.println("Find All Users: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Find documents with query
        response = driver.findDocuments("Products", "{\"category\": \"Electronics\"}");
        System.out.println("Find Electronics: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Count documents
        response = driver.countDocuments("Users", "{}");
        System.out.println("Count Users: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Update documents
        response = driver.updateDocuments("Users", 
            "{\"name\": \"John Doe\"}", 
            "{\"age\": 31, \"updated\": true}");
        System.out.println("Update User: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Print collection (for debugging)
        response = driver.printCollection("Users");
        System.out.println("Print Users Collection: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
    }
    
    private static void testProfileOperations(ProtonDBDriver driver) throws Exception {
        System.out.println("\n=== Testing Profile Operations ===");
        
        // Create user profile
        ProtonResponse response = driver.createProfile("testuser", "testpass", "user");
        System.out.println("Create Profile: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        System.out.println("  Message: " + response.getMessage());
        
        // List profiles
        response = driver.listProfiles();
        System.out.println("List Profiles: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Delete test profile
        response = driver.deleteProfile("testuser");
        System.out.println("Delete Profile: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
    }
    
    private static void testUtilityOperations(ProtonDBDriver driver) throws Exception {
        System.out.println("\n=== Testing Utility Operations ===");
        
        // Enable debug mode
        ProtonResponse response = driver.setDebugMode(true);
        System.out.println("Enable Debug: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Get server profile
        response = driver.getServerProfile();
        System.out.println("Get Server Profile: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Fetch results
        response = driver.fetchResults();
        System.out.println("Fetch Results: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Execute raw query
        response = driver.executeQuery("database.list()");
        System.out.println("Raw Query: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
        
        // Disable debug mode
        response = driver.setDebugMode(false);
        System.out.println("Disable Debug: " + (response.isSuccess() ? "SUCCESS" : "FAILED"));
    }
}
