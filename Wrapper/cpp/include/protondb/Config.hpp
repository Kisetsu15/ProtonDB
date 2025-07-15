#pragma once

/// \file Config.hpp
/// \brief Configuration flags and platform macros for ProtonDB client library.

// ─────────────────────────────────────────────
// ProtonDB Client Library - Configuration Flags
// ─────────────────────────────────────────────

//
// Feature Toggles
//

/// Enable nlohmann/json parsing support (1 = enabled)
#ifndef PROTONDB_USE_JSON
#define PROTONDB_USE_JSON 1
#endif

/// Enable TLS support (planned feature, not yet implemented)
#ifndef PROTONDB_ENABLE_TLS
#define PROTONDB_ENABLE_TLS 0
#endif

//
// Default Settings
//

/// Default I/O timeout in milliseconds
#ifndef PROTONDB_DEFAULT_TIMEOUT_MS
#define PROTONDB_DEFAULT_TIMEOUT_MS 5000
#endif

/// Default number of retry attempts for failed sends
#ifndef PROTONDB_DEFAULT_RETRY_COUNT
#define PROTONDB_DEFAULT_RETRY_COUNT 2
#endif

/// Socket read buffer size in bytes
#ifndef PROTONDB_IO_BUFFER_SIZE
#define PROTONDB_IO_BUFFER_SIZE 8192
#endif

/// Wire protocol version string
#ifndef PROTONDB_PROTOCOL_VERSION
#define PROTONDB_PROTOCOL_VERSION "1.0"
#endif

//
// Cross-Platform Support
//

#if defined(_WIN32) || defined(_WIN64)
    /// Defined on Windows platforms
    #define PROTONDB_PLATFORM_WINDOWS 1
    /// Windows socket handle type
    #define PROTONDB_SOCKET_HANDLE uintptr_t
#else
    /// Defined on POSIX platforms (Linux, macOS, etc.)
    #define PROTONDB_PLATFORM_POSIX 1
    /// POSIX socket handle type
    #define PROTONDB_SOCKET_HANDLE int
#endif

//
// Logging / Debug Hooks
//

#ifdef PROTONDB_DEBUG
    #include <iostream>
    /// Internal macro for logging debug messages (stderr)
    #define PROTONDB_LOG(msg) \
        do { std::cerr << "[protondb] " << msg << std::endl; } while(0)
#else
    /// Logging is disabled unless PROTONDB_DEBUG is defined
    #define PROTONDB_LOG(msg) do {} while(0)
#endif

//
// Utility Macros
//

/// Delete copy constructor and assignment operator for a type
#define PROTONDB_DISALLOW_COPY(TypeName) \
    TypeName(const TypeName&) = delete; \
    TypeName& operator=(const TypeName&) = delete

/// Delete move constructor and move assignment operator for a type
#define PROTONDB_DISALLOW_MOVE(TypeName) \
    TypeName(TypeName&&) = delete; \
    TypeName& operator=(TypeName&&) = delete

/// Declare defaulted move constructor and assignment operator
#define PROTONDB_DEFAULT_MOVE(TypeName) \
    TypeName(TypeName&&) noexcept = default; \
    TypeName& operator=(TypeName&&) noexcept = default
