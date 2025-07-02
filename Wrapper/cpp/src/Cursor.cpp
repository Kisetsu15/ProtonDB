#include "protondb/Cursor.hpp"
#include <iostream>

#if PROTONDB_USE_JSON
# include <nlohmann/json.hpp>
  using json = nlohmann::json;
#endif

#include <sstream>

namespace protondb {

#define PROTON_THROW_ERR(ExceptionType, message) throw ExceptionType(message, lastResponse_)

Cursor::Cursor(Connection& conn)
  : conn_(conn)
{}

std::string Cursor::execute(const std::string& command) {
#if PROTONDB_USE_JSON
    json j;
    j["Command"] = "QUERY";   // Always QUERY
    j["Data"] = command;      // DSL string goes here
    auto payload = j.dump();
#else
    auto payload = std::string("{\"Command\":\"QUERY\",\"Data\":\"") + command + "\"}";
#endif

    try {
        lastResponse_ = conn_.sendLine(payload);
    }
    catch (const ProtonException&) {
        throw;
    }
    catch (const std::exception& e) {
        throw ConnectionError("execute failed: " + std::string(e.what()), "");
    }

    parseResponse_();
    return lastResponse_;
}


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
    catch (const std::exception& e) {
        throw ConnectionError("executeRaw failed: " + std::string(e.what()), "");
    }

    parseResponse_();
    return lastResponse_;
}

std::string Cursor::fetch() {
#if PROTONDB_USE_JSON
    json j;
    j["Command"] = "FETCH";
    auto payload = j.dump();
#else
    std::string payload = R"({"Command":"FETCH"})";
#endif

    try {
        lastResponse_ = conn_.sendLine(payload);
    }
    catch (const ProtonException&) {
        throw;
    }
    catch (const std::exception& e) {
        throw ConnectionError("fetch failed: " + std::string(e.what()), "");
    }

    parseResponse_();
    return lastResponse_;
}


const std::string& Cursor::response() const {
    return lastResponse_;
}

std::string Cursor::result() const {
#if PROTONDB_USE_JSON
    if (lastJson_.contains("result")) {
        return lastJson_["result"].dump();
    } else if (lastJson_.contains("Result")) {
        return lastJson_["Result"].dump();
    }
    PROTON_THROW_ERR(ProtocolError, "no \"result\" or \"Result\" field in response");
#else
    return lastResponse_;
#endif
}

std::string Cursor::status() const {
#if PROTONDB_USE_JSON
    if (lastJson_.contains("status")) {
        return lastJson_.value("status", "");
    } else if (lastJson_.contains("Status")) {
        return lastJson_.value("Status", "");
    }
    PROTON_THROW_ERR(ProtocolError, "no \"status\" or \"Status\" field in response");
#else
    auto pos = lastResponse_.find(R"("status":")");
    if (pos == std::string::npos) {
        pos = lastResponse_.find(R"("Status":")");
        if (pos == std::string::npos)
            PROTON_THROW_ERR(ProtocolError, "no \"status\" or \"Status\" field in response");
    }
    pos += 9;
    auto end = lastResponse_.find('"', pos);
    return lastResponse_.substr(pos, end - pos);
#endif
}

std::string Cursor::message() const {
#if PROTONDB_USE_JSON
    if (lastJson_.contains("message")) {
        return lastJson_.value("message", "");
    } else if (lastJson_.contains("Message")) {
        return lastJson_.value("Message", "");
    }
    return {};
#else
    auto pos = lastResponse_.find(R"("message":")");
    if (pos == std::string::npos) {
        pos = lastResponse_.find(R"("Message":")");
        if (pos == std::string::npos) return {};
    }
    pos += 10;
    auto end = lastResponse_.find('"', pos);
    return lastResponse_.substr(pos, end - pos);
#endif
}

void Cursor::parseResponse_() {
    if (lastResponse_.empty()) {
        PROTON_THROW_ERR(ProtocolError, "empty response from server");
    }

#if PROTONDB_USE_JSON
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
#else
    auto st = status();
    if (st != "ok") {
        auto msg = message();
        PROTON_THROW_ERR(ProtocolError, "server error: status=" + st +
                                        (msg.empty() ? "" : ", message=" + msg));
    }
#endif
}

} // namespace protondb
