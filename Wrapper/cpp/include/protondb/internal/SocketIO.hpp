// wrapper/include/protondb/internal/SocketIO.hpp
#pragma once

#include <string>
#include <cstddef>

#if PROTONDB_PLATFORM_WINDOWS
  #include <winsock2.h>
  using socket_t = SOCKET;
#else
  using socket_t = int;
#endif

namespace protondb::internal {

/// @brief Sends all bytes over the socket, handling partial sends and retries.
/// @param fd         The connected socket.
/// @param data       Pointer to byte buffer.
/// @param len        Number of bytes to send.
/// @param retryCount Number of retry attempts for EINTR/EAGAIN (default: 0).
/// @throws ConnectionError on fatal send failure or disconnect.
/// @return true if all bytes were sent successfully.
bool sendAll(socket_t fd, const char* data, size_t len, int retryCount = 0);

/// @brief Reads from socket until the delimiter is encountered (default: '\n').
/// @param fd         The socket file descriptor.
/// @param delimiter  Character to stop reading at (default: '\n').
/// @throws ConnectionError on socket error or disconnect.
/// @return A complete string up to (but excluding) the delimiter.
std::string readUntil(socket_t fd, char delimiter = '\n');

} // namespace protondb::internal
