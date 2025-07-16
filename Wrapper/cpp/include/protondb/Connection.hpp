// wrapper/include/protondb/Connection.hpp
#pragma once

#include <string>
#include <chrono>
#include <memory>
#include "protondb/Exception.hpp"
#include "protondb/Config.hpp"
#include "protondb/internal/SocketHandle.hpp"

namespace protondb {

/// Manages a TCP (and optionally TLS) connection and LOGIN handshake to ProtonDB.
class Connection {
public:
    Connection() = default;
    Connection(const Connection&) = delete;
    Connection& operator=(const Connection&) = delete;

    Connection(Connection&&) noexcept;
    Connection& operator=(Connection&&) noexcept;

    ~Connection();

    /// Establishes a socket connection and performs LOGIN.
    /// Throws ConnectionError, TimeoutError, or ProtocolError on failure.
    static Connection Connect(const std::string& host,
                              int port,
                              const std::string& username,
                              const std::string& password);

    /// Closes the socket and invalidates the connection.
    void close();

    /// Returns true if socket is open and LOGIN has succeeded.
    [[nodiscard]] bool isConnected() const noexcept;

    /// Sets unified read/write/connect timeout (legacy compatibility).
    void setTimeout(int timeoutMs);

    /// Sets individual timeouts for connect, send, and receive (in milliseconds).
    void setTimeouts(int connectMs, int sendMs, int recvMs);


    /// Sets number of retries for transient send/recv failures.
    void setRetry(int retries);

    /// Enables automatic reconnect and login if the socket breaks.
    void enableAutoReconnect(bool enable);

    /// Enables TLS support (unimplemented placeholder).
    void enableTLS(const std::string& certPath);

    /// Sends a JSON-encoded line to the server and waits for response.
    std::string sendLine(const std::string& jsonLine);

    /// Reads a single UTF-8 line from the server (blocking).
    std::string readLine();

    /// Returns the hostname of the connected server.
    [[nodiscard]] const std::string& host() const noexcept { return host_; }

    /// Returns the port of the connected server.
    [[nodiscard]] int port() const noexcept { return port_; }

    [[nodiscard]] int connectTimeout() const noexcept { return connectTimeoutMs_; }
    [[nodiscard]] int sendTimeout() const noexcept    { return sendTimeoutMs_; }
    [[nodiscard]] int recvTimeout() const noexcept    { return recvTimeoutMs_; }

    [[nodiscard]] const std::string& getHost() const noexcept { return host_; }
    [[nodiscard]] int getPort() const noexcept { return port_; }


private:
    bool login_(const std::string& user, const std::string& pass);

    std::string host_;
    int         port_          = 0;
    int         connectTimeoutMs_ = 0;
    int         sendTimeoutMs_    = 0;
    int         recvTimeoutMs_    = 0;
    int         retryCount_    = 0;
    bool        autoReconnect_ = false;

    std::string certPath_;
    std::string user_;         // cached for reconnect
    std::string pass_;         // cached for reconnect

    std::unique_ptr<protondb::internal::SocketHandle> socket_;  // RAII-managed socket


#if PROTONDB_PLATFORM_WINDOWS
    bool wsaInitialized_ = false; // Track WSAStartup state (for Windows only)
#endif
};

} // namespace protondb
