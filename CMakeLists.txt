cmake_minimum_required(VERSION 3.31)
project(MicroDB C)

set(CMAKE_C_STANDARD 11)

add_library(MicroDB SHARED
        Scripts/C/StorageEngine.h
        Scripts/C/StorageEngine.c
        Scripts/C/DatabaseUtils.h
        Scripts/C/DatabaseUtils.c
        Scripts/C/cJSON.h
        Scripts/C/cJSON.c)
