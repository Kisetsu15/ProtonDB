#include <stdlib.h>
#include <string.h>
#include <io.h>
#include <stdio.h>
#include <stdbool.h>
#include "DatabaseUtils.h"

#include <stdarg.h>

#include "cJSON.h"
char** names;
char** document;

bool check_database(const char* databaseName) {
    bool _exist = false;
    char _metaPath[MAX_PATH_LEN];
    get_database_meta(_metaPath);
    cJSON* _databaseMeta = load_json(_metaPath);
    if (cJSON_HasObjectItem(_databaseMeta, databaseName)) _exist = true;

    cJSON_Delete(_databaseMeta);
    return _exist;
}

void delete_dir_content(const char* directory) {
    char _searchPath[MAX_PATH_LEN];
    snprintf(_searchPath, sizeof(_searchPath), "%s\\*.*", directory);

    struct _finddata_t _file;
    const intptr_t _handle = _findfirst(_searchPath, &_file);
    if (_handle == -1) return;

    do {
        if (strcmp(_file.name, ".") != 0 && strcmp(_file.name, "..") != 0) {
            char fullPath[MAX_PATH_LEN];
            snprintf(fullPath, sizeof(fullPath), "%s\\%s", directory, _file.name);
            remove(fullPath);
        }
    } while (_findnext(_handle, &_file) == 0);

    _findclose(_handle);
}

bool remove_entry(const char* metaFile, const char* name, const FileType fileType, char* error) {
    const char* fileString = file_type_string(fileType);
    bool _status = false;
    cJSON* _metaConfig = load_json(metaFile);

    if (!_metaConfig || !cJSON_IsObject(_metaConfig)) {
        get_error(error, "fatal: Could not load or parse meta file '%s'", metaFile);
        if (_metaConfig) cJSON_Delete(_metaConfig);
        return false;
    }

    if (cJSON_HasObjectItem(_metaConfig, name)) {
        cJSON_DeleteItemFromObject(_metaConfig, name);
        _status = save_json(metaFile, _metaConfig, error);
    } else {
        get_error(error, "fatal: %s entry '%s' not found", fileString, name);
    }

    cJSON_Delete(_metaConfig);
    return _status;
}

bool append_entry(const char* metaFile, const char* name, const char* path, const FileType fileType, char* error) {
    const char* fileString = file_type_string(fileType);
    bool _status = false;
    cJSON* _meta = load_json(metaFile);
    if (!_meta) _meta = cJSON_CreateObject();

    if (!cJSON_HasObjectItem(_meta, name)) {
        cJSON_AddStringToObject(_meta, name, path);
        _status = save_json(metaFile, _meta, error);
    } else {
        get_error(error, "warning: %s '%s' already exists", fileString, name);
    }

    cJSON_Delete(_meta);
    return _status;
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
        get_error(error, "fatal: Invalid JSON object");
        return false;
    }

    char* _dataString = cJSON_PrintUnformatted(data);

    if (!_dataString) {
        get_error(error, "fatal: Failed to convert JSON to string");
        return false;
    }

    FILE* _file = fopen(fileName, "wb");
    if (!_file) {
        get_error(error, "fatal: Could not open file '%s' for writing", fileName);
        free(_dataString);
        return false;
    }

    fwrite(_dataString, sizeof(char), strlen(_dataString), _file);
    fclose(_file);
    free(_dataString);
    return true;
}

cJSON* load_binary(const char* fileName, char* error) {
    FILE* _file = fopen(fileName, "rb");
    if (!_file) {
        return NULL;
    }

    fseek(_file, 0, SEEK_END);
    const long _len = ftell(_file);
    if (_len <= 0) {
        fclose(_file);
        get_error(error, "fatal: File '%s' is empty or unreadable", fileName);
        return NULL;
    }

    rewind(_file);
    char* _buffer = malloc(_len + 1);
    if (!_buffer) {
        fclose(_file);
        get_error(error, "fatal: Memory allocation failed for reading '%s'", fileName);
        return NULL;
    }

    fread(_buffer, 1, _len, _file);
    _buffer[_len] = '\0';
    fclose(_file);

    cJSON* _json = cJSON_Parse(_buffer);
    free(_buffer);

    if (!_json) {
        get_error(error, "fatal: Failed to parse JSON from binary");
        return NULL;
    }

    return _json;
}


cJSON* load_json(const char* file_name) {
    FILE* _file = fopen(file_name, "r");
    if (!_file) return NULL;

    fseek(_file, 0, SEEK_END);
    const long _len = ftell(_file);
    rewind(_file);

    char* _data = malloc(_len + 1);
    fread(_data, 1, _len, _file);
    _data[_len] = '\0';
    fclose(_file);

    cJSON* _dict = cJSON_Parse(_data);
    free(_data);

    if (!_dict || !cJSON_IsObject(_dict)) {
        if (_dict) cJSON_Delete(_dict);
        return NULL;
    }

    return _dict;
}

bool save_json(const char* filename, cJSON* config, char* error) {
    if (!config || !cJSON_IsObject(config)) {
        get_error(error, "fatal: Invalid JSON object passed");
        return false;
    }

    char* _jsonString = cJSON_PrintUnformatted(config);
    if (!_jsonString) {
        get_error(error, "fatal: Failed to convert JSON object to string");
        return false;
    }

    FILE* _file = fopen(filename, "w");
    if (!_file) {
        get_error(error, "fatal: Error opening file for writing");
        free(_jsonString);
        return false;
    }

    fputs(_jsonString, _file);
    fclose(_file);
    free(_jsonString);
    return true;
}

int print_filtered_documents(cJSON* collection, const char* key, const char* value, const Condition condition, char*** list, char* error) {
    if (condition > all) {
        get_error(error, "fatal: Invalid condition specified");
        *list = NULL;
        return -1;
    }

    if (!collection || !cJSON_IsArray(collection)) {
        get_error(error, "fatal: Not a valid array format");
        *list = NULL;
        return -1;
    }

    const cJSON* _item = NULL;
    const int _size = cJSON_GetArraySize(collection);
    if (_size == 0) *list = NULL;
    document = malloc(_size * sizeof(char*));
    int _index = 0;
    cJSON_ArrayForEach(_item, collection) {
        if (condition == all || key == NULL || value == NULL) {
            print_item(document, _index, _item);
        } else {
            bool _match = false;
            cJSON* _field = cJSON_GetObjectItem(_item, key);
            if (!_field) continue;
            const bool isNumber = cJSON_IsNumber(_field);
            if (!isNumber && condition != equal) continue;
            if (isNumber) {
                _match = is_related(_field->valuedouble, atof(value), condition);
            } else if (cJSON_IsString(_field)) {
                _match = strcmp(_field->valuestring, value) == 0;
            } else if (cJSON_IsBool(_field)) {
                _match = (strcmp(value, "true") == 0 && _field->valueint == 1) ||
                        (strcmp(value, "false") == 0 && _field->valueint == 0);
            }

            if (_match) {
                print_item(document, _index, _item);
            }
         }
        _index++;
    }
    cJSON_Delete(collection);
    *list = document;
    return _index;
}

void print_item(char** document, const int index, const cJSON* item) {
    char* str = cJSON_Print(item);
    if (str && document != NULL) {
        document[index] = _strdup(str);
        free(str);
    }
}

int load_list(const char* metaFile, char*** list, char* error) {
    cJSON* _meta = load_json(metaFile);
    if (!_meta || !cJSON_IsObject(_meta)) {
        if (_meta) cJSON_Delete(_meta);
        get_error(error, "fatal: Invalid JSON object passed");
        *list = NULL;
        return -1;
    }

    int _count = 0;
    cJSON* _item = NULL;
    cJSON_ArrayForEach(_item, _meta) _count++;
    names = malloc(_count * sizeof(char*));

    int index = 0;
    _item = NULL;

    cJSON_ArrayForEach(_item, _meta) {
        if (!_item->string) continue;
        names[index] = _strdup(_item->string);
        index++;
    }

    cJSON_Delete(_meta);
    *list = names;
    return _count;
}



int remove_filtered_documents(cJSON* collection, const char* key, const char* value, const Condition condition, char* error) {
    if (!collection || !cJSON_IsArray(collection)) {
        get_error(error, "fatal: Not a valid array format");
        return -1;
    }
    const bool _filterEnabled = !(condition == all || key == NULL || value == NULL);
    const int _size = cJSON_GetArraySize(collection);
    int _deletedCount = 0;

    for (int i = _size - 1; i >= 0; i--) {
        bool _match = false;
        if (_filterEnabled) {
            cJSON* _item = cJSON_GetArrayItem(collection, i);
            cJSON* _field = cJSON_GetObjectItem(_item, key);
            if (!_field) continue;
            const bool _isNumber = cJSON_IsNumber(_field);

            // Only apply condition logic to numeric fields
            if (!_isNumber && condition != equal) continue;
            if (_isNumber) {
                _match = is_related( _field->valuedouble, atof(value), condition);
            } else if (cJSON_IsString(_field)) {
                _match = strcmp(_field->valuestring, value) == 0;
            } else if (cJSON_IsBool(_field)) {
                _match = ((strcmp(value, "true") == 0 && _field->valueint == 1) ||
                        (strcmp(value, "false") == 0 && _field->valueint == 0));
            }
        } else {
            _match = true;
        }

        if (_match) {
            cJSON_DeleteItemFromArray(collection, i);
            _deletedCount++;
        }
    }

    return _deletedCount;
}

int update_filtered_documents(cJSON *collection, const char *key, const char* value, const Condition condition, const Action action, const char *data, char* error) {
    if (!collection || !cJSON_IsArray(collection)) return -1;

    const bool _filterEnabled = !(condition == all || key == NULL || value == NULL);
    const int _size = cJSON_GetArraySize(collection);
    int _updatedCount = 0;

    for (int i = 0; i < _size; i++) {
        cJSON* _item = cJSON_GetArrayItem(collection, i);
        bool _match = false;

        if (_filterEnabled) {
            cJSON* _field = cJSON_GetObjectItem(_item, key);
            if (!_field) continue;

            if (cJSON_IsNumber(_field)) {
                _match = is_related(_field->valuedouble, atof(value), condition);
            } else if (cJSON_IsString(_field)) {
                _match = strcmp(_field->valuestring, value) == 0;
            } else if (cJSON_IsBool(_field)) {
                _match = ((strcmp(value, "true") == 0 && _field->valueint == 1) ||
                         (strcmp(value, "false") == 0 && _field->valueint == 0));
            } else {
                continue;
            }
        } else {
            _match = true;
        }

        if (_match) {
            _updatedCount++;
            switch (action) {
                case add:
                    if (!add_action(_item, data, error)) return -1;
                    break;
                case drop:
                    if (!drop_action(_item, data, error)) return -1;
                    break;
                case alter:
                    if (!alter_action(_item, data, error)) return -1;
                    break;
                default:
                    get_error(error, "fatal: Invalid action specified");
                    return -1;
            }
        }
    }
    return _updatedCount;
}


bool add_action(cJSON* item, const char* data, char* error) {
    cJSON* _temp = cJSON_Parse(data);
    if (!_temp || !cJSON_IsObject(_temp)) {
        get_error(error, "fatal: Invalid data format '%s'", data);
        if (_temp) cJSON_Delete(_temp);
        return false;
    }

    cJSON* _field = NULL;
    cJSON_ArrayForEach(_field, _temp) {
        cJSON* copy = cJSON_Duplicate(_field, 1);
        cJSON_AddItemToObject(item, _field->string, copy);
    }

    cJSON_Delete(_temp);
    return true;
}


bool drop_action(cJSON* item, const char* data, char* error) {
    cJSON* _temp = cJSON_Parse(data);
    if (!_temp || !cJSON_IsString(_temp)) {
        get_error(error, "fatal: Invalid data format '%s'", data);
        if (_temp) cJSON_Delete(_temp);
        return false;;
    }

    cJSON_DeleteItemFromObject(item, _temp->valuestring);
    cJSON_Delete(_temp);
    return true;
}


bool alter_action(cJSON* item, const char* data, char* error) {
    cJSON* _temp = cJSON_Parse(data);
    if (!_temp || !cJSON_IsObject(_temp)) {
        get_error(error, "fatal: Invalid data format '%s'", data);
        if (_temp) cJSON_Delete(_temp);
        return false;
    }

    cJSON* _value = _temp->child;
    if (!_value || !_value->string) {
        cJSON_Delete(_temp);
        return false;
    }

    bool _match = false;
    cJSON* _newValue = NULL;

    if (cJSON_IsString(_value)) {
        _newValue = cJSON_CreateString(_value->valuestring);
        _match = true;
    } else if (cJSON_IsNumber(_value)) {
        _newValue = cJSON_CreateNumber(_value->valuedouble);
        _match = true;
    } else if (cJSON_IsBool(_value)) {
        _newValue = cJSON_CreateBool(_value->valueint);
        _match = true;
    } else if (cJSON_IsArray(_value)) {
        _newValue = cJSON_Duplicate(_value, 1);
        _match = true;
    } else if (cJSON_IsObject(_value)) {
        _newValue = cJSON_Duplicate(_value, 1);
        _match = true;
    } else {
        get_error(error, "fatal: Unsupported value type in data");
    }

    if (_match) {
        cJSON_ReplaceItemInObject(item, _value->string, _newValue);
    } else {
        cJSON_Delete(_newValue);
    }

    cJSON_Delete(_temp);
    return _match;
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
    char* _env = getenv("APPDATA");
    snprintf(array, MAX_PATH_LEN, "%s/%s/%s/%s/%s.col", _env, PROTON_DB, DB, databaseName, collectionName);
}

void get_col_meta(char* array, const char* databaseName) {
    char* _env = getenv("APPDATA");
    snprintf(array, MAX_PATH_LEN, "%s/%s/%s/%s/%s", _env, PROTON_DB, DB, databaseName, COLLECTION_META);
}

void get_database_meta(char* array) {
    char* _env = getenv("APPDATA");
    snprintf(array, MAX_PATH_LEN, "%s/%s/%s", _env, PROTON_DB, DATABASE_META);
}

void get_database_dir(char* array, const char* databaseName) {
    char* _env = getenv("APPDATA");
    snprintf(array, MAX_PATH_LEN, "%s/%s/%s/%s", _env, PROTON_DB, DB, databaseName);
}

void get_message(char* buffer, const char* format, ...) {
    va_list args;
    va_start(args, format);
    vsnprintf(buffer, MAX_MESSAGE_LEN, format, args);
    va_end(args);
}

void get_error(char* buffer, const char* format, ...) {
    va_list args;
    va_start(args, format);
    vsnprintf(buffer, MAX_ERROR_LEN, format, args);
    va_end(args);
}


