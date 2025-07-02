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

# ✅ Build the protondb library from source
file(GLOB_RECURSE PROTONDB_SOURCES "${ROOT_DIR}/src/*.cpp")
add_library(protondb STATIC ${PROTONDB_SOURCES})

if (PROTONDB_USE_JSON)
    target_compile_definitions(protondb PRIVATE PROTONDB_USE_JSON=1)
endif()

if (WIN32)
    target_link_libraries(protondb PRIVATE ws2_32)
endif()

# Build executable
add_executable(library library.cpp)
target_link_libraries(library PRIVATE protondb)
target_include_directories(library PRIVATE
    ${ROOT_DIR}/include
    ${ROOT_DIR}/third_party
)

if (WIN32)
    target_link_libraries(library PRIVATE ws2_32)
endif()
