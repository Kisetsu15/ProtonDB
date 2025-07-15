// wrapper/src/internal/SocketIO.cpp

#include "protondb/internal/SocketIO.hpp"
#include "protondb/Exception.hpp"

#include <string>
#include <cstring>
#include <stdexcept>

#if PROTONDB_PLATFORM_WINDOWS
  #include <winsock2.h>
  #include <ws2tcpip.h>
#else
  #include <sys/types.h>
  #include <sys/socket.h>
  #include <unistd.h>
  #include <errno.h>
#endif

namespace protondb::internal {

// Utility: Convert system error to string
static std::string systemErrorString() {
#if PROTONDB_PLATFORM_WINDOWS
    return "WSA error " + std::to_string(WSAGetLastError());
#else
    return std::string(strerror(errno));
#endif
}

// Utility: Determine if error is retryable
static bool isRetryableError() {
#if PROTONDB_PLATFORM_WINDOWS
    const int err = WSAGetLastError();
    return err == WSAEINTR || err == WSAEWOULDBLOCK;
#else
    return errno == EINTR || errno == EAGAIN;
#endif
}

/// @brief Sends all bytes reliably with retry support.
bool sendAll(socket_t fd, const char* data, size_t len, int retryCount) {
    size_t totalSent = 0;
    int attempts = 0;

    while (totalSent < len) {
        ssize_t sent = ::send(fd, data + totalSent, static_cast<int>(len - totalSent), 0);
        if (sent > 0) {
            totalSent += static_cast<size_t>(sent);
        } else if (sent == 0) {
            throw ConnectionError("sendAll: connection closed by peer");
        } else {
            if (isRetryableError() && attempts++ < retryCount)
                continue;
            throw ConnectionError("sendAll failed: " + systemErrorString());
        }
    }

    return true;
}

/// @brief Reads from socket until the delimiter is seen.
std::string readUntil(socket_t fd, char delimiter) {
    std::string result;
    char buffer[4096];
    size_t offset = 0;

    while (true) {
        ssize_t bytes = ::recv(fd, buffer + offset, 1, 0);
        if (bytes == 1) {
            if (buffer[offset] == delimiter) {
                result.append(buffer, offset);
                break;
            }
            ++offset;

            // Flush buffer if full
            if (offset >= sizeof(buffer)) {
                result.append(buffer, offset);
                offset = 0;
            }
        } else if (bytes == 0) {
            throw ConnectionError("readUntil: connection closed by peer");
        } else {
            if (isRetryableError()) continue;
            throw ConnectionError("readUntil failed: " + systemErrorString());
        }
    }

    return result;
}

} // namespace protondb::internal
