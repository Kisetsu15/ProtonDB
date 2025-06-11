#include <direct.h>
#include <stdio.h>
#include "storage_engine.h"
#include "database_utils.h"

char path[128];
char meta_file[128];

void create_database(const char* db_name) {
    if (strlen(db_name) + 4 >= sizeof(path)) {
        printf("Database name too long.\n");
        return;
    }

    if (check_database(db_name)) {
        printf("Database '%s' already exists.\n", db_name);
        return;
    }

    snprintf(path, sizeof(path), "db/%s", db_name);

    if (_mkdir(path) != 0 && !append_entry(DATABASE_META, db_name, path)) {
        perror("fatal: could not create database");
    } else {
        printf("Database '%s' created.\n", db_name);
    }
}

void delete_database(const char* db_name) {
    if (!check_database(db_name)) {
        printf("Database '%s' doesn't exists.\n", db_name);
        return;
    }

    snprintf(path, sizeof(path), "db/%s", db_name);

    delete_dir_content(path);

    if (_rmdir(path) != 0 && !remove_entry(DATABASE_META, db_name)) {
        perror("fatal: could not remove database");
    } else {
        printf("Database '%s' deleted.\n", db_name);
    }
}

void show_database() {
    DatabaseInfo dbs[MAX_DATABASES];
    const int db_count = load_databases(dbs, MAX_DATABASES);
    for (int i = 0; i < db_count; i++) {
        printf("%s\n", dbs[i].name);
    }
}

void create_collection(const char* db_name, const char* collection_name) {
    if (!check_database(db_name)) {
        printf("Database '%s' does not exist.\n", db_name);
        return;
    }

    if (strlen(collection_name) + strlen(db_name) + 8 >= sizeof(path)) {
        printf("Collection name too long.\n");
        return;
    }

    snprintf(meta_file, sizeof(path), "db/%s/collection.meta", db_name);
    snprintf(path, sizeof(path), "db/%s/%s.col", db_name, collection_name);
    append_entry(meta_file, collection_name, path);

    cJSON* data = cJSON_CreateObject();
    if (!data && !save_json(path, data)) {
        perror("fatal: could not create Collection");
    } else {
        printf("Collection '%s' created inside '%s'.\n", collection_name, db_name);
    }
}




