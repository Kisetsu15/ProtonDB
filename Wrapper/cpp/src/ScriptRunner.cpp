// wrapper/src/ScriptRunner.cpp

#include "protondb/ScriptRunner.hpp"

#include <fstream>
#include <algorithm>
#include <cctype>

#if PROTONDB_USE_JSON
# include <nlohmann/json.hpp>
  using json = nlohmann::json;
#endif

namespace protondb {

ScriptRunner::ScriptRunner(Connection& conn)
  : conn_(conn)
  , errorHandler_()
{}

// Install user‐supplied error callback
void ScriptRunner::onScriptError(ErrorHandler handler) {
    errorHandler_ = std::move(handler);
}

// Read commands from file, line‐by‐line
void ScriptRunner::executeScript(const std::string& filename) {
    std::ifstream file(filename);
    if (!file) {
        throw ScriptParseError("cannot open script file: " + filename);
    }
    executeStream(file);
}

// Read commands from any std::istream
void ScriptRunner::executeStream(std::istream& input) {
    std::string line;
    while (std::getline(input, line)) {
        processLine_(line);
    }
}

// Trim, skip blanks/comments, send each command, report errors
void ScriptRunner::processLine_(const std::string& line) {
    // trim leading/trailing whitespace
    auto it1 = std::find_if_not(line.begin(), line.end(),
                                [](unsigned char c){ return std::isspace(c); });
    auto it2 = std::find_if_not(line.rbegin(), line.rend(),
                                [](unsigned char c){ return std::isspace(c); })
                 .base();
    if (it1 >= it2) {
        // empty or all‐whitespace
        return;
    }
    std::string trimmed(it1, it2);

    // skip comment lines
    if (trimmed.front() == '#') {
        return;
    }

    try {
        // build and send payload
    #if PROTONDB_USE_JSON
        json j;
        j["command"] = trimmed;
        conn_.sendLine(j.dump());
    #else
        conn_.sendLine(std::string("{\"command\":\"") + trimmed + "\"}");
    #endif
    }
    catch (const ProtonException& e) {
        if (errorHandler_) {
            errorHandler_(trimmed, e);
        } else {
            // rethrow original exception
            throw;
        }
    }
}

} // namespace protondb
