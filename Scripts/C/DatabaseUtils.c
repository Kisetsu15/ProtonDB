#include <stdlib.h>
#include <string.h>
#include <io.h>
#include <stdio.h>
#include <stdbool.h>
#include "DatabaseUtils.h"
#include "cJSON.h"

char* pair = NULL;

bool CheckDatabase(const char* databaseName) {
    bool exist = false;
    cJSON* databaseMeta = LoadJson(DATABASE_META);
    if (cJSON_HasObjectItem(databaseMeta, databaseName)) exist = true;

    cJSON_Delete(databaseMeta);
    return exist;
}

void DeleteDirectoryContent(const char* directory) {
    char searchPath[256];
    snprintf(searchPath, sizeof(searchPath), "%s\\*.*", directory);

    struct _finddata_t file;
    const intptr_t handle = _findfirst(searchPath, &file);
    if (handle == -1) return;

    do {
        if (strcmp(file.name, ".") != 0 && strcmp(file.name, "..") != 0) {
            char fullPath[256];
            snprintf(fullPath, sizeof(fullPath), "%s\\%s", directory, file.name);
            remove(fullPath);
        }
    } while (_findnext(handle, &file) == 0);

    _findclose(handle);
}

bool RemoveEntry(const char* metaFile, const char* name, const FileType fileType) {
    const char* fileString = FileTypeString(fileType);
    bool status = false;
    cJSON* metaConfig = LoadJson(metaFile);

    if (!metaConfig || !cJSON_IsObject(metaConfig)) {
        fprintf(stderr, "fatal: Could not load or parse JSON file: %s\n", metaFile);
        if (metaConfig) cJSON_Delete(metaConfig);
        return false;
    }

    if (cJSON_HasObjectItem(metaConfig, name)) {
        cJSON_DeleteItemFromObject(metaConfig, name);
        SaveJson(metaFile, metaConfig);
        status = true;
    } else {
        printf("fatal: %s entry '%s' not found.\n", fileString, name);
    }

    cJSON_Delete(metaConfig);
    return status;
}


bool AppendEntry(const char* metaFile, const char* name, const char* path, const FileType fileType) {
    const char* fileString = FileTypeString(fileType);
    bool status = false;
    cJSON* meta = LoadJson(metaFile);
    if (!meta) {
        meta = cJSON_CreateObject();
    }

    if (!cJSON_HasObjectItem(meta, name)) {
        cJSON_AddStringToObject(meta, name, path);
        SaveJson(metaFile, meta);
        status = true;
    } else {
        printf("warning: %s '%s' already exists.\n", fileString, name);
    }

    cJSON_Delete(meta);
    return status;
}

const char* FileTypeString(const FileType fileType) {
    switch (fileType) {
        case database: return "Database";
        case collection: return "Collection";
        default: return "Unknown";
    }
}

bool DumpBinary(const char* fileName, const cJSON* data) {
    if (!data || !cJSON_IsArray(data)) {
        fprintf(stderr, "fatal: Invalid JSON object\n");
        return false;
    }

    char* dataString = cJSON_PrintUnformatted(data);

    if (!dataString) {
        fprintf(stderr, "fatal: Failed to convert JSON to string\n");
        return false;
    }

    FILE* file = fopen(fileName, "wb");
    if (!file) {
        perror("fatal: Could not open file for writing");
        free(dataString);
        return false;
    }

    fwrite(dataString, sizeof(char), strlen(dataString), file);
    fclose(file);
    free(dataString);
    return true;
}

cJSON* LoadBinary(const char* fileName) {
    FILE* file = fopen(fileName, "rb");
    if (!file) {
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
        return NULL;
    }

    return json;
}


cJSON* LoadJson(const char* file_name) {
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

bool SaveJson(const char* filename, cJSON* config) {
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
        perror("fatal: Error opening file for writing");
        free(json_string);
        return false;
    }

    fputs(json_string, file);
    fclose(file);
    free(json_string);
    return true;
}

void PrintFilteredDocuments(const cJSON* collection, const char* key, const char* value, const Condition condition) {
    if (condition > all) {
        printf("fatal: Invalid condition specified.\n");
        return;
    }

    if (!collection || !cJSON_IsArray(collection)) {
        printf("fatal: Not a valid array formats.\n");
        return;
    }

    const cJSON* item = NULL;
    cJSON_ArrayForEach(item, collection) {
        if (condition == all || key == NULL || value == NULL) {
            PrintItem(item);
        } else {
            bool match = false;
            cJSON* field = cJSON_GetObjectItem(item, key);
            if (!field) continue;
            const bool isNumber = cJSON_IsNumber(field);
            if (!isNumber && condition != equal) continue;
            if (isNumber) {
                match = IsRelated(field->valuedouble, atof(value), condition);
            } else if (cJSON_IsString(field)) {
                match = strcmp(field->valuestring, value) == 0;
            } else if (cJSON_IsBool(field)) {
                match = (strcmp(value, "true") == 0 && field->valueint == 1) ||
                        (strcmp(value, "false") == 0 && field->valueint == 0);
            }

            if (match) {
                PrintItem(item);
            }
         }
    }
}

void PrintItem(const cJSON* item) {
    char* str = cJSON_Print(item);
    if (str) {
        printf("%s\n", str);
        free(str);
    }
}

int DeleteFilteredDocuments(cJSON* collection, const char* key, const char* value, const Condition condition) {
    if (!collection || !cJSON_IsArray(collection)) {
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
                match = IsRelated( field->valuedouble, atof(value), condition);
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

int UpdateFilteredDocuments(cJSON *collection, const char *key, const char *value, const Condition condition, const Action action, const char *data) {
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
                match = IsRelated(field->valuedouble, atof(value), condition);
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
                    if (!AddAction(item, data)) return -1;
                    break;
                case drop:
                    if (!DropAction(item, data)) return -1;
                    break;
                case alter:
                    if (!AlterAction(item, data)) return -1;
                    break;
                default:
                    fprintf(stderr, "Invalid action.\n");
                    return -1;
                    break;
            }
        }
    }
    return updatedCount;
}


bool AddAction(cJSON* item, const char* param) {
    cJSON* temp = cJSON_Parse(param);
    if (!temp || !cJSON_IsObject(temp)) {
        fprintf(stderr, "Invalid add param: %s\n", param);
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


bool DropAction(cJSON* item, const char* param) {
    cJSON* temp = cJSON_Parse(param);
    if (!temp || !cJSON_IsString(temp)) {
        fprintf(stderr, "Invalid drop param: %s\n", param);
        if (temp) cJSON_Delete(temp);
        return false;;
    }

    cJSON_DeleteItemFromObject(item, temp->valuestring);
    cJSON_Delete(temp);
    return true;
}


bool AlterAction(cJSON* item, const char* param) {
    cJSON* temp = cJSON_Parse(param);
    if (!temp || !cJSON_IsObject(temp)) {
        fprintf(stderr, "Invalid alter param: %s\n", param);
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
        fprintf(stderr, "Unsupported value type in data.\n");
    }

    if (match) {
        cJSON_ReplaceItemInObject(item, value->string, new_value);
    } else {
        cJSON_Delete(new_value);
    }

    cJSON_Delete(temp);
    return match;
}



bool IsRelated(const double value1, const double value2, const Condition condition) {
    switch (condition) {
        case greaterThan:           return value1 > value2;
        case greaterThanEqual:      return value1 >= value2;
        case lessThan:              return value1 < value2;
        case lessThanEqual:         return value1 <= value2;
        case equal:                 return value1 == value2;
        default: return false;
    }
}


void GetCollectionFile(char* array, const char* databaseName, const char* collectionName, const int size) {
    snprintf(array, size, "%s/%s/%s.col", DATABASE, databaseName, collectionName);
}

void GetCollectionsMeta(char* array, const char* databaseName, const int size) {
    snprintf(array, size, "%s/%s/%s", DATABASE, databaseName, COLLECTION_META);
}

void GetDatabaseDirectory(char* array, const char* databaseName, const int size) {
    snprintf(array, size, "%s/%s", DATABASE, databaseName);
}


