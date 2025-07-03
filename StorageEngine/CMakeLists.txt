cmake_minimum_required(VERSION 3.31)
project(StorageEngine C)

set(CMAKE_C_STANDARD 11)

add_library(StorageEngine SHARED
        Scripts/StorageEngine.c
        Scripts/StorageEngine.h
        Scripts/cJSON/cJSON.c
        Scripts/cJSON/cJSON.h
        Scripts/DatabaseUtils.c
        Scripts/DatabaseUtils.h
)
