package com.protondb;

public class ProtonDBClient {
    public Connection conn;

    public ProtonDBClient(String host, int port, String username, String password) throws Exception {
        // All auth logic is handled in the Connection class
        conn = new Connection(host, port, username, password);
    }

    public ProtonResponse query(String command) throws Exception {
        return conn.sendRequest("QUERY", command);
    }

    public ProtonResponse fetch() throws Exception {
        return conn.sendRequest("FETCH", null);
    }

    public ProtonResponse debug(boolean enable) throws Exception {
        return conn.sendRequest("DEBUG", String.valueOf(enable).toLowerCase());
    }

    public ProtonResponse profile() throws Exception {
        return conn.sendRequest("PROFILE", null);
    }

    public void close() throws Exception {
        conn.close();
    }
}
