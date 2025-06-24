#include <stdlib.h>
#include <string.h>
#include <io.h>
#include <stdio.h>
#include <stdbool.h>
#include "DatabaseUtils.h"
#include "cJSON.h"
char** names;
char** document;

bool check_database(const char* databaseName) {
    bool exist = false;
    char metaPath[MAX_PATH_LEN];
    get_database_meta(metaPath);
    cJSON* databaseMeta = load_json(metaPath);
    if (cJSON_HasObjectItem(databaseMeta, databaseName)) exist = true;

    cJSON_Delete(databaseMeta);
    return exist;
}

void delete_dir_content(const char* directory) {
    char searchPath[MAX_PATH_LEN];
    snprintf(searchPath, sizeof(searchPath), "%s\\*.*", directory);

    struct _finddata_t file;
    const intptr_t handle = _findfirst(searchPath, &file);
    if (handle == -1) return;

    do {
        if (strcmp(file.name, ".") != 0 && strcmp(file.name, "..") != 0) {
            char fullPath[MAX_PATH_LEN];
            snprintf(fullPath, sizeof(fullPath), "%s\\%s", directory, file.name);
            remove(fullPath);
        }
    } while (_findnext(handle, &file) == 0);

    _findclose(handle);
}

bool remove_entry(const char* metaFile, const char* name, const FileType fileType, char* error) {
    const char* fileString = file_type_string(fileType);
    bool status = false;
    cJSON* metaConfig = load_json(metaFile);

    if (!metaConfig || !cJSON_IsObject(metaConfig)) {
        snprintf(error, MAX_ERROR_LEN, "fatal: Could not load or parse meta file '%s'", metaFile);
        if (metaConfig) cJSON_Delete(metaConfig);
        return false;
    }

    if (cJSON_HasObjectItem(metaConfig, name)) {
        cJSON_DeleteItemFromObject(metaConfig, name);
        status = save_json(metaFile, metaConfig, error);
    } else {
        snprintf(error, MAX_ERROR_LEN,"fatal: %s entry '%s' not found", fileString, name);
    }

    cJSON_Delete(metaConfig);
    return status;
}

bool append_entry(const char* metaFile, const char* name, const char* path, const FileType fileType, char* error) {
    const char* fileString = file_type_string(fileType);
    bool status = false;
    cJSON* meta = load_json(metaFile);
    if (!meta) meta = cJSON_CreateObject();

    if (!cJSON_HasObjectItem(meta, name)) {
        cJSON_AddStringToObject(meta, name, path);
        status = save_json(metaFile, meta, error);
    } else {
        snprintf(error, MAX_ERROR_LEN, "warning: %s '%s' already exists", fileString, name);
    }

    cJSON_Delete(meta);
    return status;
}

const char* file_type_string(const FileType fileType) {
    switch (fileType) {
        case database: return "Database";
        case collection: return "Collection";
        default: return "Unknown";
    }
}

bool dump_binary(const char* fileName, const cJSON* data, char* error) {
    if (!data || !cJSON_IsArray(data)) {
        snprintf(error, MAX_ERROR_LEN, "fatal: Invalid JSON object");
        return false;
    }

    char* dataString = cJSON_PrintUnformatted(data);

    if (!dataString) {
        snprintf(error, MAX_ERROR_LEN, "fatal: Failed to convert JSON to string");
        return false;
    }

    FILE* file = fopen(fileName, "wb");
    if (!file) {
        snprintf(error, MAX_ERROR_LEN,"fatal: Could not open file '%s' for writing", fileName);
        free(dataString);
        return false;
    }

    fwrite(dataString, sizeof(char), strlen(dataString), file);
    fclose(file);
    free(dataString);
    return true;
}

cJSON* load_binary(const char* fileName, char* error) {
    FILE* file = fopen(fileName, "rb");
    if (!file) {
        return NULL;
    }

    fseek(file, 0, SEEK_END);
    long len = ftell(file);
    if (len <= 0) {
        fclose(file);
        snprintf(error, MAX_ERROR_LEN, "fatal: File '%s' is empty or unreadable", fileName);
        return NULL;
    }

    rewind(file);
    char* buffer = malloc(len + 1);
    if (!buffer) {
        fclose(file);
        snprintf(error, MAX_ERROR_LEN, "fatal: Memory allocation failed for reading '%s'", fileName);
        return NULL;
    }

    fread(buffer, 1, len, file);
    buffer[len] = '\0';
    fclose(file);

    cJSON* json = cJSON_Parse(buffer);
    free(buffer);

    if (!json) {
        snprintf(error, MAX_ERROR_LEN,"fatal: Failed to parse JSON from binary");
        return NULL;
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

bool save_json(const char* filename, cJSON* config, char* error) {
    if (!config || !cJSON_IsObject(config)) {
        snprintf(error, MAX_ERROR_LEN, "fatal: Invalid JSON object passed");
        return false;
    }

    char* json_string = cJSON_PrintUnformatted(config);
    if (!json_string) {
        snprintf(error, MAX_ERROR_LEN,"fatal: Failed to convert JSON object to string");
        return false;
    }

    FILE* file = fopen(filename, "w");
    if (!file) {
        snprintf(error, MAX_ERROR_LEN,"fatal: Error opening file for writing");
        free(json_string);
        return false;
    }

    fputs(json_string, file);
    fclose(file);
    free(json_string);
    return true;
}

int print_filtered_documents(cJSON* collection, const char* key, const char* value, const Condition condition, char*** list, char* error) {
    if (condition > all) {
        snprintf(error, MAX_ERROR_LEN,"fatal: Invalid condition specified");
        *list = NULL;
        return 0;
    }

    if (!collection || !cJSON_IsArray(collection)) {
        snprintf(error, MAX_ERROR_LEN,"fatal: Not a valid array format");
        *list = NULL;
        return 0;
    }

    const cJSON* item = NULL;
    const int size = cJSON_GetArraySize(collection);
    if (size == 0) *list = NULL;
    document = malloc(size * sizeof(char*));
    int index = 0;
    cJSON_ArrayForEach(item, collection) {
        if (condition == all || key == NULL || value == NULL) {
            print_item(document, index, item);
        } else {
            bool match = false;
            cJSON* field = cJSON_GetObjectItem(item, key);
            if (!field) continue;
            const bool isNumber = cJSON_IsNumber(field);
            if (!isNumber && condition != equal) continue;
            if (isNumber) {
                match = is_related(field->valuedouble, atof(value), condition);
            } else if (cJSON_IsString(field)) {
                match = strcmp(field->valuestring, value) == 0;
            } else if (cJSON_IsBool(field)) {
                match = (strcmp(value, "true") == 0 && field->valueint == 1) ||
                        (strcmp(value, "false") == 0 && field->valueint == 0);
            }

            if (match) {
                print_item(document, index, item);
            }
         }
        index++;
    }
    cJSON_Delete(collection);
    *list = document;
    return index;
}

void print_item(char** document, const int index, const cJSON* item) {
    char* str = cJSON_Print(item);
    if (str && document != NULL) {
        document[index] = _strdup(str);
        free(str);
    }
}

int load_list(const char* metaFile, char*** list) {
    cJSON* meta = load_json(metaFile);
    if (!meta || !cJSON_IsObject(meta)) {
        if (meta) cJSON_Delete(meta);
        *list = NULL;
        return 0;
    }

    int count = 0;
    cJSON* item = NULL;
    cJSON_ArrayForEach(item, meta) count++;
    names = malloc(count * sizeof(char*));

    int index = 0;
    item = NULL;

    cJSON_ArrayForEach(item, meta) {
        if (!item->string) continue;
        names[index] = _strdup(item->string);
        index++;
    }

    cJSON_Delete(meta);
    *list = names;
    return count;
}



int remove_filtered_documents(cJSON* collection, const char* key, const char* value, const Condition condition, char* error) {
    if (!collection || !cJSON_IsArray(collection)) {
        snprintf(error, MAX_ERROR_LEN,"fatal: Not a valid array format");
        return -1;
    }
    const bool filterEnabled = !(condition == all || key == NULL || value == NULL);
    const int size = cJSON_GetArraySize(collection);
    int deletedCount = 0;

    for (int i = size - 1; i >= 0; i--) {
        bool match = false;
        if (filterEnabled) {
            cJSON* item = cJSON_GetArrayItem(collection, i);
            cJSON* field = cJSON_GetObjectItem(item, key);
            if (!field) continue;
            const bool isNumber = cJSON_IsNumber(field);

            // Only apply condition logic to numeric fields
            if (!isNumber && condition != equal) continue;
            if (isNumber) {
                match = is_related( field->valuedouble, atof(value), condition);
            } else if (cJSON_IsString(field)) {
                match = strcmp(field->valuestring, value) == 0;
            } else if (cJSON_IsBool(field)) {
                match = ((strcmp(value, "true") == 0 && field->valueint == 1) ||
                        (strcmp(value, "false") == 0 && field->valueint == 0));
            }
        } else {
            match = true;
        }

        if (match) {
            cJSON_DeleteItemFromArray(collection, i);
            deletedCount++;
        }
    }

    return deletedCount;
}

int update_filtered_documents(cJSON *collection, const char *key, const char *value, const Condition condition, const Action action, const char *data, char* error) {
    if (!collection || !cJSON_IsArray(collection)) return -1;

    const bool filterEnabled = !(condition == all || key == NULL || value == NULL);
    const int size = cJSON_GetArraySize(collection);
    int updatedCount = 0;

    for (int i = 0; i < size; i++) {
        cJSON *item = cJSON_GetArrayItem(collection, i);
        bool match = false;

        if (filterEnabled) {
            cJSON *field = cJSON_GetObjectItem(item, key);
            if (!field) continue;

            if (cJSON_IsNumber(field)) {
                match = is_related(field->valuedouble, atof(value), condition);
            } else if (cJSON_IsString(field)) {
                match = strcmp(field->valuestring, value) == 0;
            } else if (cJSON_IsBool(field)) {
                match = ((strcmp(value, "true") == 0 && field->valueint == 1) ||
                         (strcmp(value, "false") == 0 && field->valueint == 0));
            } else {
                continue;
            }
        } else {
            match = true;
        }

        if (match) {
            updatedCount++;
            switch (action) {
                case add:
                    if (!add_action(item, data, error)) return -1;
                    break;
                case drop:
                    if (!drop_action(item, data, error)) return -1;
                    break;
                case alter:
                    if (!alter_action(item, data, error)) return -1;
                    break;
                default:
                    snprintf(error, MAX_ERROR_LEN, "fatal: Invalid action specified");
                    return -1;
            }
        }
    }
    return updatedCount;
}


bool add_action(cJSON* item, const char* data, char* error) {
    cJSON* temp = cJSON_Parse(data);
    if (!temp || !cJSON_IsObject(temp)) {
        snprintf(error, MAX_ERROR_LEN,"fatal: Invalid data format '%s'", data);
        if (temp) cJSON_Delete(temp);
        return false;
    }

    cJSON* field = NULL;
    cJSON_ArrayForEach(field, temp) {
        cJSON* copy = cJSON_Duplicate(field, 1);
        cJSON_AddItemToObject(item, field->string, copy);
    }

    cJSON_Delete(temp);
    return true;
}


bool drop_action(cJSON* item, const char* data, char* error) {
    cJSON* temp = cJSON_Parse(data);
    if (!temp || !cJSON_IsString(temp)) {
        snprintf(error, MAX_ERROR_LEN, "fatal: Invalid data format '%s'", data);
        if (temp) cJSON_Delete(temp);
        return false;;
    }

    cJSON_DeleteItemFromObject(item, temp->valuestring);
    cJSON_Delete(temp);
    return true;
}


bool alter_action(cJSON* item, const char* data, char* error) {
    cJSON* temp = cJSON_Parse(data);
    if (!temp || !cJSON_IsObject(temp)) {
        snprintf(error, MAX_ERROR_LEN, "fatal: Invalid data format '%s'", data);
        if (temp) cJSON_Delete(temp);
        return false;
    }

    cJSON* value = temp->child;
    if (!value || !value->string) {
        cJSON_Delete(temp);
        return false;
    }

    bool match = false;
    cJSON* new_value = NULL;

    if (cJSON_IsString(value)) {
        new_value = cJSON_CreateString(value->valuestring);
        match = true;
    } else if (cJSON_IsNumber(value)) {
        new_value = cJSON_CreateNumber(value->valuedouble);
        match = true;
    } else if (cJSON_IsBool(value)) {
        new_value = cJSON_CreateBool(value->valueint);
        match = true;
    } else if (cJSON_IsArray(value)) {
        new_value = cJSON_Duplicate(value, 1);
        match = true;
    } else if (cJSON_IsObject(value)) {
        new_value = cJSON_Duplicate(value, 1);
        match = true;
    } else {
        snprintf(error, MAX_ERROR_LEN, "fatal: Unsupported value type in data");
    }

    if (match) {
        cJSON_ReplaceItemInObject(item, value->string, new_value);
    } else {
        cJSON_Delete(new_value);
    }

    cJSON_Delete(temp);
    return match;
}



bool is_related(const double value1, const double value2, const Condition condition) {
    switch (condition) {
        case greaterThan:           return value1 > value2;
        case greaterThanEqual:      return value1 >= value2;
        case lessThan:              return value1 < value2;
        case lessThanEqual:         return value1 <= value2;
        case equal:                 return value1 == value2;
        default: return false;
    }
}


void get_col_file(char* array, const char* databaseName, const char* collectionName) {
    char* env = getenv("APPDATA");
    snprintf(array, MAX_PATH_LEN, "%s/%s/%s/%s/%s.col", env, PROTON_DB, DATABASE, databaseName, collectionName);
}

void get_col_meta(char* array, const char* databaseName) {
    char* env = getenv("APPDATA");
    snprintf(array, MAX_PATH_LEN, "%s/%s/%s/%s/%s", env, PROTON_DB, DATABASE, databaseName, COLLECTION_META);
}

void get_database_meta(char* array) {
    char* env = getenv("APPDATA");
    snprintf(array, MAX_PATH_LEN, "%s/%s/%s", env, PROTON_DB, DATABASE_META);
}

void get_database_dir(char* array, const char* databaseName) {
    char* env = getenv("APPDATA");
    snprintf(array, MAX_PATH_LEN, "%s/%s/%s/%s", env, PROTON_DB, DATABASE, databaseName);
}

void get_message(char* array, const char* message, const char* object) {
    snprintf(array, MAX_ERROR_LEN, message, object);
}

