#pragma once

#include <utility>

#if PROTONDB_PLATFORM_WINDOWS
  #include <winsock2.h>
  using socket_t = SOCKET;
  constexpr socket_t invalid_socket = INVALID_SOCKET;
#else
  #include <unistd.h>
  using socket_t = int;
  constexpr socket_t invalid_socket = -1;
#endif

namespace protondb::internal {

/// @brief RAII wrapper for a socket descriptor.
/// Automatically closes the socket on destruction.
class SocketHandle {
public:
    /// @brief Construct from an existing socket descriptor.
    explicit SocketHandle(socket_t fd) noexcept;

    /// @brief Destructor closes the socket if still open.
    ~SocketHandle();

    // Non-copyable
    SocketHandle(const SocketHandle&) = delete;
    SocketHandle& operator=(const SocketHandle&) = delete;

    /// @brief Move constructor
    SocketHandle(SocketHandle&& other) noexcept;

    /// @brief Move assignment
    SocketHandle& operator=(SocketHandle&& other) noexcept;

    /// @brief Raw file/socket descriptor
    socket_t fd() const noexcept { return fd_; }

    /// @brief Whether the socket is valid
    bool isValid() const noexcept;

private:
    socket_t fd_{invalid_socket};
};

} // namespace protondb::internal
