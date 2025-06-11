#ifndef DATABASE_UTILS_H
#define DATABASE_UTILS_H

#define MAX_DATABASES 100
#define MAX_DB_NAME 64
#define MAX_DB_PATH 128
#define DATABASE "db"
#define DATABASE_META "db/.database.meta"
#define COLLECTION_META ".collection.meta"

#include "cJSON.h"
#include <stdbool.h>

typedef enum {
    collection,
    database
} FileType;

typedef struct {
    char name[MAX_DB_NAME];
    char path[MAX_DB_PATH];
} DatabaseInfo;

bool check_database(const char* db_name);
void delete_dir_content(const char* dir_path);

bool dump_binary(const char* file_name, const cJSON* data);
cJSON* load_binary(const char* file_name);

bool append_entry(const char* meta_file, const char* name, const char* path, const FileType file_type);
bool remove_entry(const char* meta_file, const char* name, const FileType file_type);
const char* file_type_str(const FileType file_type);

cJSON* load_json(const char* file_name);
bool save_json(const char* filename, cJSON* config);

void get_collection_file(char* array, const char* db_name, const char* name, const int size);
void get_collection_meta(char* array, const char* db_name, const int size);
void get_database_dir(char* array, const char* db_name, const int size);

#endif //DATABASE_UTILS_H
