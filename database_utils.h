#ifndef DATABASE_UTILS_H
#define DATABASE_UTILS_H

#define MAX_DATABASES 100
#define MAX_DB_NAME 64
#define MAX_DB_PATH 128
#define DATABASE_META "database.meta"

typedef struct {
    char name[MAX_DB_NAME];
    char path[MAX_DB_PATH];
} DatabaseInfo;

int check_database(const char* db_name);
void delete_dir_content(const char* dir_path);

void append_database_entry(const char* filename, const char* name, const char* path);
int remove_database_entry(const char* filename, const char* entry);
int load_databases(const char* filename, DatabaseInfo* db_list, const int max_dbs);

#endif //DATABASE_UTILS_H
