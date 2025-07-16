cmake_minimum_required(VERSION 3.14)
project(ProtonDBClient CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Build option: toggle JSON support
option(PROTONDB_USE_JSON "Enable JSON response parsing" ON)

# Define root relative to this directory
set(ROOT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../")

# Print ROOT_DIR for debugging purposes
message(STATUS "ROOT_DIR: ${ROOT_DIR}")

# Global include directories
include_directories(
    "${ROOT_DIR}/include"
    "${ROOT_DIR}/third_party"
)

# Collect library sources
file(GLOB_RECURSE PROTONDB_SOURCES "${ROOT_DIR}/src/*.cpp")

# Define static library
add_library(protondb STATIC ${PROTONDB_SOURCES})

# Enable JSON if requested
if (PROTONDB_USE_JSON)
    target_compile_definitions(protondb PRIVATE PROTONDB_USE_JSON=1)
endif()

# Platform-specific handling
if(WIN32)
    # Windows-specific settings
    target_compile_definitions(protondb PRIVATE PROTONDB_PLATFORM_WINDOWS)
    target_link_libraries(protondb PRIVATE ws2_32)  # Link to Windows sockets
elseif(UNIX)
    # Unix-specific settings (Linux, macOS, etc.)
    target_compile_definitions(protondb PRIVATE PROTONDB_PLATFORM_UNIX)
    # Here you can add Unix-specific libraries if needed
endif()

# Build example
add_executable(example_basic "${ROOT_DIR}/examples/helloWorld/example_basic.cpp")
target_link_libraries(example_basic PRIVATE protondb)

# Ensure includes for the example
target_include_directories(example_basic PRIVATE
    "${ROOT_DIR}/include"
    "${ROOT_DIR}/third_party"
    "${ROOT_DIR}/src"  # Ensure the sources are found during compilation
)

# Link platform-specific libraries for the example
if(WIN32)
    target_link_libraries(example_basic PRIVATE ws2_32)
elseif(UNIX)
    # Add any platform-specific link for Unix-based systems here if needed
endif()

