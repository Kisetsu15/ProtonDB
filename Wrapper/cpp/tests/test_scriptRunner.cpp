// tests/test_script_runner.cpp

#include <iostream>
#include <string>
#include <sstream>
#include <cassert>
#include <typeinfo>
#include "protondb/Connection.hpp"
#include "protondb/ScriptRunner.hpp"
#include "protondb/Exception.hpp"

using namespace protondb;

static int failures = 0;

#define ASSERT_TRUE_LOG(cond) \
    do { \
        if (!(cond)) { \
            std::cerr << "[FAIL] " << __FUNCTION__ << ": " << #cond << "\n"; \
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
    do { bool threw = false; \
        try { stmt; } catch (const ExType&) { threw = true; } \
        catch (const std::exception& e) { \
            std::cerr << "[FAIL] " << __FUNCTION__ << ": wrong exception type: " \
                      << typeid(e).name() << " (" << e.what() << ")\n"; ++failures; return; } \
        if (!threw) { \
            std::cerr << "[FAIL] " << __FUNCTION__ << ": expected exception " #ExType "\n"; \
            ++failures; return; } \
    } while (0)

void testInvalidScriptFileThrows() {
    auto conn = Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");
    ScriptRunner runner(conn);
    ASSERT_THROW_LOG(runner.executeScript("nonexistent_file.txt"), ScriptParseError);
}

void testExecuteStreamWithLiveErrors() {
    auto conn = Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");
    ScriptRunner runner(conn);

    std::stringstream ss("demo.insert({})\n");

    // Now correctly expects ConnectionError (since the socket was closed)
    ASSERT_THROW_LOG(runner.executeStream(ss), ConnectionError);
}


void testExecuteStreamWithErrorCallback() {
    auto conn = Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");
    ScriptRunner runner(conn);

    int errorCount = 0;
    runner.onScriptError(
        [&errorCount](const std::string& line, const ProtonException& err) {
            std::cerr << "[Handled] Line: " << line << " | Error: " << err.what() << "\n";
            ++errorCount;
        }
    );

    std::stringstream ss;
    ss << "# This is a comment\n"
       << "\n"
       << "demo.insert({})\n"   // Invalid if 'demo' not set up
       << "invalid()\n";

    ASSERT_NO_THROW_LOG(runner.executeStream(ss));
    ASSERT_TRUE_LOG(errorCount == 2);
}

int main() {
    std::cout << "Running ScriptRunner integration tests...\n";

    testInvalidScriptFileThrows();
    testExecuteStreamWithLiveErrors();
    testExecuteStreamWithErrorCallback();

    if (failures == 0) {
        std::cout << "[PASS] All ScriptRunner tests passed.\n";
        return 0;
    } else {
        std::cout << "[FAIL] " << failures << " ScriptRunner test(s) failed.\n";
        return 1;
    }
}
