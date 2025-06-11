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

    get_database_dir(path, db_name, sizeof(path));

    if (_mkdir(path) != 0 || !append_entry(DATABASE_META, db_name, path, database)) {
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

    get_database_dir(path, db_name, sizeof(path));
    delete_dir_content(path);

    if (_rmdir(path) != 0 || !remove_entry(DATABASE_META, db_name, database)) {
        perror("fatal: could not remove database");
    } else {
        printf("Database '%s' deleted.\n", db_name);
    }
}

void show_database() {
    cJSON* database_meta = load_json(DATABASE_META);
    if (!database_meta || !cJSON_IsObject(database_meta)) {
        fprintf(stderr, "fatal: Failed to load or parse database metadata.\n");
        if (database_meta) cJSON_Delete(database_meta);
        return;
    }

    cJSON* item = database_meta->child;
    if (!item) {
        printf("No Database found.\n");
    }

    while (item) {
        printf("%s\n", item->string);
        item = item->next;
    }

    cJSON_Delete(database_meta);
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

    get_collection_meta(meta_file, db_name, sizeof(meta_file));
    get_collection_file(path, db_name, collection_name, sizeof(path));

    if (!append_entry(meta_file, collection_name, path, collection)) return;

    cJSON* data = cJSON_CreateObject();
    if (!data || !dump_binary(path, data)) {
        perror("fatal: could not create Collection");
    } else {
        printf("Collection '%s' created.\n", collection_name);
    }

    cJSON_Delete(data);
}

void delete_collection(const char* db_name, const char* collection_name) {
    if (!check_database(db_name)) {
        printf("Database '%s' doesn't exist.\n", db_name);
        return;
    }
    get_collection_meta(meta_file, db_name, sizeof(meta_file));
    if (remove_entry(meta_file, collection_name, collection)) {
        get_collection_file(path, db_name, collection_name, sizeof(path));
        remove(path);
        printf("Collection '%s' deleted.\n", collection_name);
    } else {
        printf("fatal: could not delete collection '%s'\n", collection_name);
    }

}

void show_collection(const char* db_name) {
    get_collection_meta(path, db_name, sizeof(path));

    cJSON* collection_meta = load_json(path);
    if (!collection_meta || !cJSON_IsObject(collection_meta)) {
        fprintf(stderr, "fatal: Failed to load or parse collection metadata for '%s'\n", db_name);
        if (collection_meta) cJSON_Delete(collection_meta);
        return;
    }

    cJSON* item = collection_meta->child;
    if (!item) {
        printf("No collections found in database '%s'.\n", db_name);
    }

    while (item) {
        printf("%s\n", item->string);
        item = item->next;
    }

    cJSON_Delete(collection_meta);
}




