cmake_minimum_required(VERSION 3.14)
project(protondb CXX)

# Options
option(BUILD_SHARED_LIBS "Build shared (.dll/.so) instead of static library" OFF)
option(PROTONDB_USE_JSON "Enable JSON response parsing support" ON)

# C++ standard
set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Output all targets to build/lib/
set(CMAKE_ARCHIVE_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)   # Static libraries (.a/.lib)
set(CMAKE_LIBRARY_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)   # Shared libraries (.so/.dylib)
set(CMAKE_RUNTIME_OUTPUT_DIRECTORY ${CMAKE_BINARY_DIR}/lib)   # DLLs and executables (on Windows)

# Sources and headers
set(PROTONDB_HEADERS
    include/protondb/Connection.hpp
    include/protondb/Cursor.hpp
    include/protondb/ScriptRunner.hpp
    include/protondb/Exception.hpp
    include/protondb/Config.hpp
)

# Third party dependencies (json)
include_directories(
    ${CMAKE_CURRENT_SOURCE_DIR}/third_party
)

file(GLOB_RECURSE PROTONDB_SOURCES
    src/*.cpp
)

# Library target
add_library(protondb ${PROTONDB_SOURCES} ${PROTONDB_HEADERS})

# Public interface headers
target_include_directories(protondb
    PUBLIC $<BUILD_INTERFACE:${CMAKE_CURRENT_SOURCE_DIR}/include>
           $<INSTALL_INTERFACE:include>
)

# Optional: enable JSON
if (PROTONDB_USE_JSON)
    target_compile_definitions(protondb PRIVATE PROTONDB_USE_JSON=1)
endif()

# Link platform dependencies
if (WIN32)
    target_link_libraries(protondb PRIVATE ws2_32)
endif()

# Optional install rules (for using from other projects)
include(GNUInstallDirs)

install(TARGETS protondb
    EXPORT protondbTargets
    ARCHIVE DESTINATION ${CMAKE_INSTALL_LIBDIR}
    LIBRARY DESTINATION ${CMAKE_INSTALL_LIBDIR}
    RUNTIME DESTINATION ${CMAKE_INSTALL_BINDIR}
    INCLUDES DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}
)

install(DIRECTORY include/protondb/
    DESTINATION ${CMAKE_INSTALL_INCLUDEDIR}/protondb
)

install(EXPORT protondbTargets
    FILE protondbTargets.cmake
    NAMESPACE protondb::
    DESTINATION ${CMAKE_INSTALL_LIBDIR}/cmake/protondb
)
