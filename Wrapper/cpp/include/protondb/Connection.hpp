// wrapper/include/protondb/Connection.hpp
#pragma once

#include <string>
#include <chrono>
#include <stdexcept>
#include "protondb/Exception.hpp"
#include "protondb/Config.hpp"

namespace protondb {

/// Manages a TCP/TLS connection and authentication handshake to ProtonDB.
class Connection {
public:
    Connection() = default;
    Connection(const Connection&) = delete;
    Connection& operator=(const Connection&) = delete;
    Connection(Connection&&) noexcept;
    Connection& operator=(Connection&&) noexcept;
    ~Connection();

    /// Establishes socket, performs LOGIN, and returns a live connection.
    /// Throws ConnectionError, TimeoutError, ServerError or ProtocolError on failure.
    static Connection Connect(const std::string& host,
                              int port,
                              const std::string& username,
                              const std::string& password);

    /// Close the socket and invalidate this Connection.
    void close();

    /// True if TCP/TLS socket is open (and LOGIN succeeded).
    bool isConnected() const;

    /// Set read/write timeout (milliseconds) on the socket.
    void setTimeout(int ms);

    /// Number of times to retry a send/receive on transient errors.
    void setRetry(int count);

    /// If enabled, auto-reconnect & re-login on disconnect.
    void autoReconnect(bool enabled);

    /// Placeholder for TLS setup (certificate bundle path).
    void enableTLS(const std::string& certPath);

    /// Send a single JSON line to the server and return its response line.
    std::string sendLine(const std::string& jsonLine);

    /// Read a single UTF-8 JSON line from the server.
    std::string readLine();
    
    /// Get the Current Host IP of the Server
    std::string getHost() const { return host_; }

    /// Get the Current Port of the Server
    int getPort() const { return port_; }


private:
    bool login_(const std::string& user, const std::string& pass);

    std::string host_;
    int         port_          = 0;
    int         sockfd_        = -1;
    bool        connected_     = false;
    int         timeoutMs_     = 0;
    int         retryCount_    = 0;
    bool        autoReconnect_ = false;
    std::string certPath_;

    std::string user_;   // saved for auto-reconnect
    std::string pass_;   // saved for auto-reconnect

#if PROTONDB_PLATFORM_WINDOWS
    bool        wsaInit_ = false;  // WSAStartup state
#endif
};

} // namespace protondb
