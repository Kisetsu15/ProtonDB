#include <iostream>
#include <string>
#include <cassert>
#include "protondb/Connection.hpp"
#include "protondb/Cursor.hpp"

#if PROTONDB_USE_JSON
#include <nlohmann/json.hpp>
using json = nlohmann::json;
#endif

using namespace protondb;

static int failures = 0;
static Cursor* activeCursor = nullptr; // For introspection during failure

// Assertion macros
#define ASSERT_TRUE_LOG(cond) \
    do { \
        if (!(cond)) { \
            std::cerr << "[FAIL] " << __FUNCTION__ << ": " << #cond << "\n"; \
            if (activeCursor) { \
                std::cerr << "   status  = " << activeCursor->status() << "\n"; \
                std::cerr << "   message = " << activeCursor->message() << "\n"; \
                std::cerr << "   response = " << activeCursor->response() << "\n"; \
            } \
            ++failures; return; \
        } \
    } while (0)

#define ASSERT_FALSE_LOG(cond) \
    do { \
        if (cond) { \
            std::cerr << "[FAIL] " << __FUNCTION__ << ": " << #cond << "\n"; \
            if (activeCursor) { \
                std::cerr << "   status  = " << activeCursor->status() << "\n"; \
                std::cerr << "   message = " << activeCursor->message() << "\n"; \
                std::cerr << "   response = " << activeCursor->response() << "\n"; \
            } \
            ++failures; return; \
        } \
    } while (0)

#define ASSERT_NO_THROW_LOG(stmt) \
    do { \
        try { stmt; } catch (const std::exception& e) { \
            std::cerr << "[FAIL] " << __FUNCTION__ << ": exception: " << e.what() << "\n"; \
            ++failures; return; \
        } \
    } while (0)

#define ASSERT_THROW_LOG(stmt, ExType) \
    do { \
        bool threw = false; \
        try { stmt; } catch (const ExType&) { threw = true; } \
        if (!threw) { \
            std::cerr << "[FAIL] " << __FUNCTION__ << ": expected exception of type " #ExType " but none thrown.\n"; \
            ++failures; return; \
        } \
    } while (0)

// Utility function to check if the local server is reachable
bool isLocalServerReachable() {
    try {
        // Try to connect to the server here; if it fails, return false
        Connection conn = Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");
        return conn.isConnected();  // If we can connect, the server is reachable
    } catch (...) {
        return false;  // If the connection fails, server is not reachable
    }
}

// Test case: Verifies that the default constructor does not mark the connection as connected
void testDefaultConstructorNotConnected() {
    Connection conn;
    ASSERT_FALSE_LOG(conn.isConnected());
}

// Test case: Ensures calling `close()` on a newly constructed connection does not throw any exceptions.
void testCloseOnDefaultDoesNotThrow() {
    Connection conn;
    ASSERT_NO_THROW_LOG(conn.close());
}

// Test case: Verifies that setting various options on a default connection doesn't throw any exceptions.
void testSetOptionsOnDefaultDoesNotThrow() {
    Connection conn;
    ASSERT_NO_THROW_LOG(conn.setTimeouts(500, 500, 500));
    ASSERT_NO_THROW_LOG(conn.setRetry(3));
    ASSERT_NO_THROW_LOG(conn.enableAutoReconnect(true));
    ASSERT_FALSE_LOG(conn.isConnected());
}

// Test case: Verifies that an invalid host (non-existent domain) results in a `ConnectionError`
void testConnectInvalidHostThrowsConnectionError() {
    ASSERT_THROW_LOG(
        Connection::Connect("nonexistent.invalid", 9999, "user", "pass"),
        std::system_error
    );
}

// Test case: Verifies that an invalid port results in a `ConnectionError`.
void testConnectInvalidPortThrowsConnectionError() {
    ASSERT_THROW_LOG(
        Connection::Connect("127.0.0.1", -1, "user", "pass"),
        std::system_error
    );
}

// Test case: Verifies that empty credentials result in a `ProtocolError` due to authentication failure.
void testConnectEmptyCredentialsThrowsProtocolError() {
    ASSERT_THROW_LOG(
        Connection::Connect("127.0.0.1", 9090, "", "pass"),
        ProtocolError
    );
    ASSERT_THROW_LOG(
        Connection::Connect("127.0.0.1", 9090, "user", ""),
        ProtocolError
    );
}

// Test case: Verifies that connecting to a local ProtonDB server works as expected.
void testConnectLocalServerSucceeds() {
    if (!isLocalServerReachable()) {
        std::cout << "[INFO] Skipping testConnectLocalServerSucceeds: Local server not reachable.\n";
        return;
    }

    try {
        auto conn = Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");
        ASSERT_TRUE_LOG(conn.isConnected());
        conn.close();
        ASSERT_FALSE_LOG(conn.isConnected());
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
    // Skip the test if the local server is unreachable
    if (!isLocalServerReachable()) {
        std::cout << "[INFO] Skipping testAutoReconnectOnConnectionFailure: Local server not reachable.\n";
        return;
    }

    try {
        Connection conn = Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");

        conn.enableAutoReconnect(true);

        ASSERT_NO_THROW_LOG(conn.sendLine("QUERY"));

        conn.close();

        ASSERT_FALSE_LOG(conn.isConnected());

        std::cout << "[INFO] Attempting to reconnect and send another query...\n";
        ASSERT_NO_THROW_LOG(conn.sendLine("QUERY"));

        ASSERT_TRUE_LOG(conn.isConnected());
        std::cout << "[PASS] Auto-reconnect worked successfully.\n";

    } catch (const std::exception& e) {
        std::cerr << "[FAIL] Auto-reconnect failed: " << e.what() << '\n';
        ++failures;
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
