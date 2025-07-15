// wrapper/include/protondb/Cursor.hpp
#pragma once

#include <string>
#include "protondb/Connection.hpp"
#include "protondb/Exception.hpp"
#include "protondb/Config.hpp"

#if PROTONDB_USE_JSON
# include <nlohmann/json.hpp>
  using json = nlohmann::json;
#endif

namespace protondb {

/// Cursor manages a live query session over a ProtonDB connection.
/// It supports executing commands, fetching results, and parsing JSON responses.
class Cursor {
public:
    explicit Cursor(Connection& conn);

    Cursor(const Cursor&) = delete;
    Cursor& operator=(const Cursor&) = delete;
    Cursor(Cursor&&) noexcept = default;
    Cursor& operator=(Cursor&&) noexcept = default;
    ~Cursor() = default;

    /// Execute a DSL command (e.g., SELECT * FROM foo)
    std::string execute(const std::string& command);

    /// Execute a raw JSON string (must conform to protocol)
    std::string executeRaw(const std::string& rawJson);

    /// Send a FETCH command to retrieve next result batch
    std::string fetch();

    /// Set fine-grained socket timeouts via the underlying connection.
    void setTimeouts(int connectMs, int sendMs, int recvMs);

    /// Access the last raw JSON response as string
    const std::string& response() const;

    /// Extract the `result` field from the last response (if JSON mode)
    std::string result() const;

    /// Extract the `status` field from the last response
    std::string status() const;

    /// Extract the `message` field from the last response
    std::string message() const;

private:
    Connection& conn_;
    std::string lastResponse_;

#if PROTONDB_USE_JSON
    json lastJson_;
#endif

    /// Parses lastResponse_ into lastJson_ and validates protocol correctness
    void parseResponse_();
};

} // namespace protondb
