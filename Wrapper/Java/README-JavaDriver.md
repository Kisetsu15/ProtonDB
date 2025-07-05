# ProtonDB Java Driver Documentation

## Overview

The ProtonDBDriver is a comprehensive Java client library for interacting with ProtonDB. It provides a high-level API for database operations, collection management, and document CRUD operations.

## Features

- **Database Management**: Create, use, drop, and list databases
- **Collection Operations**: Create, drop, and list collections
- **Document CRUD**: Insert, find, update, delete, and count documents
- **Profile Management**: Create, delete, and list user profiles
- **Utility Operations**: Debug mode, server profiling, result fetching
- **Type Safety**: Structured response handling with ProtonResponse
- **Flexible Input**: Support for both JSON strings and Map objects for documents

## Quick Start

### 1. Include the ProtonDB Java Driver

Add the compiled classes to your classpath or include the source files in your project.

### 2. Basic Usage

```java
import com.protondb.ProtonDBDriver;
import com.protondb.ProtonResponse;

public class Example {
    public static void main(String[] args) {
        try {
            // Connect to ProtonDB
            ProtonDBDriver driver = new ProtonDBDriver("admin123", "welcome");
            
            // Create and use a database
            driver.createDatabase("MyApp");
            driver.useDatabase("MyApp");
            
            // Create a collection
            driver.createCollection("Users");
            
            // Insert a document
            ProtonResponse response = driver.insertDocument("Users", 
                "{\"name\": \"John Doe\", \"age\": 30, \"email\": \"john@example.com\"}");
            
            if (response.isSuccess()) {
                System.out.println("Document inserted successfully!");
            }
            
            // Find documents
            response = driver.findAllDocuments("Users");
            System.out.println("Found documents: " + response.getMessage());
            
            // Close connection
            driver.close();
            
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
```

## API Reference

### Constructor

#### `ProtonDBDriver(String username, String password)`
Creates a connection using default host (localhost) and port (9090).

#### `ProtonDBDriver(String host, int port, String username, String password)`
Creates a connection with custom host and port.

**Parameters:**
- `host`: Server hostname
- `port`: Server port (default: 9090)
- `username`: Authentication username
- `password`: Authentication password

**Throws:** `Exception` if connection or authentication fails

### Database Operations

#### `createDatabase(String databaseName)`
Creates a new database.

#### `useDatabase(String databaseName)`
Selects a database for subsequent operations.

#### `dropDatabase(String databaseName)`
Drops a database. Pass `null` to drop the current database.

#### `listDatabases()`
Lists all available databases.

#### `getCurrentDatabase()`
Returns the name of the currently selected database.

### Collection Operations

#### `createCollection(String collectionName)`
Creates a new collection in the current database.

#### `dropCollection(String collectionName)`
Drops a collection from the current database.

#### `listCollections()`
Lists all collections in the current database.

### Document Operations

#### `insertDocument(String collectionName, String document)`
Inserts a document using a JSON string.

```java
driver.insertDocument("Users", "{\"name\": \"John\", \"age\": 30}");
```

#### `insertDocument(String collectionName, Map<String, Object> documentMap)`
Inserts a document using a Map of key-value pairs.

```java
Map<String, Object> doc = new HashMap<>();
doc.put("name", "John");
doc.put("age", 30);
driver.insertDocument("Users", doc);
```

#### `findDocuments(String collectionName, String query)`
Finds documents matching the given query criteria.

```java
// Find users with age 30
driver.findDocuments("Users", "{\"age\": 30}");
```

#### `findAllDocuments(String collectionName)`
Finds all documents in a collection.

#### `updateDocuments(String collectionName, String query, String updateData)`
Updates documents matching the query criteria.

```java
driver.updateDocuments("Users", "{\"name\": \"John\"}", "{\"age\": 31}");
```

#### `deleteDocuments(String collectionName, String query)`
Deletes documents matching the query criteria.

#### `countDocuments(String collectionName, String query)`
Counts documents matching the query criteria.

#### `printCollection(String collectionName)`
Prints/displays all documents in a collection (for debugging).

### Profile Operations

#### `createProfile(String username, String password, String role)`
Creates a new user profile.

**Parameters:**
- `username`: Username for the new profile
- `password`: Password for the new profile
- `role`: User role ("admin" or "user")

#### `deleteProfile(String username)`
Deletes a user profile.

#### `listProfiles()`
Lists all user profiles.

### Utility Operations

#### `setDebugMode(boolean enable)`
Enables or disables debug mode on the server.

#### `getServerProfile()`
Retrieves server profile information.

#### `fetchResults()`
Fetches results from the last query operation.

#### `executeQuery(String query)`
Executes a raw query command.

#### `close()`
Closes the database connection.

## Response Handling

All operations return a `ProtonResponse` object with the following methods:

- `isSuccess()`: Returns true if the operation was successful
- `getStatus()`: Returns the status ("ok" or "error")
- `getMessage()`: Returns the response message
- `toString()`: Returns a formatted string representation

```java
ProtonResponse response = driver.createDatabase("TestDB");
if (response.isSuccess()) {
    System.out.println("Success: " + response.getMessage());
} else {
    System.out.println("Error: " + response.getMessage());
}
```

## Examples

### Complete CRUD Example

```java
public class CRUDExample {
    public static void main(String[] args) {
        ProtonDBDriver driver = null;
        try {
            // Connect
            driver = new ProtonDBDriver("admin123", "welcome");
            
            // Setup database and collection
            driver.createDatabase("CRUDDemo");
            driver.useDatabase("CRUDDemo");
            driver.createCollection("Products");
            
            // CREATE - Insert documents
            Map<String, Object> product1 = new HashMap<>();
            product1.put("name", "Laptop");
            product1.put("price", 999.99);
            product1.put("category", "Electronics");
            
            driver.insertDocument("Products", product1);
            driver.insertDocument("Products", "{\"name\": \"Book\", \"price\": 29.99, \"category\": \"Education\"}");
            
            // READ - Find documents
            ProtonResponse allProducts = driver.findAllDocuments("Products");
            System.out.println("All Products: " + allProducts.getMessage());
            
            ProtonResponse electronics = driver.findDocuments("Products", "{\"category\": \"Electronics\"}");
            System.out.println("Electronics: " + electronics.getMessage());
            
            // UPDATE - Modify documents
            driver.updateDocuments("Products", "{\"name\": \"Laptop\"}", "{\"price\": 899.99, \"sale\": true}");
            
            // COUNT - Count documents
            ProtonResponse count = driver.countDocuments("Products", "{}");
            System.out.println("Total products: " + count.getMessage());
            
            // DELETE - Remove documents
            driver.deleteDocuments("Products", "{\"category\": \"Education\"}");
            
        } catch (Exception e) {
            e.printStackTrace();
        } finally {
            if (driver != null) {
                try { driver.close(); } catch (Exception e) { }
            }
        }
    }
}
```

### Profile Management Example

```java
public class ProfileExample {
    public static void main(String[] args) {
        try {
            ProtonDBDriver driver = new ProtonDBDriver("admin123", "welcome");
            
            // Create profiles
            driver.createProfile("alice", "password123", "user");
            driver.createProfile("bob", "securepass", "admin");
            
            // List all profiles
            ProtonResponse profiles = driver.listProfiles();
            System.out.println("Profiles: " + profiles.getMessage());
            
            // Clean up
            driver.deleteProfile("alice");
            driver.deleteProfile("bob");
            
            driver.close();
        } catch (Exception e) {
            e.printStackTrace();
        }
    }
}
```

## Error Handling

Always wrap ProtonDBDriver operations in try-catch blocks:

```java
try {
    ProtonDBDriver driver = new ProtonDBDriver("admin123", "welcome");
    ProtonResponse response = driver.createDatabase("TestDB");
    
    if (!response.isSuccess()) {
        System.err.println("Database creation failed: " + response.getMessage());
    }
    
} catch (Exception e) {
    System.err.println("Connection error: " + e.getMessage());
    e.printStackTrace();
}
```

## Best Practices

1. **Always close connections**: Use try-finally or try-with-resources pattern
2. **Check response status**: Always verify if operations were successful
3. **Handle exceptions**: Wrap operations in appropriate try-catch blocks
4. **Use proper JSON**: Ensure JSON strings are properly formatted
5. **Validate input**: Check for null or empty parameters before operations

## Compilation and Usage

1. **Compile the driver**:
   ```bash
   javac -cp . com/protondb/*.java
   ```

2. **Run your application**:
   ```bash
   java -cp . your.package.YourClass
   ```

3. **Include in your project**: Add the compiled classes to your project's classpath

## Dependencies

- Java 8 or higher
- ProtonDB Server running and accessible
- No external dependencies required

## License

This Java driver is part of the ProtonDB project and follows the same MIT license terms.
