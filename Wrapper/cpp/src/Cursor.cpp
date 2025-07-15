// wrapper/src/Cursor.cpp

#include "protondb/Cursor.hpp"
#include <sstream>
#include <iostream>
#include <exception>

#if PROTONDB_USE_JSON
# include <nlohmann/json.hpp>
  using json = nlohmann::json;
#endif

#define PROTON_THROW_ERR(ExceptionType, message) throw ExceptionType(message, lastResponse_)

namespace protondb {

//------------------------------------------------------------------------------
// Constructor
//------------------------------------------------------------------------------

Cursor::Cursor(Connection& conn)
  : conn_(conn)
{}

//------------------------------------------------------------------------------
// Executes a DSL query command as JSON
//------------------------------------------------------------------------------

std::string Cursor::execute(const std::string& command) {
    json j;
    j["Command"] = "QUERY";
    j["Data"] = command;
    const std::string payload = j.dump();

    try {
        lastResponse_ = conn_.sendLine(payload);
    }
    catch (const ProtonException&) {
        throw;
    }
    catch (const std::exception& e) {
        std::throw_with_nested(ConnectionError("execute failed", ""));
    }

    parseResponse_();
    return lastResponse_;
}

//------------------------------------------------------------------------------
// Executes raw pre-built JSON command
//------------------------------------------------------------------------------

std::string Cursor::executeRaw(const std::string& rawJson) {
    if (rawJson.empty()) {
        throw ProtocolError("executeRaw: payload is empty", "");
    }

    try {
        lastResponse_ = conn_.sendLine(rawJson);
    }
    catch (const ProtonException&) {
        throw;
    }
    catch (const std::exception&) {
        std::throw_with_nested(ConnectionError("executeRaw failed", ""));
    }

    parseResponse_();
    return lastResponse_;
}

//------------------------------------------------------------------------------
// Issues FETCH command for incremental data
//------------------------------------------------------------------------------

std::string Cursor::fetch() {
    const std::string payload = R"({"Command":"FETCH"})";

    try {
        lastResponse_ = conn_.sendLine(payload);
    }
    catch (const ProtonException&) {
        throw;
    }
    catch (const std::exception&) {
        std::throw_with_nested(ConnectionError("fetch failed", ""));
    }

    parseResponse_();
    return lastResponse_;
}

//------------------------------------------------------------------------------
// Returns the last raw response string
//------------------------------------------------------------------------------

const std::string& Cursor::response() const {
    return lastResponse_;
}

//------------------------------------------------------------------------------
// Extracts the `result` field from parsed JSON
//------------------------------------------------------------------------------

std::string Cursor::result() const {
    if (lastJson_.contains("result")) {
        return lastJson_["result"].dump();
    } else if (lastJson_.contains("Result")) {
        return lastJson_["Result"].dump();
    }
    PROTON_THROW_ERR(ProtocolError, "no \"result\" or \"Result\" field in response");
}

//------------------------------------------------------------------------------
// Extracts the `status` field from parsed JSON
//------------------------------------------------------------------------------

std::string Cursor::status() const {
    if (lastJson_.contains("status")) {
        return lastJson_.value("status", "");
    } else if (lastJson_.contains("Status")) {
        return lastJson_.value("Status", "");
    }
    PROTON_THROW_ERR(ProtocolError, "no \"status\" or \"Status\" field in response");
}

//------------------------------------------------------------------------------
// Extracts the `message` field from parsed JSON, if present
//------------------------------------------------------------------------------

std::string Cursor::message() const {
    if (lastJson_.contains("message")) {
        return lastJson_.value("message", "");
    } else if (lastJson_.contains("Message")) {
        return lastJson_.value("Message", "");
    }
    return {};
}

//------------------------------------------------------------------------------
// Parses last JSON response, validates `status` and throws if not OK
//------------------------------------------------------------------------------

void Cursor::parseResponse_() {
    if (lastResponse_.empty()) {
        PROTON_THROW_ERR(ProtocolError, "empty response from server");
    }

    try {
        lastJson_ = json::parse(lastResponse_);
    }
    catch (const std::exception& e) {
        PROTON_THROW_ERR(ProtocolError, "invalid JSON in response: " + std::string(e.what()));
    }

    std::string st;
    if (lastJson_.contains("status")) {
        st = lastJson_.value("status", "");
    } else if (lastJson_.contains("Status")) {
        st = lastJson_.value("Status", "");
    }

    if (st != "ok") {
        std::string msg;
        if (lastJson_.contains("message")) {
            msg = lastJson_.value("message", "");
        } else if (lastJson_.contains("Message")) {
            msg = lastJson_.value("Message", "");
        }

        PROTON_THROW_ERR(ProtocolError, "server error: status=" + st +
                                        (msg.empty() ? "" : ", message=" + msg));
    }
}

//------------------------------------------------------------------------------
// Set fine-grained timeout parameters (delegated to Connection)
//------------------------------------------------------------------------------

void Cursor::setTimeouts(int connectMs, int sendMs, int recvMs) {
    conn_.setTimeouts(connectMs, sendMs, recvMs);
}


} // namespace protondb
