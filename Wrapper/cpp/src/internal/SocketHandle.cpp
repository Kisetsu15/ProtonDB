#include "protondb/internal/SocketHandle.hpp"

namespace protondb::internal {

SocketHandle::SocketHandle(socket_t fd) noexcept
    : fd_{fd} {}

SocketHandle::~SocketHandle() {
    if (isValid()) {
#if PROTONDB_PLATFORM_WINDOWS
        ::closesocket(fd_);
#else
        ::close(fd_);
#endif
    }
}

SocketHandle::SocketHandle(SocketHandle&& other) noexcept
    : fd_{std::exchange(other.fd_, invalid_socket)} {}

SocketHandle& SocketHandle::operator=(SocketHandle&& other) noexcept {
    if (this != &other) {
        if (isValid()) {
#if PROTONDB_PLATFORM_WINDOWS
            ::closesocket(fd_);
#else
            ::close(fd_);
#endif
        }
        fd_ = std::exchange(other.fd_, invalid_socket);
    }
    return *this;
}

bool SocketHandle::isValid() const noexcept {
    return fd_ != invalid_socket;
}

} // namespace protondb::internal
