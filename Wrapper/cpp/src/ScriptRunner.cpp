#include "protondb/ScriptRunner.hpp"

#include <fstream>
#include <algorithm>
#include <cctype>
#include <exception>  // for std::throw_with_nested

#if PROTONDB_USE_JSON
  #include <nlohmann/json.hpp>
  using json = nlohmann::json;
#endif

namespace protondb {

// Constructor binds to an authenticated connection
ScriptRunner::ScriptRunner(Connection& conn)
  : conn_(conn), errorHandler_() {}

// Set the callback to handle script errors.
void ScriptRunner::onScriptError(ErrorHandler handler) {
    errorHandler_ = std::move(handler);
}

// Load a script file and execute each valid line.
void ScriptRunner::executeScript(const std::string& filename) {
    std::ifstream file(filename);
    if (!file.is_open()) {
        throw ScriptParseError("ScriptRunner", "cannot open script file", filename);
    }

    try {
        executeStream(file);
    }
    catch (const std::exception&) {
        std::throw_with_nested(ScriptParseError("ScriptRunner", "error while reading script", filename));
    }
}

// Read lines from input stream and dispatch one by one.
void ScriptRunner::executeStream(std::istream& input) {
    std::string line;
    while (std::getline(input, line)) {
        try {
            processLine_(line);
        } catch (const ProtonException& ex) {
            if (errorHandler_) {
                errorHandler_(line, ex); // Call the error handler with the line and the exception
            } else {
                throw;  // Rethrow the exception if no handler is set
            }
        }
    }
}

// Trim line, skip blanks/comments, then send as a command.
void ScriptRunner::processLine_(const std::string& line) {
    // Trim leading/trailing whitespace
    auto start = std::find_if_not(line.begin(), line.end(),
                                  [](unsigned char c) { return std::isspace(c); });
    auto end = std::find_if_not(line.rbegin(), line.rend(),
                                [](unsigned char c) { return std::isspace(c); }).base();

    if (start >= end) return;

    std::string trimmed(start, end);

    // Skip comment lines
    if (!trimmed.empty() && trimmed.front() == '#') return;

    try {
#if PROTONDB_USE_JSON
        json commandPayload = {
            {"Command", trimmed}
        };
        conn_.sendLine(commandPayload.dump());
#else
        conn_.sendLine(std::string("{\"Command\":\"") + trimmed + "\"}");
#endif
    }
    catch (const ProtonException& ex) {
        if (errorHandler_) {
            errorHandler_(trimmed, ex);
        } else {
            throw;  // Rethrow the exception if no handler is set
        }
    }
    catch (const std::exception&) {
        std::throw_with_nested(ScriptParseError("ScriptRunner", "error while sending command", trimmed));
    }
}

} // namespace protondb
