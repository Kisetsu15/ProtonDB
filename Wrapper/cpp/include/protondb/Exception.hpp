#pragma once

#include <stdexcept>
#include <string>
#include <sstream>

namespace protondb {

/// Base class for all ProtonDB client errors.
class ProtonException : public std::runtime_error {
public:
    ProtonException(const std::string& prefix, const std::string& message, const std::string& response = "")
        : std::runtime_error(buildMessage(prefix, message, response)),
          response_(response)
    {}

    const std::string& response() const noexcept { return response_; }

private:
    std::string response_;

    static std::string buildMessage(const std::string& prefix, const std::string& message, const std::string& response) {
        std::ostringstream oss;
        oss << prefix << ": " << message;
        if (!response.empty()) {
            oss << "\n↳ Response: " << response;
        }
        return oss.str();
    }
};

/// Thrown when a socket/connect-level error occurs.
class ConnectionError : public ProtonException {
public:
    ConnectionError(const std::string& message, const std::string& response = "")
        : ProtonException("ConnectionError", message, response)
    {}
};

/// Thrown when an operation exceeds the configured timeout.
class TimeoutError : public ProtonException {
public:
    TimeoutError(const std::string& message, const std::string& response = "")
        : ProtonException("TimeoutError", message, response)
    {}
};

/// Thrown if the server’s response is malformed or violates the protocol.
class ProtocolError : public ProtonException {
public:
    ProtocolError(const std::string& message, const std::string& response = "")
        : ProtonException("ProtocolError", message, response)
    {}
};

/// Thrown when parsing or executing a `.pdb` script fails.
class ScriptParseError : public ProtonException {
public:
    ScriptParseError(const std::string& message, const std::string& response = "")
        : ProtonException("ScriptParseError", message, response)
    {}
};

} // namespace protondb
