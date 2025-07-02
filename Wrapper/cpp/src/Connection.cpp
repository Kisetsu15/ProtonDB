// wrapper/src/Connection.cpp

#include "protondb/Connection.hpp"
#include "protondb/Exception.hpp"
#include <iostream>
#include <sstream>
#include <string>
#include <algorithm>

#if PROTONDB_USE_JSON
# include <nlohmann/json.hpp>
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

Connection::Connection(Connection&& o) noexcept
  : host_(std::move(o.host_))
  , port_(o.port_)
  , sockfd_(o.sockfd_)
  , connected_(o.connected_)
  , timeoutMs_(o.timeoutMs_)
  , retryCount_(o.retryCount_)
  , autoReconnect_(o.autoReconnect_)
  , certPath_(std::move(o.certPath_))
  , user_(std::move(o.user_))
  , pass_(std::move(o.pass_))
#if PROTONDB_PLATFORM_WINDOWS
  , wsaInit_(o.wsaInit_)
#endif
{
    o.sockfd_    = -1;
    o.connected_ = false;
#if PROTONDB_PLATFORM_WINDOWS
    o.wsaInit_   = false;
#endif
}

Connection& Connection::operator=(Connection&& o) noexcept {
    if (this != &o) {
        close();
        host_          = std::move(o.host_);
        port_          = o.port_;
        sockfd_        = o.sockfd_;
        connected_     = o.connected_;
        timeoutMs_     = o.timeoutMs_;
        retryCount_    = o.retryCount_;
        autoReconnect_ = o.autoReconnect_;
        certPath_      = std::move(o.certPath_);
        user_          = std::move(o.user_);
        pass_          = std::move(o.pass_);
#if PROTONDB_PLATFORM_WINDOWS
        wsaInit_       = o.wsaInit_;
        o.wsaInit_     = false;
#endif
        o.sockfd_      = -1;
        o.connected_   = false;
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
    conn.timeoutMs_  = PROTONDB_DEFAULT_TIMEOUT_MS;
    conn.retryCount_ = PROTONDB_DEFAULT_RETRY_COUNT;

  #if PROTONDB_PLATFORM_WINDOWS
    WSADATA wsa;
    if (WSAStartup(MAKEWORD(2,2), &wsa) != 0) {
        throw ConnectionError("WSAStartup failed");
    }
    conn.wsaInit_ = true;
  #endif

    // 1) Resolve address
    addrinfo hints{}, *res = nullptr;
    hints.ai_family   = AF_UNSPEC;
    hints.ai_socktype = SOCK_STREAM;
    auto portStr = std::to_string(port);

    int rc = getaddrinfo(host.c_str(), portStr.c_str(),
                         &hints, &res);
    if (rc != 0 || !res) {
    #if PROTONDB_PLATFORM_WINDOWS
        if (conn.wsaInit_) { WSACleanup(); conn.wsaInit_ = false; }
    #endif
        throw ConnectionError("getaddrinfo: " +
                              std::string(gai_strerror(rc)));
    }

    // 2) Create socket
    int sock = socket(res->ai_family,
                      res->ai_socktype,
                      res->ai_protocol);
    if (sock < 0) {
        freeaddrinfo(res);
    #if PROTONDB_PLATFORM_WINDOWS
        if (conn.wsaInit_) { WSACleanup(); conn.wsaInit_ = false; }
        throw ConnectionError("socket(): " +
                              std::to_string(WSAGetLastError()));
    #else
        throw ConnectionError(std::string("socket(): ") +
                              strerror(errno));
    #endif
    }

    // 3) Connect
    if (::connect(sock, res->ai_addr, res->ai_addrlen) < 0) {
    #if PROTONDB_PLATFORM_WINDOWS
        ::closesocket(sock);
    #else
        ::close(sock);
    #endif
        freeaddrinfo(res);
        throw ConnectionError(
    #if PROTONDB_PLATFORM_WINDOWS
            std::to_string(WSAGetLastError())
    #else
            strerror(errno)
    #endif
        );
    }
    freeaddrinfo(res);

    conn.sockfd_    = sock;
    conn.connected_ = true;

    // 4) Apply timeouts
    conn.setTimeout(conn.timeoutMs_);

    // 4.5) Consume the initial banner (non-JSON greeting)
    try {
        auto banner = conn.readLine();
    } catch (...) {
        // If it fails, we’ll ignore – may happen if server didn't send one
    }

    // 5) Perform LOGIN
    if (!conn.login_(username, password)) {
        conn.close();
        throw ProtocolError("authentication failed");
    }

    return conn;
}

//------------------------------------------------------------------------------
// close / flags / sendLine / readLine
//------------------------------------------------------------------------------

void Connection::close() {
    if (sockfd_ >= 0) {
    #if PROTONDB_PLATFORM_WINDOWS
        ::shutdown(sockfd_, SD_BOTH);
        ::closesocket(sockfd_);
        if (wsaInit_) {
            WSACleanup();
            wsaInit_ = false;
        }
    #else
        ::shutdown(sockfd_, SHUT_RDWR);
        ::close(sockfd_);
    #endif
    }
    sockfd_    = -1;
    connected_ = false;
}

bool Connection::isConnected() const {
    return connected_ && sockfd_ >= 0;
}

void Connection::setTimeout(int ms) {
    timeoutMs_ = ms;
  #if PROTONDB_PLATFORM_WINDOWS
    DWORD tv = static_cast<DWORD>(ms);
    setsockopt(sockfd_, SOL_SOCKET, SO_RCVTIMEO,
               (const char*)&tv, sizeof(tv));
    setsockopt(sockfd_, SOL_SOCKET, SO_SNDTIMEO,
               (const char*)&tv, sizeof(tv));
  #else
    timeval tv{ ms/1000, (ms%1000)*1000 };
    setsockopt(sockfd_, SOL_SOCKET, SO_RCVTIMEO,
               &tv, sizeof(tv));
    setsockopt(sockfd_, SOL_SOCKET, SO_SNDTIMEO,
               &tv, sizeof(tv));
  #endif
}

void Connection::setRetry(int count) {
    retryCount_ = count;
}

void Connection::autoReconnect(bool enabled) {
    autoReconnect_ = enabled;
}

void Connection::enableTLS(const std::string& certPath) {
    (void)certPath;  // TODO: wire up OpenSSL later
}

std::string Connection::sendLine(const std::string& jsonLine) {
    if (!isConnected()) {
        if (autoReconnect_) {
            *this = Connect(host_, port_, user_, pass_);
        }
        else {
            throw ConnectionError("sendLine on closed socket");
        }
    }

    std::string payload = jsonLine;
    if (payload.back() != '\n') payload.push_back('\n');

    int attempts = 0;
    while (attempts <= retryCount_) {
        auto sent = ::send(sockfd_,
                           payload.data(),
                           (int)payload.size(),
                           0);
        if (sent == (ssize_t)payload.size()) break;

    #if PROTONDB_PLATFORM_WINDOWS
        int lasterr = WSAGetLastError();
        if ((lasterr == WSAEINTR || lasterr == WSAEWOULDBLOCK)
            && attempts++ < retryCount_) continue;
        throw ConnectionError("send(): " + std::to_string(lasterr));
    #else
        if ((errno == EINTR || errno == EAGAIN)
            && attempts++ < retryCount_) continue;
        throw ConnectionError(std::string("send(): ") + strerror(errno));
    #endif
    }

    return readLine();
}

std::string Connection::readLine() {
    if (!isConnected()) {
        throw ConnectionError("readLine on closed socket");
    }

    std::string line;
    char c;
    while (true) {
        int n = ::recv(sockfd_, &c, 1, 0);
        if (n == 1) {
            if (c == '\n') break;
            line.push_back(c);
        }
        else if (n == 0) {
            throw ConnectionError("connection closed by peer");
        }
        else {
        #if PROTONDB_PLATFORM_WINDOWS
            int lasterr = WSAGetLastError();
            if (lasterr == WSAEINTR) continue;
            throw ConnectionError("recv(): " + std::to_string(lasterr));
        #else
            if (errno == EINTR) continue;
            throw ConnectionError(std::string("recv(): ") + strerror(errno));
        #endif
        }
    }
    return line;
}

//------------------------------------------------------------------------------
// login_: send LOGIN JSON, parse a single JSON reply
//------------------------------------------------------------------------------

bool Connection::login_(const std::string& user,
                        const std::string& pass)
{
    // Build the exact same JSON payload the Python/C# clients use
    std::ostringstream ss;
    ss << R"({"Command":"LOGIN","Data":")"
    << user << "," << pass
    << R"("})";

    auto payload = ss.str();

    // Send it once.  readLine() now returns *that* JSON, not the banner.
    std::string resp = sendLine(payload);

  #if PROTONDB_USE_JSON
    try {
        auto j = json::parse(resp);

        // read either "status" or "Status", case‐insensitive
        std::string st = j.value("status", "");
        if (st.empty()) st = j.value("Status", "");
        std::transform(st.begin(), st.end(), st.begin(), ::tolower);

        return (st == "ok");
    }
    catch (const std::exception& e) {
        // no valid JSON → fail
        std::cerr << "[Warning] login did not return JSON: "
                  << e.what() << std::endl;
        return false;
    }
  #else
    // legacy: simple substring check
    return (resp.find(R"("status":"ok")") != std::string::npos);
  #endif
}

} // namespace protondb
