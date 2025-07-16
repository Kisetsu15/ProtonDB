cmake_minimum_required(VERSION 3.14)
project(ProtonDBTests CXX)

set(CMAKE_CXX_STANDARD 17)
set(CMAKE_CXX_STANDARD_REQUIRED ON)

# Optional: toggle which test to build (empty = build all)
set(TEST_NAME "" CACHE STRING "Build only a single test without 'test_' prefix")

# Set root dir as one level up from this file (points to wrapper/)
set(ROOT_DIR "${CMAKE_CURRENT_SOURCE_DIR}/..")

# Header and dependency includes
include_directories(
    "${ROOT_DIR}/include"
    "${ROOT_DIR}/third_party"
)

# Library source files
file(GLOB_RECURSE PROTONDB_SOURCES "${ROOT_DIR}/src/*.cpp")

# Define static lib
add_library(protondb STATIC ${PROTONDB_SOURCES})

# Enable optional JSON handling
option(PROTONDB_USE_JSON "Enable JSON response parsing" ON)
if (PROTONDB_USE_JSON)
    target_compile_definitions(protondb PRIVATE PROTONDB_USE_JSON=1)
endif()

# Platform-specific configuration
if (WIN32)
    add_definitions(-DPROTONDB_PLATFORM_WINDOWS)
    target_link_libraries(protondb PRIVATE ws2_32)
else()
    add_definitions(-DPROTONDB_PLATFORM_UNIX)
    # You may need to add additional Unix-specific libraries, e.g., pthread if needed
endif()

# Discover tests/test_*.cpp
file(GLOB TEST_SOURCES "${ROOT_DIR}/tests/test_*.cpp")

foreach(test_file IN LISTS TEST_SOURCES)
    get_filename_component(test_src_name ${test_file} NAME_WE)       # test_connection
    string(REPLACE "test_" "" shortname ${test_src_name})            # connection

    if(TEST_NAME STREQUAL "" OR TEST_NAME STREQUAL shortname)
        add_executable(${test_src_name} ${test_file})
        target_link_libraries(${test_src_name} PRIVATE protondb)
        target_include_directories(${test_src_name} PRIVATE
            "${ROOT_DIR}/include"
            "${ROOT_DIR}/third_party"
        )

        # Platform-specific linking for each test
        if(WIN32)
            target_link_libraries(${test_src_name} PRIVATE ws2_32)
        else()
            # Add Unix-specific libraries for testing if required
        endif()

        enable_testing()
        add_test(NAME ${test_src_name} COMMAND ${test_src_name})
    endif()
endforeach()
