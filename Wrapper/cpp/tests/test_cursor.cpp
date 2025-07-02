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

// Global cursor object for introspection during failure
static Cursor* activeCursor = nullptr;

#define TEST(name) \
    void name(); \
    int main() { \
        std::cout << "Running " #name "...\n"; \
        name(); \
        if (failures == 0) { \
            std::cout << "[PASS] All Cursor tests passed.\n"; \
            return 0; \
        } else { \
            std::cout << "[FAIL] " << failures << " test(s) failed.\n"; \
            return 1; \
        } \
    } \
    void name()

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

#define ASSERT_NO_THROW_LOG(stmt) \
    do { \
        try { stmt; } catch (const std::exception& e) { \
            std::cerr << "[FAIL] " << __FUNCTION__ << ": exception: " << e.what() << "\n"; \
            ++failures; return; \
        } \
    } while (0)

TEST(testCursorWithLiveServer) {
    Connection conn = Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");
    ASSERT_TRUE_LOG(conn.isConnected());

    Cursor cursor(conn);
    activeCursor = &cursor;

    std::cout << "[Step] db.use(\"helloworld\")\n";
    cursor.execute(R"(db.use("helloworld"))");
    cursor.fetch();
    ASSERT_TRUE_LOG(cursor.status() == "ok");

    std::cout << "[Step] collection.create(\"demo\")\n";
    std::string raw = R"json({"Command":"QUERY","Data":"collection.create(\"demo\")"})json";
    ASSERT_NO_THROW_LOG(cursor.executeRaw(raw));
    ASSERT_NO_THROW_LOG(cursor.fetch());
    ASSERT_TRUE_LOG(cursor.status() == "ok");

    std::cout << "[Step] demo.insert(...)\n";
    cursor.execute(R"(demo.insert({ "name": "Allan", "role": "admin" }))");
    cursor.fetch();
    ASSERT_TRUE_LOG(cursor.status() == "ok");

    std::cout << "[Step] demo.print()\n";
    cursor.execute("demo.print()");
    cursor.fetch();
#if PROTONDB_USE_JSON
    {
        auto data = json::parse(cursor.result());
        ASSERT_TRUE_LOG(!data.empty());
        auto doc = json::parse(data.at(0).get<std::string>());
        ASSERT_TRUE_LOG(doc["name"] == "Allan");
    }
#else
    ASSERT_TRUE_LOG(cursor.result().find("Allan") != std::string::npos);
#endif

    std::cout << "[Step] demo.print(role=\"admin\")\n";
    cursor.execute(R"(demo.print(role = "admin"))");
    cursor.fetch();
#if PROTONDB_USE_JSON
    {
        auto data = json::parse(cursor.result());
        ASSERT_TRUE_LOG(!data.empty());
    }
#else
    ASSERT_TRUE_LOG(cursor.result().find("role") != std::string::npos);
#endif

    conn.close();
    ASSERT_TRUE_LOG(!conn.isConnected());

    activeCursor = nullptr;
}
