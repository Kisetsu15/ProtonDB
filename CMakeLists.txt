cmake_minimum_required(VERSION 3.31)
project(MicroDB C)

set(CMAKE_C_STANDARD 11)

add_executable(MicroDB main.c
        storage_engine.h
        storage_engine.c
        database_utils.h
        database_utils.c
        cJSON.h
        cJSON.c)
