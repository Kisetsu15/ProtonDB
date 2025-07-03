#include <iostream>
#include <string>
#include <limits>
#include "protondb/Connection.hpp"
#include "protondb/Cursor.hpp"

#if PROTONDB_USE_JSON
# include <nlohmann/json.hpp>
  using json = nlohmann::json;
#endif

void pause() {
    std::cout << "\n<press Enter to continue>";
    std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');
}

void printJsonResult(const std::string& raw, const std::string& label) {
#if PROTONDB_USE_JSON
    std::cout << "\n[" << label << "]\n";
    try {
        auto arr = json::parse(raw);
        if (!arr.is_array()) {
            std::cerr << "[Error] Result is not a JSON array:\n" << raw << "\n";
            return;
        }
        for (const auto& s : arr) {
            auto doc = json::parse(s.get<std::string>());
            std::cout << doc.dump(4) << "\n";
        }
    } catch (const std::exception& e) {
        std::cerr << "[JSON parse error] " << e.what() << "\n";
        std::cerr << "[Raw result] → " << raw << "\n";
    }
#else
    std::cout << raw << "\n";
#endif
}

int main() {
    auto conn = protondb::Connection::Connect("127.0.0.1", 9090, "admin123", "welcome");
    std::cout << "[Connected] isConnected=" << std::boolalpha << conn.isConnected() << "\n";

    protondb::Cursor cursor(conn);

    cursor.execute(R"(db.create("library"))"); cursor.fetch();
    cursor.execute(R"(db.use("library"))");   cursor.fetch();
    cursor.execute(R"(collection.create("books"))"); cursor.fetch();

    while (true) {
        std::cout << R"(
=== Library Menu ===
1) Add book
2) List all books
3) List available books
4) List lent books
5) List by author
6) Lend a book
7) Return a book
8) Exit
> )";

        int choice;
        if (!(std::cin >> choice)) break;
        std::cin.ignore(std::numeric_limits<std::streamsize>::max(), '\n');

        switch (choice) {
          case 1: {
            int id; std::string title, author;
            std::cout << "ID: ";    std::cin  >> id;    std::cin.ignore();
            std::cout << "Title: "; std::getline(std::cin, title);
            std::cout << "Author: ";std::getline(std::cin, author);

            std::ostringstream cmd;
            cmd << "books.insert({"
                << "\"id\":"    << id    << ","
                << "\"title\":\""<< title<<"\","
                << "\"author\":\""<< author<<"\","
                << "\"status\":true"
                << "})";

            cursor.execute(cmd.str());
            cursor.fetch();
            std::cout << cursor.message() << "\n";
            break;
          }

          case 2:
            cursor.execute("books.print()");
            cursor.fetch();
            printJsonResult(cursor.result(), "All Books");
            break;

          case 3:
            cursor.execute("books.print()");
            cursor.fetch();
            printJsonResult(cursor.result(), "Available Books");
            break;

          case 4:
            cursor.execute("books.print()");
            cursor.fetch();
            printJsonResult(cursor.result(), "Lent Books");
            break;

          case 5: {
            std::string author;
            std::cout << "Author: ";
            std::getline(std::cin, author);
            cursor.execute(R"(books.print(author = ")" + author + R"("))");
            cursor.fetch();
            printJsonResult(cursor.result(), "Books by " + author);
            break;
          }

          case 6: {
            int id;
            std::cout << "Book ID to lend: ";
            std::cin >> id; std::cin.ignore();
            cursor.execute("books.update(alter,{\"status\":false},id = " + std::to_string(id) + ")");
            cursor.fetch();
            std::cout << cursor.message() << "\n";
            break;
          }

          case 7: {
            int id;
            std::cout << "Book ID to return: ";
            std::cin >> id; std::cin.ignore();
            cursor.execute("books.update(alter,{\"status\":true},id = " + std::to_string(id) + ")");
            cursor.fetch();
            std::cout << cursor.message() << "\n";
            break;
          }

          case 8:
            conn.close();
            std::cout << "\n[Disconnected]\n";
            return 0;

          default:
            std::cout << "Invalid choice\n";
        }

        pause();
    }

    return 0;
}
