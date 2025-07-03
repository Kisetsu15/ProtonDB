// wrapper/include/protondb/Config.hpp
#pragma once

// ─────────────────────────────────────────────
// ProtonDB Client Library - Configuration Flags
// ─────────────────────────────────────────────

//
// Feature Toggles
//

#ifndef PROTONDB_USE_JSON
#define PROTONDB_USE_JSON 1  // 1 = Enable nlohmann/json parsing support
#endif

#ifndef PROTONDB_ENABLE_TLS
#define PROTONDB_ENABLE_TLS 0  // 1 = Enable TLS (planned feature)
#endif

//
// Default Settings
//

#ifndef PROTONDB_DEFAULT_TIMEOUT_MS
#define PROTONDB_DEFAULT_TIMEOUT_MS 5000  // I/O timeout in milliseconds
#endif

#ifndef PROTONDB_DEFAULT_RETRY_COUNT
#define PROTONDB_DEFAULT_RETRY_COUNT 2    // Retry attempts for failed sends
#endif

#ifndef PROTONDB_IO_BUFFER_SIZE
#define PROTONDB_IO_BUFFER_SIZE 8192      // Socket read buffer size (bytes)
#endif

#ifndef PROTONDB_PROTOCOL_VERSION
#define PROTONDB_PROTOCOL_VERSION "1.0"
#endif

//
// Cross-Platform Support
//

#if defined(_WIN32) || defined(_WIN64)
    #define PROTONDB_PLATFORM_WINDOWS 1
    #define PROTONDB_SOCKET_HANDLE uintptr_t
#else
    #define PROTONDB_PLATFORM_POSIX 1
    #define PROTONDB_SOCKET_HANDLE int
#endif

//
// Logging / Debug Hooks
//

#ifdef PROTONDB_DEBUG
    #include <iostream>
    #define PROTONDB_LOG(msg) \
        do { std::cerr << "[protondb] " << msg << std::endl; } while(0)
#else
    #define PROTONDB_LOG(msg) do {} while(0)
#endif

//
// Utility Macros
//

#define PROTONDB_DISALLOW_COPY(TypeName) \
    TypeName(const TypeName&) = delete; \
    TypeName& operator=(const TypeName&) = delete

#define PROTONDB_DISALLOW_MOVE(TypeName) \
    TypeName(TypeName&&) = delete; \
    TypeName& operator=(TypeName&&) = delete

#define PROTONDB_DEFAULT_MOVE(TypeName) \
    TypeName(TypeName&&) noexcept = default; \
    TypeName& operator=(TypeName&&) noexcept = default
