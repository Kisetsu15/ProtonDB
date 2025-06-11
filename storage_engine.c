#include <direct.h>
#include <stdio.h>
#include "storage_engine.h"
#include "database_utils.h"

char path[128];
char entry[256];

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

    if (_mkdir(path) != 0) {
        perror("fatal: could not create database");
    } else {
        append_database_entry(DATABASE_META, db_name, path);
        printf("Database '%s' created.\n", db_name);
    }
}

void delete_database(const char* db_name) {
    if (!check_database(db_name)) {
        printf("Database '%s' doesn't exists.\n", db_name);
        return;
    }

    snprintf(path, sizeof(path), "db/%s", db_name);
    snprintf(entry, sizeof(entry), "%s|%s", db_name, path);

    delete_dir_content(path);

    if (_rmdir(path) != 0) {
        perror("fatal: could not remove database");
    } else {
        remove_database_entry(DATABASE_META, entry);
        printf("Database '%s' deleted.\n", db_name);
    }
}


