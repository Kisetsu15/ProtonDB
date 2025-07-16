#pragma once

#include <stdexcept>
#include <string>
#include <sstream>
#include <exception>
#include <utility>

namespace protondb {

/// Base class for all ProtonDB client errors.
/// Supports chaining via std::nested_exception.
class ProtonException : public std::runtime_error, public std::nested_exception {
public:
    ProtonException(const std::string& prefix,
                    const std::string& message,
                    const std::string& response = "")
        : std::runtime_error(buildMessage(prefix, message, response)),
          std::nested_exception(),
          response_(response)
    {}

    const std::string& response() const noexcept { return response_; }

private:
    std::string response_;

    static std::string buildMessage(const std::string& prefix,
                                    const std::string& message,
                                    const std::string& response) {
        std::ostringstream oss;
        oss << prefix << ": " << message;
        if (!response.empty()) {
            oss << "\n↳ Response: " << response;
        }
        return oss.str();
    }
};

//------------------------------------------------------------------------------
// Derived Exceptions — All support exception chaining
//------------------------------------------------------------------------------

/// Thrown when a socket/connect-level error occurs.
class ConnectionError : public ProtonException {
public:
    using ProtonException::ProtonException;

    explicit ConnectionError(const std::string& msg)
        : ProtonException("ConnectionError", msg, "") {}
};

/// Thrown when an operation exceeds the configured timeout.
class TimeoutError : public ProtonException {
public:
    using ProtonException::ProtonException;
};

/// Thrown when a connection attempt exceeds the timeout.
class ConnectTimeoutError : public TimeoutError {
public:
    using TimeoutError::TimeoutError;
};

/// Thrown if the server’s response is malformed or violates the protocol.
class ProtocolError : public ProtonException {
public:
    using ProtonException::ProtonException;

    explicit ProtocolError(const std::string& msg)
        : ProtonException("ProtocolError", msg, "") {}
};

/// Thrown when parsing or executing a `.pdb` script fails.
class ScriptParseError : public ProtonException {
public:
    using ProtonException::ProtonException;
};

} // namespace protondb
