cmake_minimum_required(VERSION 3.14)
project(ProtonDBClient CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Build option: toggle JSON support
option(PROTONDB_USE_JSON "Enable JSON response parsing" ON)

# Define root relative to this directory
set(ROOT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../")

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

# Link Windows sockets if building on Windows
if (WIN32)
    target_link_libraries(protondb PRIVATE ws2_32)
endif()

# Build example
add_executable(example_basic "${ROOT_DIR}/examples/example_basic.cpp")
target_link_libraries(example_basic PRIVATE protondb)

# Ensure includes for the example
target_include_directories(example_basic PRIVATE
    "${ROOT_DIR}/include"
    "${ROOT_DIR}/third_party"
)

# Link ws2_32 for the example if on Windows
if (WIN32)
    target_link_libraries(example_basic PRIVATE ws2_32)
endif()
