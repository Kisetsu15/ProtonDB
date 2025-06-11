#ifndef DATABASE_UTILS_H
#define DATABASE_UTILS_H

#define MAX_DATABASES 100
#define MAX_DB_NAME 64
#define MAX_DB_PATH 128
#define DATABASE_META "db/database.meta"

#include "cJSON.h"
#include <stdbool.h>

typedef struct {
    char name[MAX_DB_NAME];
    char path[MAX_DB_PATH];
} DatabaseInfo;

bool check_database(const char* db_name);
void delete_dir_content(const char* dir_path);


bool append_entry(const char* meta_file, const char* db_name, const char* path);
bool remove_entry(const char* meta_file, const char* db_name);
int load_databases(DatabaseInfo* db_list, const int max_dbs);

cJSON* load_json(const char* filename);
bool save_json(const char* filename, cJSON* config);

#endif //DATABASE_UTILS_H
