package com.protondb;

import java.util.Map;

/**
 * ProtonDB Database Driver - High-level API for database operations
 * 
 * This class provides a convenient interface for interacting with ProtonDB,
 * including database management, collection operations, and document CRUD operations.
 */
public class ProtonDBDriver {
    private ProtonDBClient client;
    private String currentDatabase;
    
    /**
     * Constructor that establishes connection to ProtonDB server
     * @param host Server hostname (default: localhost)
     * @param port Server port (default: 9090)
     * @param username Authentication username
     * @param password Authentication password
     * @throws Exception If connection or authentication fails
     */
    public ProtonDBDriver(String host, int port, String username, String password) throws Exception {
        this.client = new ProtonDBClient(host, port, username, password);
    }
    
    /**
     * Convenience constructor using default host and port
     */
    public ProtonDBDriver(String username, String password) throws Exception {
        this("localhost", 9090, username, password);
    }
    
    // =========================
    // DATABASE OPERATIONS
    // =========================
    
    /**
     * Create a new database
     * @param databaseName Name of the database to create
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse createDatabase(String databaseName) throws Exception {
        return client.query(String.format("database.create(\"%s\")", databaseName));
    }
    
    /**
     * Use/select a database for subsequent operations
     * @param databaseName Name of the database to use
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse useDatabase(String databaseName) throws Exception {
        ProtonResponse response = client.query(String.format("database.use(\"%s\")", databaseName));
        if (response.isSuccess()) {
            this.currentDatabase = databaseName;
        }
        return response;
    }
    
    /**
     * Drop/delete a database
     * @param databaseName Name of the database to drop (null for current database)
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse dropDatabase(String databaseName) throws Exception {
        if (databaseName != null) {
            return client.query(String.format("database.drop(\"%s\")", databaseName));
        } else {
            return client.query("database.drop()");
        }
    }
    
    /**
     * List all available databases
     * @return ProtonResponse with database list
     */
    public ProtonResponse listDatabases() throws Exception {
        return client.query("database.list()");
    }
    
    // =========================
    // COLLECTION OPERATIONS
    // =========================
    
    /**
     * Create a new collection in the current database
     * @param collectionName Name of the collection to create
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse createCollection(String collectionName) throws Exception {
        return client.query(String.format("collection.create(\"%s\")", collectionName));
    }
    
    /**
     * Drop/delete a collection
     * @param collectionName Name of the collection to drop
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse dropCollection(String collectionName) throws Exception {
        return client.query(String.format("collection.drop(\"%s\")", collectionName));
    }
    
    /**
     * List all collections in the current database
     * @return ProtonResponse with collection list
     */
    public ProtonResponse listCollections() throws Exception {
        return client.query("collection.list()");
    }
    
    // =========================
    // DOCUMENT OPERATIONS
    // =========================
    
    /**
     * Insert a document into a collection
     * @param collectionName Name of the collection
     * @param document JSON document as string
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse insertDocument(String collectionName, String document) throws Exception {
        return client.query(String.format("%s.insert(%s)", collectionName, document));
    }
    
    /**
     * Insert a document with key-value pairs
     * @param collectionName Name of the collection
     * @param documentMap Map of key-value pairs
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse insertDocument(String collectionName, Map<String, Object> documentMap) throws Exception {
        String jsonDoc = mapToJson(documentMap);
        return insertDocument(collectionName, jsonDoc);
    }
    
    /**
     * Find documents in a collection
     * @param collectionName Name of the collection
     * @param query Query criteria as JSON string (empty {} for all documents)
     * @return ProtonResponse with found documents
     */
    public ProtonResponse findDocuments(String collectionName, String query) throws Exception {
        return client.query(String.format("%s.find(%s)", collectionName, query));
    }
    
    /**
     * Find all documents in a collection
     * @param collectionName Name of the collection
     * @return ProtonResponse with all documents
     */
    public ProtonResponse findAllDocuments(String collectionName) throws Exception {
        return findDocuments(collectionName, "{}");
    }
    
    /**
     * Update documents in a collection
     * @param collectionName Name of the collection
     * @param query Query criteria to find documents to update
     * @param updateData Update data as JSON string
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse updateDocuments(String collectionName, String query, String updateData) throws Exception {
        return client.query(String.format("%s.update(%s, %s)", collectionName, query, updateData));
    }
    
    /**
     * Delete documents from a collection
     * @param collectionName Name of the collection
     * @param query Query criteria to find documents to delete
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse deleteDocuments(String collectionName, String query) throws Exception {
        return client.query(String.format("%s.delete(%s)", collectionName, query));
    }
    
    /**
     * Count documents in a collection
     * @param collectionName Name of the collection
     * @param query Query criteria (optional, use {} for all)
     * @return ProtonResponse with document count
     */
    public ProtonResponse countDocuments(String collectionName, String query) throws Exception {
        return client.query(String.format("%s.count(%s)", collectionName, query));
    }
    
    /**
     * Print/display documents in a collection (for debugging)
     * @param collectionName Name of the collection
     * @return ProtonResponse with formatted document display
     */
    public ProtonResponse printCollection(String collectionName) throws Exception {
        return client.query(String.format("%s.print()", collectionName));
    }
    
    // =========================
    // PROFILE OPERATIONS
    // =========================
    
    /**
     * Create a new user profile
     * @param username Username for the new profile
     * @param password Password for the new profile
     * @param role Role (admin/user)
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse createProfile(String username, String password, String role) throws Exception {
        return client.query(String.format("profile.create(\"%s\", \"%s\", \"%s\")", username, password, role));
    }
    
    /**
     * Delete a user profile
     * @param username Username to delete
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse deleteProfile(String username) throws Exception {
        return client.query(String.format("profile.delete(\"%s\")", username));
    }
    
    /**
     * List all user profiles
     * @return ProtonResponse with profile list
     */
    public ProtonResponse listProfiles() throws Exception {
        return client.query("profile.list()");
    }
    
    // =========================
    // UTILITY OPERATIONS
    // =========================
    
    /**
     * Enable or disable debug mode
     * @param enable True to enable debug mode, false to disable
     * @return ProtonResponse indicating success or failure
     */
    public ProtonResponse setDebugMode(boolean enable) throws Exception {
        return client.debug(enable);
    }
    
    /**
     * Get server profile information
     * @return ProtonResponse with server profile data
     */
    public ProtonResponse getServerProfile() throws Exception {
        return client.profile();
    }
    
    /**
     * Fetch results from the last query
     * @return ProtonResponse with fetched results
     */
    public ProtonResponse fetchResults() throws Exception {
        return client.fetch();
    }
    
    /**
     * Get the currently selected database
     * @return Name of the current database
     */
    public String getCurrentDatabase() {
        return currentDatabase;
    }
    
    /**
     * Execute a raw query command
     * @param query Raw query string
     * @return ProtonResponse with query result
     */
    public ProtonResponse executeQuery(String query) throws Exception {
        return client.query(query);
    }
    
    /**
     * Close the database connection
     */
    public void close() throws Exception {
        client.close();
    }
    
    // =========================
    // HELPER METHODS
    // =========================
    
    /**
     * Convert a Map to JSON string (simple implementation)
     * @param map Map to convert
     * @return JSON string representation
     */
    private String mapToJson(Map<String, Object> map) {
        StringBuilder json = new StringBuilder("{");
        boolean first = true;
        
        for (Map.Entry<String, Object> entry : map.entrySet()) {
            if (!first) {
                json.append(", ");
            }
            first = false;
            
            json.append("\"").append(escapeJson(entry.getKey())).append("\": ");
            
            Object value = entry.getValue();
            if (value == null) {
                json.append("null");
            } else if (value instanceof String) {
                json.append("\"").append(escapeJson(value.toString())).append("\"");
            } else if (value instanceof Number || value instanceof Boolean) {
                json.append(value.toString());
            } else {
                json.append("\"").append(escapeJson(value.toString())).append("\"");
            }
        }
        
        json.append("}");
        return json.toString();
    }
    
    /**
     * Escape JSON special characters
     * @param str String to escape
     * @return Escaped string
     */
    private String escapeJson(String str) {
        if (str == null) return "";
        return str.replace("\\", "\\\\")
                 .replace("\"", "\\\"")
                 .replace("\n", "\\n")
                 .replace("\r", "\\r")
                 .replace("\t", "\\t");
    }
}
