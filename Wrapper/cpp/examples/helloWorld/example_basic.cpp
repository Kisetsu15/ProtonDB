#include <iostream>
#include <string>
#include "protondb/Connection.hpp"
#include "protondb/Cursor.hpp"

#if PROTONDB_USE_JSON
#include <nlohmann/json.hpp>
using json = nlohmann::json;
#endif

int main() {
    // Establish connection to ProtonDB server with login credentials
    protondb::Connection conn = protondb::Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");
    std::cout << "[Connection] isConnected: " << std::boolalpha << conn.isConnected() << "\n";

    // Create a command cursor to send DSL or raw protocol commands
    protondb::Cursor cursor(conn);

    // Use the target database 'helloworld'
    cursor.execute(R"(db.use("helloworld"))");
    cursor.fetch();
    std::cout << "\n[execute -> db.use] response:\n" << cursor.response() << "\n";

    // Create a collection called 'demo' using a raw JSON protocol command
    std::string raw = R"json({"Command":"QUERY","Data":"collection.create(\"demo\")"})json";
    cursor.executeRaw(raw);
    cursor.fetch();
    std::cout << "\n[executeRaw -> create collection] status: " << cursor.status() << "\n";
    std::cout << "[message] " << cursor.message() << "\n";
    std::cout << "[result]  " << cursor.result() << "\n";

    // Insert a document into the 'demo' collection using standard DSL
    cursor.execute(R"(demo.insert({ "name": "Allan", "role": "admin" }))");
    cursor.fetch();
    std::cout << "\n[execute -> insert] status: " << cursor.status() << "\n";
    std::cout << "[message] " << cursor.message() << "\n";
    std::cout << "[result]  " << cursor.result() << "\n";

    // Print all documents in 'demo'
    cursor.execute("demo.print()");
    cursor.fetch();
#if PROTONDB_USE_JSON
    std::cout << "\n[demo.print()] result (pretty):\n";
    for (const auto& s : json::parse(cursor.result())) {
        std::cout << json::parse(s.get<std::string>()).dump(4) << "\n";
    }
#else
    std::cout << "\n[demo.print()] result:\n" << cursor.result() << "\n";
#endif

    // Print documents in 'demo' where role = "admin"
    cursor.execute(R"(demo.print(role = "admin"))");
    cursor.fetch();
#if PROTONDB_USE_JSON
    std::cout << "\n[demo.print (filtered)] result (pretty):\n";
    for (const auto& s : json::parse(cursor.result())) {
        std::cout << json::parse(s.get<std::string>()).dump(4) << "\n";
    }
#else
    std::cout << "\n[demo.print (filtered)] result:\n" << cursor.result() << "\n";
#endif

    // Close connection cleanly
    conn.close();
    std::cout << "\n[Disconnected] isConnected: " << std::boolalpha << conn.isConnected() << "\n";

    return 0;
}
