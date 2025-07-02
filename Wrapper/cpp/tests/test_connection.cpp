// tests/test_connection.cpp

#include <iostream>
#include <string>
#include <cassert>
#include <typeinfo>
#include "protondb/Connection.hpp"
#include "protondb/Exception.hpp"

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

// Test cases
void testDefaultConstructorNotConnected() {
    Connection conn;
    ASSERT_FALSE(conn.isConnected());
}

void testCloseOnDefaultDoesNotThrow() {
    Connection conn;
    ASSERT_NO_THROW(conn.close());
}

void testSetOptionsOnDefaultDoesNotThrow() {
    Connection conn;
    ASSERT_NO_THROW(conn.setTimeout(500));
    ASSERT_NO_THROW(conn.setRetry(3));
    ASSERT_NO_THROW(conn.autoReconnect(true));
    ASSERT_FALSE(conn.isConnected());
}

void testConnectInvalidHostThrowsConnectionError() {
    ASSERT_THROW(
        Connection::Connect("nonexistent.invalid", 9999, "user", "pass"),
        ConnectionError
    );
}

void testConnectInvalidPortThrowsConnectionError() {
    ASSERT_THROW(
        Connection::Connect("127.0.0.1", -1, "user", "pass"),
        ConnectionError
    );
}


void testConnectEmptyCredentialsThrowsProtocolError() {
    ASSERT_THROW(
        Connection::Connect("127.0.0.1", 9090, "", "pass"),
        ProtocolError
    );
    ASSERT_THROW(
        Connection::Connect("127.0.0.1", 9090, "user", ""),
        ProtocolError
    );
}

// Optional: requires a running local ProtonDB instance
/*
void testConnectLocalServerSucceeds() {
    auto conn = Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");
    ASSERT_TRUE(conn.isConnected());
    ASSERT_TRUE(conn.getHost() == "127.0.0.1");
    ASSERT_TRUE(conn.getPort() == 9090);
    conn.close();
    ASSERT_FALSE(conn.isConnected());
}
*/

int main() {
    std::cout << "Running Connection tests...\n";

    // List your tests here:
    testDefaultConstructorNotConnected();
    testCloseOnDefaultDoesNotThrow();
    testSetOptionsOnDefaultDoesNotThrow();
    testConnectInvalidHostThrowsConnectionError();
    testConnectInvalidPortThrowsConnectionError();
    testConnectEmptyCredentialsThrowsProtocolError();
    // testConnectLocalServerSucceeds();  // Uncomment if local server is available

    if (failures == 0) {
        std::cout << "[PASS] All Connection tests passed.\n";
        return 0;
    } else {
        std::cout << "[FAIL] " << failures << " test(s) failed.\n";
        return 1;
    }
}
