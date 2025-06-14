cmake_minimum_required(VERSION 3.31)
project(MicroDB C)

set(CMAKE_C_STANDARD 11)

add_executable(MicroDB main.c
        StorageEngine.h
        StorageEngine.c
        DatabaseUtils.h
        DatabaseUtils.c
        cJSON.h
        cJSON.c)
