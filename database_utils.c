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

bool remove_entry(const char* meta_file, const char* name, const FileType file_type) {
    const char* file_str = file_type_str(file_type);
    bool status = false;
    cJSON* meta = load_json(meta_file);

    if (!meta || !cJSON_IsObject(meta)) {
        fprintf(stderr, "Error: Could not load or parse JSON file: %s\n", meta_file);
        if (meta) cJSON_Delete(meta);
        return false;
    }

    if (cJSON_HasObjectItem(meta, name)) {
        cJSON_DeleteItemFromObject(meta, name);
        save_json(meta_file, meta);
        status = true;
    } else {
        printf("%s entry '%s' not found.\n", file_str, name);
    }

    cJSON_Delete(meta);
    return status;
}


bool append_entry(const char* meta_file, const char* name, const char* path, const FileType file_type) {
    const char* file_str = file_type_str(file_type);
    bool status = false;
    cJSON* meta = load_json(meta_file);
    if (!meta) {
        meta = cJSON_CreateObject();
    }

    if (!cJSON_HasObjectItem(meta, name)) {
        cJSON_AddStringToObject(meta, name, path);
        save_json(meta_file, meta);
        status = true;
    } else {
        printf("%s '%s' already exists.\n", file_str, name);
    }

    cJSON_Delete(meta);
    return status;
}

const char* file_type_str(const FileType file_type) {
    switch (file_type) {
        case database: return "Database";
        case collection: return "Collection";
        default: return "Unknown";
    }
}

bool dump_binary(const char* file_name, const cJSON* data) {
    if (!cJSON_IsObject(data)) {
        fprintf(stderr, "fatal: Invalid JSON object\n");
        return false;
    }

    char* data_str;
    if (data != NULL) {
        data_str = cJSON_PrintUnformatted(data);
    } else {
        data_str = "[]";
    }

    if (!data_str) {
        fprintf(stderr, "fatal: Failed to convert JSON to string\n");
        return false;
    }

    FILE* file = fopen(file_name, "wb");
    if (!file) {
        perror("fatal: Could not open file for writing");
        free(data_str);
        return false;
    }

    fwrite(data_str, sizeof(char), strlen(data_str), file);
    fclose(file);
    free(data_str);
    return true;
}

cJSON* load_binary(const char* file_name) {
    FILE* file = fopen(file_name, "rb");  // fixed: use `file_name` instead of hardcoded "data.bin"
    if (!file) {
        perror("fatal: Could not open file for reading");
        return NULL;
    }

    fseek(file, 0, SEEK_END);
    long len = ftell(file);
    if (len <= 0) {
        fclose(file);
        fprintf(stderr, "fatal: File is empty or unreadable\n");
        return NULL;
    }

    rewind(file);
    char* buffer = malloc(len + 1);
    if (!buffer) {
        fclose(file);
        fprintf(stderr, "fatal: Memory allocation failed\n");
        return NULL;
    }

    fread(buffer, 1, len, file);
    buffer[len] = '\0';
    fclose(file);

    cJSON* json = cJSON_Parse(buffer);
    free(buffer);

    if (!json) {
        fprintf(stderr, "fatal: Failed to parse JSON from binary\n");
    }

    return json;
}


cJSON* load_json(const char* file_name) {
    FILE* file = fopen(file_name, "r");
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


void get_collection_file(char* array, const char* db_name, const char* name, const int size) {
    snprintf(array, size, "%s/%s/%s.col", DATABASE, db_name, name);
}

void get_collection_meta(char* array, const char* db_name, const int size) {
    snprintf(array, size, "%s/%s/%s", DATABASE, db_name, COLLECTION_META);
}

void get_database_dir(char* array, const char* db_name, const int size) {
    snprintf(array, size, "%s/%s", DATABASE, db_name);
}



