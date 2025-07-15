// wrapper/src/Connection.cpp

#include "protondb/Connection.hpp"
#include "protondb/Exception.hpp"
#include "protondb/internal/SocketHandle.hpp"
#include "protondb/internal/SocketIO.hpp"

#include <iostream>
#include <sstream>
#include <algorithm>
#include <memory>
#include <system_error>


#if PROTONDB_USE_JSON
  #include <nlohmann/json.hpp>
  using json = nlohmann::json;
#endif

#if PROTONDB_PLATFORM_WINDOWS
  #include <winsock2.h>
  #include <ws2tcpip.h>
  #pragma comment(lib, "Ws2_32.lib")
#else
  #include <sys/types.h>
  #include <sys/socket.h>
  #include <netdb.h>
  #include <unistd.h>
  #include <errno.h>
  #include <cstring>
#endif

namespace protondb {

//------------------------------------------------------------------------------
// move ctor, move assign, dtor
//------------------------------------------------------------------------------

Connection::Connection(Connection&& other) noexcept
  : host_(std::move(other.host_)),
    port_(other.port_),
    connectTimeoutMs_(other.connectTimeoutMs_),
    sendTimeoutMs_(other.sendTimeoutMs_),
    recvTimeoutMs_(other.recvTimeoutMs_),
    retryCount_(other.retryCount_),
    autoReconnect_(other.autoReconnect_),
    certPath_(std::move(other.certPath_)),
    user_(std::move(other.user_)),
    pass_(std::move(other.pass_)),
    socket_(std::move(other.socket_))
#if PROTONDB_PLATFORM_WINDOWS
  , wsaInitialized_(other.wsaInitialized_)
#endif
{
#if PROTONDB_PLATFORM_WINDOWS
    other.wsaInitialized_ = false;
#endif
}

Connection& Connection::operator=(Connection&& other) noexcept {
    if (this != &other) {
        close();
        host_          = std::move(other.host_);
        port_          = other.port_;
        connectTimeoutMs_ = other.connectTimeoutMs_;
        sendTimeoutMs_    = other.sendTimeoutMs_;
        recvTimeoutMs_    = other.recvTimeoutMs_;
        retryCount_    = other.retryCount_;
        autoReconnect_ = other.autoReconnect_;
        certPath_      = std::move(other.certPath_);
        user_          = std::move(other.user_);
        pass_          = std::move(other.pass_);
        socket_        = std::move(other.socket_);
#if PROTONDB_PLATFORM_WINDOWS
        wsaInitialized_ = other.wsaInitialized_;
        other.wsaInitialized_ = false;
#endif
    }
    return *this;
}

Connection::~Connection() {
    close();
}

//------------------------------------------------------------------------------
// static Connect()
//------------------------------------------------------------------------------

Connection Connection::Connect(const std::string& host,
                                int port,
                                const std::string& username,
                                const std::string& password)
{
    Connection conn;
    conn.host_       = host;
    conn.port_       = port;
    conn.user_       = username;
    conn.pass_       = password;
    conn.setTimeouts(PROTONDB_DEFAULT_TIMEOUT_MS,
                 PROTONDB_DEFAULT_TIMEOUT_MS,
                 PROTONDB_DEFAULT_TIMEOUT_MS);
    conn.retryCount_ = PROTONDB_DEFAULT_RETRY_COUNT;

#if PROTONDB_PLATFORM_WINDOWS
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2,2), &wsa) != 0) {
        throw ConnectionError("WSAStartup failed");
    }
    conn.wsaInitialized_ = true;
#endif

    // 1) Resolve address
    addrinfo hints{};
    hints.ai_family   = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;

    addrinfo* res = nullptr;
    std::string portStr = std::to_string(port);
    int rc = getaddrinfo(host.c_str(), portStr.c_str(), &hints, &res);
    if (rc != 0 || !res) {
        throw std::system_error(rc, std::generic_category(), "getaddrinfo() failed");
    }


    // 2) Create socket
    socket_t rawfd = ::socket(res->ai_family, res->ai_socktype, res->ai_protocol);
    if (rawfd < 0) {
        freeaddrinfo(res);
        throw std::system_error(errno, std::system_category(), "socket() failed");
    }
    auto sock = std::make_unique<internal::SocketHandle>(rawfd);



    // 3) Connect
    if (::connect(sock->fd(), res->ai_addr, res->ai_addrlen) < 0) {
        freeaddrinfo(res);
        throw std::system_error(errno, std::system_category(), "connect() failed");
    }
    conn.socket_ = std::move(sock);



    freeaddrinfo(res);

    // 4) Apply timeouts
    conn.setTimeouts(conn.connectTimeoutMs_, conn.sendTimeoutMs_, conn.recvTimeoutMs_);


    // 4.5) Consume the initial banner (non-JSON greeting)
    try {
        conn.readLine();
    } catch (const std::exception& e) {
        std::cerr << "[Warning] failed to read banner: " << e.what() << '\n';
    }


    // 5) Perform LOGIN
    if (!conn.login_(username, password)) {
        conn.close();
        throw ProtocolError(std::string("authentication failed"));
    }

    return conn;
}

//------------------------------------------------------------------------------
// close / flags / sendLine / readLine
//------------------------------------------------------------------------------

void Connection::close() {
    socket_.reset();
#if PROTONDB_PLATFORM_WINDOWS
    if (wsaInitialized_) {
        WSACleanup();
        wsaInitialized_ = false;
    }
#endif
}

bool Connection::isConnected() const noexcept {
    return socket_ && socket_->isValid();
}

void Connection::setTimeouts(int connectMs, int sendMs, int recvMs) {
    connectTimeoutMs_ = connectMs;
    sendTimeoutMs_    = sendMs;
    recvTimeoutMs_    = recvMs;

    if (!isConnected()) return;

#if PROTONDB_PLATFORM_WINDOWS
    DWORD sendTv = static_cast<DWORD>(sendTimeoutMs_);
    DWORD recvTv = static_cast<DWORD>(recvTimeoutMs_);
    if (setsockopt(socket_->fd(), SOL_SOCKET, SO_SNDTIMEO, reinterpret_cast<const char*>(&sendTv), sizeof(sendTv)) != 0) {
        throw std::system_error(WSAGetLastError(), std::system_category(), "setsockopt(SO_SNDTIMEO) failed");
    }
    if (setsockopt(socket_->fd(), SOL_SOCKET, SO_RCVTIMEO, reinterpret_cast<const char*>(&recvTv), sizeof(recvTv)) != 0) {
        throw std::system_error(WSAGetLastError(), std::system_category(), "setsockopt(SO_RCVTIMEO) failed");
    }
#else
    timeval sendTv{ sendTimeoutMs_ / 1000, (sendTimeoutMs_ % 1000) * 1000 };
    timeval recvTv{ recvTimeoutMs_ / 1000, (recvTimeoutMs_ % 1000) * 1000 };
    if (setsockopt(socket_->fd(), SOL_SOCKET, SO_SNDTIMEO, &sendTv, sizeof(sendTv)) != 0) {
        throw std::system_error(errno, std::system_category(), "setsockopt(SO_SNDTIMEO) failed");
    }
    if (setsockopt(socket_->fd(), SOL_SOCKET, SO_RCVTIMEO, &recvTv, sizeof(recvTv)) != 0) {
        throw std::system_error(errno, std::system_category(), "setsockopt(SO_RCVTIMEO) failed");
    }
#endif
}


void Connection::setRetry(int retries) {
    retryCount_ = retries;
}

void Connection::enableAutoReconnect(bool enable) {
    autoReconnect_ = enable;
}

void Connection::enableTLS(const std::string& certPath) {
    certPath_ = certPath;
    // TODO: wire up OpenSSL later
}

std::string Connection::sendLine(const std::string& jsonLine) {
    if (!isConnected()) {
        if (autoReconnect_) {
            std::cout << "[INFO] Attempting to reconnect...\n";
            *this = Connect(host_, port_, user_, pass_);
            std::cout << "[INFO] Reconnected successfully.\n";
        } else {
            throw ConnectionError("sendLine on closed socket");
        }
    }

    std::string payload = jsonLine;
    if (!payload.empty() && payload.back() != '\n') {
        payload.push_back('\n');
    }

    try {
        if (!internal::sendAll(socket_->fd(), payload.data(), payload.size(), retryCount_)) {
            throw ConnectionError("send() failed during sendLine()");
        }
        return readLine();
    } catch (...) {
        std::throw_with_nested(ConnectionError("sendLine failed"));
    }
}



std::string Connection::readLine() {
    if (!isConnected()) {
        throw ConnectionError("readLine on closed socket");
    }

    try {
        return internal::readUntil(socket_->fd(), '\n');
    } catch (...) {
        std::throw_with_nested(ConnectionError("readLine failed"));
    }
}


//------------------------------------------------------------------------------
// login_: send LOGIN JSON, parse a single JSON reply
//------------------------------------------------------------------------------

bool Connection::login_(const std::string& user, const std::string& pass) {
#if PROTONDB_USE_JSON
    json loginPayload = {
        {"Command", "LOGIN"},
        {"Data", user + "," + pass}
    };
    const std::string payload = loginPayload.dump();
#else
    const std::string payload = "{\"Command\":\"LOGIN\",\"Data\":\"" + user + "," + pass + "\"}";
#endif

    const std::string response = sendLine(payload);

#if PROTONDB_USE_JSON
    try {
        const auto j = json::parse(response);
        std::string status = j.value("status", j.value("Status", ""));
        std::transform(status.begin(), status.end(), status.begin(), ::tolower);
        return status == "ok";
    } catch (const std::exception& e) {
        std::cerr << "[Warning] login failed: invalid JSON: " << e.what() << '\n';
        return false;
    }
#else
    return response.find(R"("status":"ok")") != std::string::npos;
#endif
}

} // namespace protondb
