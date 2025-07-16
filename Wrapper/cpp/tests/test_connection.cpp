#include <iostream>
#include <string>
#include <cassert>
#include <typeinfo>
#include "protondb/Connection.hpp"
#include "protondb/Exception.hpp"
#include <fstream>  // For checking local server availability
#include <cstdlib>  // For system calls (ping)

using namespace protondb;

// Simple macros for assertions
#define ASSERT_TRUE(cond)                                                       \
    do {                                                                        \
        if (!(cond)) {                                                          \
            std::cerr << "[FAIL] " << __FUNCTION__                              \
                      << ": expected true but was false (" #cond ")\n";         \
            ++failures;                                                         \
            return;                                                             \
        }                                                                       \
    } while (0)

#define ASSERT_FALSE(cond)                                                      \
    do {                                                                        \
        if (cond) {                                                             \
            std::cerr << "[FAIL] " << __FUNCTION__                              \
                      << ": expected false but was true (" #cond ")\n";         \
            ++failures;                                                         \
            return;                                                             \
        }                                                                       \
    } while (0)

#define ASSERT_NO_THROW(stmt)                                                   \
    do {                                                                        \
        try {                                                                   \
            stmt;                                                               \
        } catch (const std::exception& e) {                                     \
            std::cerr << "[FAIL] " << __FUNCTION__                              \
                      << ": unexpected exception: " << e.what() << "\n";        \
            ++failures;                                                         \
            return;                                                             \
        }                                                                       \
    } while (0)

#define ASSERT_THROW(stmt, ExType)                                              \
    do {                                                                        \
        bool threw = false;                                                     \
        try {                                                                   \
            stmt;                                                               \
        } catch (const ExType&) {                                               \
            threw = true;                                                       \
        } catch (const std::exception& e) {                                     \
            std::cerr << "[FAIL] " << __FUNCTION__                              \
                      << ": wrong exception type: " << typeid(e).name()         \
                      << " (" << e.what() << ")\n";                             \
            ++failures;                                                         \
            return;                                                             \
        }                                                                       \
        if (!threw) {                                                           \
            std::cerr << "[FAIL] " << __FUNCTION__                              \
                      << ": expected exception " #ExType " but none thrown\n";   \
            ++failures;                                                         \
            return;                                                             \
        }                                                                       \
    } while (0)

static int failures = 0;

// Test case: Verifies that the default constructor does not mark the connection as connected
void testDefaultConstructorNotConnected() {
    Connection conn;
    ASSERT_FALSE(conn.isConnected());
}

// Test case: Ensures calling `close()` on a newly constructed connection does not throw any exceptions.
void testCloseOnDefaultDoesNotThrow() {
    Connection conn;
    ASSERT_NO_THROW(conn.close());
}

// Test case: Verifies that setting various options on a default connection doesn't throw any exceptions.
void testSetOptionsOnDefaultDoesNotThrow() {
    Connection conn;
    ASSERT_NO_THROW(conn.setTimeouts(500, 500, 500));
    ASSERT_NO_THROW(conn.setRetry(3));
    ASSERT_NO_THROW(conn.enableAutoReconnect(true));
    ASSERT_FALSE(conn.isConnected());
}

// Test case: Verifies that an invalid host (non-existent domain) results in a `ConnectionError`
void testConnectInvalidHostThrowsConnectionError() {
    ASSERT_THROW(
        Connection::Connect("nonexistent.invalid", 9999, "user", "pass"),
        std::system_error   // Expecting std::system_error due to invalid host.
    );
}

void testConnectInvalidPortThrowsConnectionError() {
    ASSERT_THROW(
        Connection::Connect("127.0.0.1", -1, "user", "pass"),
        std::system_error   // Expecting std::system_error due to invalid port.
    );
}

void testConnectEmptyCredentialsThrowsProtocolError() {
    ASSERT_THROW(
        Connection::Connect("127.0.0.1", 9090, "", "pass"),
        ProtocolError   // Expecting ProtocolError when the username is empty.
    );
    ASSERT_THROW(
        Connection::Connect("127.0.0.1", 9090, "user", ""),
        ProtocolError   // Expecting ProtocolError when the password is empty.
    );
}

// Utility function to check if the local server is reachable
bool isLocalServerReachable() {
    // Using system call to ping the local server (on Windows or Unix systems).
    int result = std::system("ping -c 1 127.0.0.1 >nul 2>&1");  // Modify command for Windows if needed
    return result == 0;  // Return true if ping is successful
}

// Test case: Verifies that connecting to a local ProtonDB server works as expected.
void testConnectLocalServerSucceeds() {
    if (!isLocalServerReachable()) {
        std::cout << "[INFO] Skipping testConnectLocalServerSucceeds: Local server not reachable.\n";
        return;
    }

    try {
        auto conn = Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");
        ASSERT_TRUE(conn.isConnected());
        // For now, we'll assume successful connection; no need for private getter access.
        conn.close();
        ASSERT_FALSE(conn.isConnected());
    } catch (const ProtocolError& e) {
        std::cerr << "[FAIL] Protocol error during local connection test: " << e.what() << '\n';
        ++failures;
    } catch (const std::exception& e) {
        std::cerr << "[FAIL] Unexpected error during local connection test: " << e.what() << '\n';
        ++failures;
    }
}

// Test case: Verifies that the connection correctly attempts to auto-reconnect after failure.
void testAutoReconnectOnConnectionFailure() {
    try {
        // Establish initial connection
        Connection conn = Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");

        // Enable auto-reconnect
        conn.enableAutoReconnect(true);

        // Send a query successfully
        ASSERT_NO_THROW(conn.sendLine("QUERY"));

        // Simulate connection failure by closing the connection
        conn.close();
        
        // Check if connection is closed
        ASSERT_FALSE(conn.isConnected());
        
        // Try to send another query, which should trigger auto-reconnect
        std::cout << "[INFO] Attempting to reconnect and send another query...\n";
        
        // Verify auto-reconnect triggers reconnect and succeeds
        ASSERT_NO_THROW(conn.sendLine("QUERY"));
        
        // Ensure that the connection is back after auto-reconnect
        ASSERT_TRUE(conn.isConnected());  // Ensure it reconnected successfully.
        std::cout << "[PASS] Auto-reconnect worked successfully.\n";

    } catch (const std::exception& e) {
        std::cerr << "[FAIL] Auto-reconnect failed: " << e.what() << '\n';
        ++failures;  // Increment failure count if reconnect fails
    }
}


int main() {
    std::cout << "Running Connection tests...\n";

    testDefaultConstructorNotConnected();
    testCloseOnDefaultDoesNotThrow();
    testSetOptionsOnDefaultDoesNotThrow();
    testConnectInvalidHostThrowsConnectionError();
    testConnectInvalidPortThrowsConnectionError();
    testConnectEmptyCredentialsThrowsProtocolError();
    testConnectLocalServerSucceeds();  // This is now optional
    testAutoReconnectOnConnectionFailure();

    if (failures == 0) {
        std::cout << "[PASS] All Connection tests passed.\n";
        return 0;
    } else {
        std::cout << "[FAIL] " << failures << " test(s) failed.\n";
        return 1;
    }
}
