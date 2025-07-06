# ProtonDB Java Driver

A comprehensive Java database driver for ProtonDB with high-level API support.

## 🚀 Features

- **High-level Database Driver**: `ProtonDBDriver` with intuitive API
- **Low-level Client**: `ProtonDBClient` for direct TCP communication
- **Database Management**: Create, use, drop, and list databases
- **Collection Operations**: Full collection lifecycle management
- **Document CRUD**: Insert, find, update, delete, and count operations
- **Profile Management**: User creation and management
- **Type Safety**: Structured request/response handling
- **Flexible Input**: Support for JSON strings and Java Maps
- **Error Handling**: Comprehensive error reporting and validation
- **No Dependencies**: Uses only standard Java libraries

## 📋 Module Components

### Core Classes
- `ProtonDBDriver.java` - High-level database driver API
- `ProtonDBClient.java` - Mid-level client interface  
- `Connection.java` - Low-level TCP connection handler
- `ProtonRequest.java` - Request structure and JSON formatting
- `ProtonResponse.java` - Response parsing and validation

### Test Classes
- `TestClient.java` - Basic connectivity test
- `DriverQuickTest.java` - Driver functionality verification
- `ProtonDBDriverTest.java` - Comprehensive test suite

## 🛠️ Prerequisites

- **Java**: Version 8 or higher
- **ProtonDB Server**: Running on port 9090 (default)
- **Network**: Access to ProtonDB server
- **Credentials**: Valid username/password (default: admin123/welcome)

## 🏗️ Building & Running

### Option 1: Use Build Script (Windows)
```bash
# Navigate to Java wrapper directory
cd Wrapper/Java

# Run build script with test selection
.\build_and_run.bat
```

### Option 2: Manual Compilation
```bash
# Navigate to source directory
cd Wrapper/Java/src

# Compile all Java files
javac -cp . com/protondb/*.java

# Run quick test
java -cp . com.protondb.DriverQuickTest
```

### Option 3: Maven Build
```bash
cd Wrapper/Java
mvn clean compile
mvn exec:java -Dexec.mainClass="com.protondb.DriverQuickTest"
```

## 🚀 Quick Start - Testing ProtonDBDriver

### Step 1: Start ProtonDB Server
Ensure the ProtonDB server is running before testing:
```bash
# Navigate to server directory
cd ProtonDB.Server/bin/Debug/net9.0

# Start the server
.\ProtonDB.Server.exe
```

### Step 2: Test ProtonDBDriver
Choose one of the following test methods:

#### Interactive Test Selection (Recommended)
```bash
# From Java wrapper root
cd Wrapper/Java
.\build_and_run.bat
# Select option 2 for ProtonDBDriver Quick Test
# Select option 3 for Comprehensive Driver Test
```

#### Direct ProtonDBDriver Testing
```bash
# Compile and run ProtonDBDriver quick test
cd Wrapper/Java/src
javac -cp . com/protondb/*.java
java -cp . com.protondb.DriverQuickTest
```

#### Comprehensive ProtonDBDriver Test
```bash
# Run full test suite
cd Wrapper/Java/src
javac -cp . com/protondb/*.java
java -cp . com.protondb.ProtonDBDriverTest
```

### Expected Output
```
=== ProtonDBDriver Quick Test ===
Welcome: Connected to ProtonDB. Send a query or use FETCH.
Auth Response: {"Status":"ok","Message":"Login successful","Result":null}
[SUCCESS] Connected to ProtonDB
Create DB: ok - Query accepted. Use FETCH to retrieve result.
Use DB: ok - Query accepted. Use FETCH to retrieve result.
Current DB: QuickTestDB
Create Collection: ok - Query accepted. Use FETCH to retrieve result.
Insert Document: ok - Query accepted. Use FETCH to retrieve result.
Find Documents: ok - Query accepted. Use FETCH to retrieve result.
Debug Mode: ok - Debug logs enabled
[SUCCESS] All tests completed and connection closed
```

## 💻 Usage Examples

### High-Level Driver API (Recommended)

```java
import com.protondb.ProtonDBDriver;
import com.protondb.ProtonResponse;
import java.util.HashMap;
import java.util.Map;

public class Example {
    public static void main(String[] args) {
        try {
            // Connect to ProtonDB
            ProtonDBDriver driver = new ProtonDBDriver("admin123", "welcome");
            
            // Database operations
            driver.createDatabase("MyApp");
            driver.useDatabase("MyApp");
            System.out.println("Current DB: " + driver.getCurrentDatabase());
            
            // Collection operations
            driver.createCollection("Users");
            driver.createCollection("Products");
            
            // Insert documents using Map
            Map<String, Object> user = new HashMap<>();
            user.put("name", "John Doe");
            user.put("age", 30);
            user.put("email", "john@example.com");
            user.put("active", true);
            
            ProtonResponse response = driver.insertDocument("Users", user);
            if (response.isSuccess()) {
                System.out.println("User inserted: " + response.getMessage());
            }
            
            // Insert using JSON string
            driver.insertDocument("Products", 
                "{\"name\": \"Laptop\", \"price\": 999.99, \"category\": \"Electronics\"}");
            
            // Query documents
            ProtonResponse users = driver.findAllDocuments("Users");
            System.out.println("Found users: " + users.getMessage());
            
            ProtonResponse electronics = driver.findDocuments("Products", 
                "{\"category\": \"Electronics\"}");
            System.out.println("Electronics: " + electronics.getMessage());
            
            // Update documents
            driver.updateDocuments("Users", 
                "{\"name\": \"John Doe\"}", 
                "{\"age\": 31, \"updated\": true}");
            
            // Count documents
            ProtonResponse count = driver.countDocuments("Users", "{}");
            System.out.println("Total users: " + count.getMessage());
            
            // Profile management
            driver.createProfile("newuser", "password123", "user");
            ProtonResponse profiles = driver.listProfiles();
            System.out.println("Profiles: " + profiles.getMessage());
            
            // Clean up
            driver.close();
            
        } catch (Exception e) {
            System.err.println("Error: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
```

### Low-Level Client API

```java
import com.protondb.ProtonDBClient;
import com.protondb.ProtonResponse;

public class LowLevelExample {
    public static void main(String[] args) {
        try {
            // Direct client connection
            ProtonDBClient client = new ProtonDBClient("localhost", 9090, "admin123", "welcome");
            
            // Execute raw queries
            ProtonResponse response = client.query("database.create(\"TestDB\")");
            System.out.println("Create DB: " + response.getStatus() + " - " + response.getMessage());
            
            response = client.query("Users.insert({\"name\": \"Test User\"})");
            System.out.println("Insert: " + response.getStatus() + " - " + response.getMessage());
            
            // Utility operations
            response = client.debug(true);
            System.out.println("Debug mode: " + response.getStatus());
            
            response = client.fetch();
            System.out.println("Fetch results: " + response.getMessage());
            
            client.close();
            
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
```
if (response.isSuccess()) {
    System.out.println("Query successful: " + response.getMessage());
} else {
    System.out.println("Query failed: " + response.getMessage());
}

// Close connection
client.close();
```

```

## 🔧 Creating Your Own ProtonDBDriver Application

### Step 1: Copy Required Files
Copy these files to your project:
```
src/com/protondb/
├── ProtonDBDriver.java    (Main high-level API)
├── ProtonDBClient.java    (Mid-level client)
├── Connection.java        (TCP connection handler)
├── ProtonRequest.java     (Request structure)
└── ProtonResponse.java    (Response parser)
```

### Step 2: Compile Your Application
```bash
# Place your application class alongside ProtonDB classes
# Example: MyApp.java in src/com/yourpackage/

# Compile everything together
javac -cp . com/protondb/*.java com/yourpackage/*.java

# Run your application
java -cp . com.yourpackage.MyApp
```

### Step 3: Sample Custom Application
```java
package com.yourpackage;

import com.protondb.ProtonDBDriver;
import com.protondb.ProtonResponse;
import java.util.HashMap;
import java.util.Map;
import java.util.Scanner;

public class UserManagementApp {
    private ProtonDBDriver driver;
    private Scanner scanner;
    
    public UserManagementApp() throws Exception {
        driver = new ProtonDBDriver("admin123", "welcome");
        scanner = new Scanner(System.in);
        
        // Setup database
        driver.createDatabase("UserManagement");
        driver.useDatabase("UserManagement");
        driver.createCollection("Users");
    }
    
    public void run() {
        System.out.println("=== User Management System ===");
        
        while (true) {
            System.out.println("\n1. Add User");
            System.out.println("2. Find User");
            System.out.println("3. List All Users");
            System.out.println("4. Delete User");
            System.out.println("5. Exit");
            System.out.print("Choose option: ");
            
            int choice = scanner.nextInt();
            scanner.nextLine(); // consume newline
            
            switch (choice) {
                case 1: addUser(); break;
                case 2: findUser(); break;
                case 3: listUsers(); break;
                case 4: deleteUser(); break;
                case 5: exit(); return;
                default: System.out.println("Invalid option!");
            }
        }
    }
    
    private void addUser() {
        try {
            System.out.print("Enter name: ");
            String name = scanner.nextLine();
            System.out.print("Enter email: ");
            String email = scanner.nextLine();
            System.out.print("Enter age: ");
            int age = scanner.nextInt();
            scanner.nextLine();
            
            Map<String, Object> user = new HashMap<>();
            user.put("name", name);
            user.put("email", email);
            user.put("age", age);
            user.put("created", System.currentTimeMillis());
            
            ProtonResponse response = driver.insertDocument("Users", user);
            if (response.isSuccess()) {
                System.out.println("✅ User added successfully!");
            } else {
                System.out.println("❌ Failed to add user: " + response.getMessage());
            }
        } catch (Exception e) {
            System.out.println("❌ Error: " + e.getMessage());
        }
    }
    
    private void findUser() {
        try {
            System.out.print("Enter name to search: ");
            String name = scanner.nextLine();
            
            String query = String.format("{\"name\": \"%s\"}", name);
            ProtonResponse response = driver.findDocuments("Users", query);
            
            if (response.isSuccess()) {
                System.out.println("✅ Search results: " + response.getMessage());
            } else {
                System.out.println("❌ Search failed: " + response.getMessage());
            }
        } catch (Exception e) {
            System.out.println("❌ Error: " + e.getMessage());
        }
    }
    
    private void listUsers() {
        try {
            ProtonResponse response = driver.findAllDocuments("Users");
            if (response.isSuccess()) {
                System.out.println("📋 All users: " + response.getMessage());
                
                ProtonResponse count = driver.countDocuments("Users", "{}");
                System.out.println("👥 Total users: " + count.getMessage());
            } else {
                System.out.println("❌ Failed to list users: " + response.getMessage());
            }
        } catch (Exception e) {
            System.out.println("❌ Error: " + e.getMessage());
        }
    }
    
    private void deleteUser() {
        try {
            System.out.print("Enter name to delete: ");
            String name = scanner.nextLine();
            
            String query = String.format("{\"name\": \"%s\"}", name);
            ProtonResponse response = driver.deleteDocuments("Users", query);
            
            if (response.isSuccess()) {
                System.out.println("✅ User deleted successfully!");
            } else {
                System.out.println("❌ Failed to delete user: " + response.getMessage());
            }
        } catch (Exception e) {
            System.out.println("❌ Error: " + e.getMessage());
        }
    }
    
    private void exit() {
        try {
            driver.close();
            scanner.close();
            System.out.println("👋 Goodbye!");
        } catch (Exception e) {
            System.out.println("❌ Error closing connection: " + e.getMessage());
        }
    }
    
    public static void main(String[] args) {
        try {
            UserManagementApp app = new UserManagementApp();
            app.run();
        } catch (Exception e) {
            System.out.println("❌ Failed to start application: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
```

### Step 4: Advanced Features
```java
// Use transactions (if supported by server)
try {
    driver.executeQuery("transaction.begin()");
    driver.insertDocument("Users", user1);
    driver.insertDocument("Logs", logEntry);
    driver.executeQuery("transaction.commit()");
} catch (Exception e) {
    driver.executeQuery("transaction.rollback()");
}

// Batch operations
for (Map<String, Object> user : userList) {
    driver.insertDocument("Users", user);
}

// Complex queries with multiple criteria
String complexQuery = "{\"age\": {\"$gt\": 18}, \"status\": \"active\"}";
ProtonResponse adults = driver.findDocuments("Users", complexQuery);
```

## 📚 API Reference

### ProtonDBDriver Methods

#### Database Operations
- `createDatabase(String name)` → Create new database
- `useDatabase(String name)` → Select database for operations
- `dropDatabase(String name)` → Delete database (null for current)
- `listDatabases()` → List all available databases
- `getCurrentDatabase()` → Get currently selected database name

#### Collection Operations
- `createCollection(String name)` → Create new collection
- `dropCollection(String name)` → Delete collection
- `listCollections()` → List collections in current database

#### Document Operations
- `insertDocument(String collection, String json)` → Insert from JSON string
- `insertDocument(String collection, Map<String, Object> doc)` → Insert from Map
- `findDocuments(String collection, String query)` → Find with criteria
- `findAllDocuments(String collection)` → Find all documents
- `updateDocuments(String collection, String query, String update)` → Update matching docs
- `deleteDocuments(String collection, String query)` → Delete matching docs
- `countDocuments(String collection, String query)` → Count matching docs
- `printCollection(String collection)` → Display collection (debug)

#### Profile Operations
- `createProfile(String username, String password, String role)` → Create user
- `deleteProfile(String username)` → Delete user
- `listProfiles()` → List all users

#### Utility Operations
- `setDebugMode(boolean enable)` → Enable/disable debug logging
- `getServerProfile()` → Get server information
- `fetchResults()` → Retrieve last query results
- `executeQuery(String query)` → Execute raw query
- `close()` → Close database connection

### ProtonResponse Methods
- `isSuccess()` → Returns true if operation succeeded
- `getStatus()` → Returns "ok" or "error"
- `getMessage()` → Returns response message
- `toString()` → Returns formatted response string

## 🧪 Testing ProtonDBDriver

### Available Test Classes

1. **TestClient.java** - Basic TCP connectivity and ProtonDBClient operations
2. **DriverQuickTest.java** - ProtonDBDriver core functionality verification ⭐
3. **ProtonDBDriverTest.java** - Comprehensive ProtonDBDriver test suite ⭐

### ProtonDBDriver-Specific Testing

#### Quick Verification Test
```bash
# Compile and run ProtonDBDriver quick test
cd Wrapper/Java/src
javac -cp . com/protondb/*.java
java -cp . com.protondb.DriverQuickTest
```

**What it tests:**
- ProtonDBDriver connection and authentication
- Database creation and selection
- Collection creation
- Document insertion and querying
- Debug mode functionality
- Proper connection cleanup

#### Comprehensive ProtonDBDriver Test
```bash
# Run full ProtonDBDriver test suite
cd Wrapper/Java/src
javac -cp . com/protondb/*.java
java -cp . com.protondb.ProtonDBDriverTest
```

**What it tests:**
- All database operations (create, use, drop, list)
- All collection operations (create, drop, list)
- Complete document CRUD cycle (insert, find, update, delete, count)
- Profile management (create, delete, list users)
- Utility operations (debug, profile, fetch)
- Error handling and edge cases
- Map-to-JSON conversion
- Response parsing and validation

#### Interactive Test Selection
```bash
# Use the build script for guided testing
cd Wrapper/Java
.\build_and_run.bat
# Options:
# 1. Basic Client Test (TestClient)
# 2. ProtonDB Driver Quick Test (DriverQuickTest) ⭐
# 3. Comprehensive Driver Test (ProtonDBDriverTest) ⭐
```

### ProtonDBDriver Test Coverage
- ✅ **Connection Management**: TCP connection, authentication, session handling
- ✅ **Database Operations**: Create, use, drop, list databases
- ✅ **Collection Management**: Create, drop, list collections
- ✅ **Document CRUD**: Insert (JSON/Map), find, update, delete, count
- ✅ **Profile Management**: Create users, delete users, list profiles
- ✅ **Utility Functions**: Debug mode, server profiling, result fetching
- ✅ **Error Handling**: Connection errors, authentication failures, invalid operations
- ✅ **Data Types**: String, Number, Boolean, null value handling
- ✅ **Response Parsing**: JSON parsing, status validation, message extraction

## 🔧 Troubleshooting

### Common Issues

**Connection Failed**
```
Error: Connection refused
```
→ Ensure ProtonDB server is running on port 9090

**Authentication Failed**
```
Error: Authentication failed
```
→ Check username/password (default: admin123/welcome)

**Compilation Errors**
```
Error: cannot find symbol
```
→ Ensure all .java files are compiled together:
```bash
javac -cp . com/protondb/*.java
```

**Runtime ClassNotFoundException**
```
Error: Could not find main class
```
→ Run from the src directory with correct classpath:
```bash
cd src
java -cp . com.protondb.DriverQuickTest
```

## 📖 Additional Documentation

- [Detailed API Documentation](README-JavaDriver.md) - Complete reference with examples
- [ProtonDB Protocol](../../ProtonDB.Server/protocol.json) - Server communication protocol
- [Main Project README](../../README.md) - Overall project documentation

## 🔄 Version History

- **v1.0.0** - Initial release with full driver functionality
  - High-level ProtonDBDriver API
  - Low-level ProtonDBClient interface
  - Comprehensive test suite
  - Complete documentation

## 📄 License

MIT License - Free to use, modify, and distribute.

---

**Status**: ✅ **Production Ready** - Fully functional and tested!
