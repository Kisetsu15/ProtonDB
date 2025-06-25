cmake_minimum_required(VERSION 3.31)
project(ProtonDB.CLI C)

set(CMAKE_C_STANDARD 11)

include_directories(Scripts/C)

add_library(ProtonDB.CLI SHARED
        Scripts/C/cJSON.c
        Scripts/C/cJSON.h
        Scripts/C/DatabaseUtils.c
        Scripts/C/DatabaseUtils.h
        Scripts/C/StorageEngine.c
        Scripts/C/StorageEngine.h)