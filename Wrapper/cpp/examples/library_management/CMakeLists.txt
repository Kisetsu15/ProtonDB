cmake_minimum_required(VERSION 3.14)
project(LibraryApp CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

option(PROTONDB_USE_JSON "Enable JSON response parsing" ON)

set(ROOT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/../../")

include_directories(
    "${ROOT_DIR}/include"
    "${ROOT_DIR}/third_party"
)

# Collect the source files for protondb library
file(GLOB_RECURSE PROTONDB_SOURCES "${ROOT_DIR}/src/*.cpp")
add_library(protondb STATIC ${PROTONDB_SOURCES})

if (PROTONDB_USE_JSON)
    target_compile_definitions(protondb PRIVATE PROTONDB_USE_JSON=1)
endif()

# Platform-specific linking for socket libraries
if (CMAKE_SYSTEM_NAME STREQUAL "Windows")
    add_definitions(-DPROTONDB_PLATFORM_WINDOWS)
    target_link_libraries(protondb PRIVATE ws2_32)
else()
    add_definitions(-DPROTONDB_PLATFORM_UNIX)
    # Other Unix-specific settings
endif()

# Build the executable
add_executable(library library.cpp)
target_link_libraries(library PRIVATE protondb)

# Ensure the executable includes the correct directories
target_include_directories(library PRIVATE
    ${ROOT_DIR}/include
    ${ROOT_DIR}/third_party
)

# Platform-specific linking for the executable
if (CMAKE_SYSTEM_NAME STREQUAL "Windows")
    target_link_libraries(library PRIVATE ws2_32)
else()
    # Additional Unix-specific settings
endif()
