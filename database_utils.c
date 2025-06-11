#include <stdlib.h>
#include <string.h>
#include <direct.h>
#include <io.h>
#include <stdio.h>
#include <stdbool.h>
#include "database_utils.h"
#include "cJSON.h"

bool check_database(const char* db_name) {
    bool exist = false;
    cJSON* database_meta = load_json(DATABASE_META);
    if (cJSON_HasObjectItem(database_meta, db_name)) exist = true;

    cJSON_Delete(database_meta);
    return exist;
}

void delete_dir_content(const char* dir_path) {
    char search_path[256];
    snprintf(search_path, sizeof(search_path), "%s\\*.*", dir_path);

    struct _finddata_t file;
    const intptr_t h = _findfirst(search_path, &file);
    if (h == -1) return;

    do {
        if (strcmp(file.name, ".") != 0 && strcmp(file.name, "..") != 0) {
            char full_path[256];
            snprintf(full_path, sizeof(full_path), "%s\\%s", dir_path, file.name);
            remove(full_path);
        }
    } while (_findnext(h, &file) == 0);

    _findclose(h);
}

bool remove_entry(const char* meta_file, const char* db_name) {
    bool status = false;
    cJSON* meta = load_json(meta_file);

    if (!meta || !cJSON_IsObject(meta)) {
        fprintf(stderr, "Error: Could not load or parse JSON file: %s\n", meta_file);
        if (meta) cJSON_Delete(meta);
        return false;
    }

    if (!cJSON_HasObjectItem(meta, db_name)) {
        cJSON_DeleteItemFromObject(meta, db_name);
        save_json(meta_file, meta);
        printf("Database entry '%s' removed successfully.\n", db_name);
        status = true;
    } else {
        printf("Database entry '%s' not found.\n", db_name);
    }

    cJSON_Delete(meta);
    return status;
}


bool append_entry(const char* meta_file, const char* db_name, const char* path) {
    bool status = false;
    cJSON* meta = load_json(meta_file);
    if (!meta) {
        meta = cJSON_CreateObject();
    }

    if (!cJSON_HasObjectItem(meta, db_name)) {
        cJSON_AddStringToObject(meta, db_name, path);
        save_json(meta_file, meta);
        status = true;
    } else {
        printf("Database '%s' already exists.\n", db_name);
    }

    cJSON_Delete(meta);
    return status;
}

cJSON* load_json(const char* filename) {
    FILE* file = fopen(filename, "r");
    if (!file) return NULL;

    fseek(file, 0, SEEK_END);
    const long len = ftell(file);
    rewind(file);

    char* data = malloc(len + 1);
    fread(data, 1, len, file);
    data[len] = '\0';
    fclose(file);

    cJSON* dict = cJSON_Parse(data);
    free(data);

    if (!dict || !cJSON_IsObject(dict)) {
        if (dict) cJSON_Delete(dict);
        return NULL;
    }

    return dict;
}

bool save_json(const char* filename, cJSON* config) {
    if (!config || !cJSON_IsObject(config)) {
        fprintf(stderr, "fatal: Invalid JSON object passed to save_json\n");
        return false;
    }

    char* json_string = cJSON_Print(config);
    if (!json_string) {
        fprintf(stderr, "fatal: Failed to convert JSON to string\n");
        return false;
    }

    FILE* file = fopen(filename, "w");
    if (!file) {
        perror("Error opening file for writing");
        free(json_string);
        return false;
    }

    fputs(json_string, file);
    fclose(file);
    free(json_string);
    return true;
}

int load_databases(DatabaseInfo* db_list, const int max_dbs) {
    cJSON* config = load_json(DATABASE_META);
    if (!config || !cJSON_IsObject(config)) {
        fprintf(stderr, "fatal: Failed to load or parse JSON from %s\n", DATABASE_META);
        if (config) cJSON_Delete(config);
        return 0;
    }

    cJSON* item = config->child;
    int count = 0;

    while (item && count < max_dbs) {
        if (cJSON_IsString(item)) {
            strncpy(db_list[count].name, item->string, MAX_DB_NAME - 1);
            db_list[count].name[MAX_DB_NAME - 1] = '\0';

            strncpy(db_list[count].path, item->valuestring, MAX_DB_PATH - 1);
            db_list[count].path[MAX_DB_PATH - 1] = '\0';

            count++;
        }
        item = item->next;
    }

    cJSON_Delete(config);
    return count;
}

