// wrapper/include/protondb/Cursor.hpp
#pragma once

#include <string>
#include <optional>
#include "protondb/Connection.hpp"
#include "protondb/Exception.hpp"
#include "protondb/Config.hpp"

#if PROTONDB_USE_JSON
# include <nlohmann/json.hpp>
  using json = nlohmann::json;
#endif

namespace protondb {

/// A simple command cursor over a live Connection.
/// Sends single‐line commands or raw JSON, and parses the response.
class Cursor {
public:
    /// Construct a cursor that uses an open, authenticated Connection.
    explicit Cursor(Connection& conn);

    Cursor(const Cursor&) = delete;
    Cursor& operator=(const Cursor&) = delete;
    Cursor(Cursor&&) = default;
    Cursor& operator=(Cursor&&) = default;

    ~Cursor() = default;

    /// Send a text‐DSL command (just the command itself).
    /// Returns the full JSON response string.
    std::string execute(const std::string& command);

    /// Send raw JSON (must conform to protocol: {"command":...,"data":...}).
    /// Returns the full JSON response string.
    std::string executeRaw(const std::string& rawJson);

    /// Send an explicit FETCH command.
    std::string fetch();


    /// Retrieve the last full JSON response (unparsed).
    const std::string& response() const;

    /// Retrieve the “result” field from the last response.
    /// If JSON support is disabled, returns the raw response.
    std::string result() const;

    /// Retrieve the “status” field from the last response.
    std::string status() const;

    /// Retrieve the “message” field from the last response.
    std::string message() const;

private:
    Connection&  conn_;
    std::string  lastResponse_;

#if PROTONDB_USE_JSON
    json         lastJson_;
#endif

    /// Parse lastResponse_ into lastJson_ (if enabled) or validate format.
    void parseResponse_();
};

} // namespace protondb
