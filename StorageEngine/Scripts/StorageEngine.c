#include <direct.h>
#include <stdio.h>
#include <stdlib.h>
#include "StorageEngine.h"

static char filePath[MAX_PATH_LEN];
static char metaFile[MAX_PATH_LEN];
static char databaseMeta[MAX_PATH_LEN];
static char error[MAX_ERROR_LEN];

export Output create_database(const QueryConfig config) {
    Output output = NEW_OUTPUT;

    if (strlen(config.databaseName) + 4 >= sizeof(filePath)) {
        get_message(output.message, "warning: Database name too long");
        return output;
    }
    get_database_meta(databaseMeta);
    if (check_database(config.databaseName)) {
        get_message(output.message,"warning: Database '%s' already exists",config.databaseName);
        return output;
    }

    get_database_dir(filePath, config.databaseName);

    if (_mkdir(filePath) != 0 || !append_entry(databaseMeta, config.databaseName, filePath, database, error)) {
        get_message(output.message,"fatal: Failed to create database \n%s", error);
        return output;
    }

    output.success = true;
    get_message(output.message, "Database '%s' created", config.databaseName);
    printf("%s\n", output.message);
    return output;
}

export Output drop_database(const QueryConfig config) {
    Output output = NEW_OUTPUT;

    if (!check_database(config.databaseName)) {
        get_message(output.message,"fatal: Database '%s' doesn't exists", config.databaseName);
        return output;
    }

    get_database_dir(filePath, config.databaseName);
    get_database_meta(databaseMeta);
    delete_dir_content(filePath);

    if (_rmdir(filePath) != 0 || !remove_entry(databaseMeta, config.databaseName, database, error)) {
        get_message(output.message, "fatal: Failed to drop database \n%s", error);
        return output;
    }

    output.success = true;
    get_message(output.message,"Database '%s' dropped", config.databaseName);
    return output;
}

export ArrayOut list_database() {
    ArrayOut arrayOut = NEW_ARRAY_OUT;
    get_database_meta(metaFile);
    char** _list = NULL;
    arrayOut.size = load_list(metaFile, &_list, error);
    if (arrayOut.size < 0) {
        get_message(arrayOut.message,"fatal: Failed to load database \n%s", error);
    }
    arrayOut.list = _list;
    return arrayOut;
}

export Output create_collection(const QueryConfig config) {
    Output output = NEW_OUTPUT;

    if (!check_database(config.databaseName)) {
        get_message(output.message,"fatal: Database '%s' does not exist", config.databaseName);
        return output;
    }

    if (strlen(config.collectionName) + strlen(config.databaseName) + 8 >= sizeof(filePath)) {
        get_message(output.message,"warning: Collection name too long");
        return output;
    }

    get_col_meta(metaFile, config.databaseName);
    get_col_file(filePath, config.databaseName, config.collectionName);

    if (!append_entry(metaFile, config.collectionName, filePath, collection, error)) {
        get_message(output.message, "fatal: Collection could not be created\n%s", error);
        return output;
    }


    cJSON* _data = cJSON_CreateArray();
    if (!_data || !dump_binary(filePath, _data, error)) {
        get_message(output.message, "fatal: Collection could not be created\n%s", error);
    } else {
        get_message(output.message,"Collection '%s' created", config.collectionName);
        output.success = true;
    }

    cJSON_Delete(_data);
    return output;
}

export Output drop_collection(const QueryConfig config) {
    Output output = NEW_OUTPUT;

    if (!check_database(config.databaseName)) {
        get_message(output.message,"fatal: Database '%s' doesn't exist\n", config.databaseName);
        return output;
    }
    get_col_meta(metaFile, config.databaseName);
    if (remove_entry(metaFile, config.collectionName, collection, error)) {
        get_col_file(filePath, config.databaseName, config.collectionName);
        remove(filePath);
        get_message(output.message, "Collection '%s' dropped", config.collectionName);
        output.success = true;
    } else {
        get_message(output.message, "fatal: Could not delete collection '%s'\n %s", config.collectionName, error);
    }

    return output;
}

export ArrayOut list_collection(const QueryConfig config) {
    ArrayOut arrayOut = NEW_ARRAY_OUT;

    get_col_meta(filePath, config.databaseName);
    char** _list = NULL;
    arrayOut.size = load_list(filePath, &_list, error);
    if (arrayOut.size < 0) {
        get_message(arrayOut.message,"fatal: Failed to load collection \n%s", error);
    }
    arrayOut.list = _list;
    return arrayOut;
}

export Output insert_document(const QueryConfig config) {
    Output output = NEW_OUTPUT;

    get_col_file(filePath, config.databaseName, config.collectionName);

    cJSON* _root = load_binary(filePath, error);
    if (!_root) {
        create_collection(config);
        _root = cJSON_CreateArray();
    }

    cJSON* _parsedDocument = cJSON_Parse(config.data);
    if (!_parsedDocument) {
        get_message(output.message, "fatal: Failed to parse document \n%s", error);
        cJSON_Delete(_root);
        return output;
    }

    int _insertedCount = 0;

    if (cJSON_IsArray(_parsedDocument)) {
        cJSON* _item = NULL;
        cJSON_ArrayForEach(_item, _parsedDocument) {
            cJSON* _copy = cJSON_Duplicate(_item, 1);
            if (_copy) {
                cJSON_AddItemToArray(_root, _copy);
                _insertedCount++;
            }
        }
    } else if (cJSON_IsObject(_parsedDocument)) {
        cJSON* _copy = cJSON_Duplicate(_parsedDocument, 1);
        if (_copy) {
            cJSON_AddItemToArray(_root, _copy);
            _insertedCount++;
        }
    } else {
        get_message(output.message, "fatal: Document must be a JSON object or array of objects\n%s", error);
        return output;
    }

    if (!dump_binary(filePath, _root, error)) {
        get_message(output.message, "fatal: Failed to insert document \n%s", error);
        return output;
    }

    output.success = true;
    get_message(output.message, "Inserted %d", _insertedCount);
    cJSON_Delete(_parsedDocument);
    cJSON_Delete(_root);
    return output;
}


export ArrayOut print_all_documents(const QueryConfig config) {
    return print_documents(config);
}


export ArrayOut print_documents(const QueryConfig config) {
    ArrayOut arrayOut = NEW_ARRAY_OUT;
    get_col_file(filePath, config.databaseName, config.collectionName);
    cJSON* _collection = load_binary(filePath, error);

    if (!_collection) {
        get_message(arrayOut.message, "fatal: Collection file for '%s' not found or empty\n%s", config.databaseName, error);
        arrayOut.size = -1;
        return arrayOut;
    }

    if (!cJSON_IsArray(_collection)) {
        get_message(arrayOut.message,"fatal: Malformed array in collection '%s'\n%s", config.databaseName, error);
        arrayOut.size = -1;
        cJSON_Delete(_collection);
        return arrayOut;
    }

    char** _list = NULL;
    arrayOut.size = print_filtered_documents(_collection, config.key, config.value, config.condition, &_list, error);
    if (arrayOut.size < 0) {
        get_message(arrayOut.message,"fatal: Failed to print document \n%s", error);
    } else if (arrayOut.size == 0) {
        get_message(arrayOut.message, "fatal: Collection '%s' contains no documents\n%s", config.collectionName, error);
    } else {
        arrayOut.list = _list;
    }
    cJSON_Delete(_collection);
    return arrayOut;
}

export Output remove_documents(const QueryConfig config) {
    Output output = NEW_OUTPUT;
    get_col_file(filePath, config.databaseName, config.collectionName);
    cJSON* _collection = load_binary(filePath, error);
    if (!_collection || !cJSON_IsArray(_collection)) {
        get_message(output.message, "fatal: Collection file for '%s' not found or empty\n%s", config.databaseName, error);
        if (_collection) cJSON_Delete(_collection);
        return output;
    }

    const int _deletedCount = remove_filtered_documents(_collection, config.key, config.value, config.condition, error);
    if (_deletedCount > 0 && dump_binary(filePath, _collection, error)) {
        get_message(output.message, "Document removed %d", _deletedCount);
        output.success = true;
    } else if (_deletedCount > 0) {
        get_message(output.message, "fatal: Failed to delete document\n%s", error);
    }else {
        get_message(output.message, "No document found for specified condition");
    }
    cJSON_Delete(_collection);
    return output;
}

export Output remove_all_documents(const QueryConfig config) {
    return remove_documents(config);
}

export Output update_documents(const QueryConfig config) {
    Output output = NEW_OUTPUT;


    if (!config.databaseName || !config.collectionName || !config.data) {
        get_message(output.message,"fatal: Missing required query parameters");
        return output;
    }

    get_col_file(filePath, config.databaseName, config.collectionName);
    cJSON* _collection = load_binary(filePath, error);
    if (!_collection || !cJSON_IsArray(_collection)) {
        get_message(output.message,
                 "fatal: Collection file for '%s' not found or invalid\n%s", config.databaseName, error);
        if (_collection) cJSON_Delete(_collection);
        return output;
    }

    const int _count = update_filtered_documents(_collection, config.key, config.value,
                                          config.condition, config.action, config.data, error);

    if (_count > 0) {
        if (!dump_binary(filePath, _collection, error)) {
            get_message(output.message, "fatal: Failed to save updated documents\n%s", error);
            cJSON_Delete(_collection);
            return output;
        }
        get_message(output.message, "Document updated %d", _count);
        output.success = true;
    } else if (_count < 0) {
        get_message(output.message, "fatal: Failed to update document\n%s", error);
    } else {
        get_message(output.message, "No document found for given condition");
    }

    cJSON_Delete(_collection);
    return output;
}


export Output update_all_documents(const QueryConfig config) {
    return update_documents(config);
}

export void free_list(char** list, const int size) {
    for (int i = 0; i < size; i++) {
        free(list[i]);
    }
    free(list);
}


