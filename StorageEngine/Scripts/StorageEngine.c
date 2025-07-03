#include <direct.h>
#include <stdio.h>
#include <stdlib.h>
#include "StorageEngine.h"

// Global path buffers used across operations
static char filePath[MAX_PATH_LEN];
static char metaFile[MAX_PATH_LEN];
static char databaseMeta[MAX_PATH_LEN];
static char error[MAX_ERROR_LEN];

/// @brief Creates a new database directory and registers it in the metadata.
/// @param config QueryConfig containing databaseName
/// @return Output with success flag and status message
export Output create_database(const QueryConfig config) {
    Output output = NEW_OUTPUT;

    // Check if database name is too long
    if (strlen(config.databaseName) + 4 >= sizeof(filePath)) {
        get_message(output.message, "warning: Database name too long");
        return output;
    }

    get_database_meta(databaseMeta);

    // Check if database already exists
    if (check_database(config.databaseName)) {
        get_message(output.message,"warning: Database '%s' already exists",config.databaseName);
        return output;
    }

    get_database_dir(filePath, config.databaseName);

    // Attempt to create directory and register it
    if (_mkdir(filePath) != 0 || !append_entry(databaseMeta, config.databaseName, filePath, database, error)) {
        get_message(output.message,"fatal: Failed to create database \n%s", error);
        return output;
    }

    output.success = true;
    get_message(output.message, "Database '%s' created", config.databaseName);
    return output;
}

/// @brief Deletes a database and its metadata entry.
/// @param config QueryConfig with databaseName
/// @return Output with success flag and message
export Output drop_database(const QueryConfig config) {
    Output output = NEW_OUTPUT;

    if (!check_database(config.databaseName)) {
        get_message(output.message,"fatal: Database '%s' doesn't exists", config.databaseName);
        return output;
    }

    get_database_dir(filePath, config.databaseName);
    get_database_meta(databaseMeta);

    // Delete all files in database and remove the directory
    delete_dir_content(filePath);

    if (_rmdir(filePath) != 0 || !remove_entry(databaseMeta, config.databaseName, database, error)) {
        get_message(output.message, "fatal: Failed to drop database \n%s", error);
        return output;
    }

    output.success = true;
    get_message(output.message,"Database '%s' dropped", config.databaseName);
    return output;
}

/// @brief Lists all existing databases from metadata.
/// @return ArrayOut with list of databases or error
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

/// @brief Creates a new collection in a database.
/// @param config QueryConfig with databaseName and collectionName
/// @return Output with success flag and status message
export Output create_collection(const QueryConfig config) {
    Output output = NEW_OUTPUT;

    if (!check_database(config.databaseName)) {
        get_message(output.message,"fatal: Database '%s' does not exist", config.databaseName);
        return output;
    }

    // Check name length to avoid path overflow
    if (strlen(config.collectionName) + strlen(config.databaseName) + 8 >= sizeof(filePath)) {
        get_message(output.message,"warning: Collection name too long");
        return output;
    }

    get_col_meta(metaFile, config.databaseName);
    get_col_file(filePath, config.databaseName, config.collectionName);

    // Append collection entry to metadata
    if (!append_entry(metaFile, config.collectionName, filePath, collection, error)) {
        get_message(output.message, "fatal: Collection could not be created\n%s", error);
        return output;
    }

    // Create empty JSON array and dump to file
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

/// @brief Drops a collection from a database.
/// @param config QueryConfig with databaseName and collectionName
/// @return Output with success flag and message
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

/// @brief Lists all collections in a given database.
/// @param config QueryConfig with databaseName
/// @return ArrayOut with collection names or error
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

/// @brief Inserts one or more JSON documents into a collection.
/// @param config QueryConfig with databaseName, collectionName, and data (JSON string)
/// @return Output with success flag and message
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

    // Insert based on whether input is array or object
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

/// @brief Prints all documents in a collection.
/// @param config QueryConfig with databaseName and collectionName
/// @return ArrayOut with stringified documents or error
export ArrayOut print_all_documents(const QueryConfig config) {
    return print_documents(config);
}

/// @brief Prints documents that match a filter condition.
/// @param config QueryConfig with filter params
/// @return ArrayOut with matching documents
export ArrayOut print_documents(const QueryConfig config) {
    ArrayOut arrayOut = NEW_ARRAY_OUT;
    get_col_file(filePath, config.databaseName, config.collectionName);

    cJSON* _collection = load_binary(filePath, error);
    if (!_collection) {
        get_message(arrayOut.message, "fatal: Collection '%s' not found or empty\n%s", config.collectionName, error);
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

/// @brief Removes documents based on a filter condition.
/// @param config QueryConfig with key, value, and condition
/// @return Output with success status and removal count
export Output remove_documents(const QueryConfig config) {
    Output output = NEW_OUTPUT;
    get_col_file(filePath, config.databaseName, config.collectionName);

    cJSON* _collection = load_binary(filePath, error);
    if (!_collection || !cJSON_IsArray(_collection)) {
        get_message(output.message, "fatal: Collection '%s' not found or empty\n%s", config.collectionName, error);
        if (_collection) cJSON_Delete(_collection);
        return output;
    }

    const int _deletedCount = remove_filtered_documents(_collection, config.key, config.value, config.condition, error);
    if (_deletedCount > 0 && dump_binary(filePath, _collection, error)) {
        get_message(output.message, "Document removed %d", _deletedCount);
        output.success = true;
    } else if (_deletedCount > 0) {
        get_message(output.message, "fatal: Failed to delete document\n%s", error);
    } else {
        get_message(output.message, "No document found for specified condition");
    }

    cJSON_Delete(_collection);
    return output;
}

/// @brief Removes all documents (alias for remove_documents).
export Output remove_all_documents(const QueryConfig config) {
    return remove_documents(config);
}

/// @brief Updates documents matching a filter with given data and action.
/// @param config QueryConfig with update info
/// @return Output with update count or error
export Output update_documents(const QueryConfig config) {
    Output output = NEW_OUTPUT;

    if (!config.databaseName || !config.collectionName || !config.data) {
        get_message(output.message,"fatal: Missing required query parameters");
        return output;
    }

    get_col_file(filePath, config.databaseName, config.collectionName);

    cJSON* _collection = load_binary(filePath, error);
    if (!_collection || !cJSON_IsArray(_collection)) {
        get_message(output.message,"fatal: Collection '%s' not found or invalid\n%s", config.collectionName, error);
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

/// @brief Updates all documents (alias for update_documents).
export Output update_all_documents(const QueryConfig config) {
    return update_documents(config);
}

/// @brief Frees memory allocated to document string lists.
/// @param list char** list to free
/// @param size number of elements
export void free_list(char** list, const int size) {
    for (int i = 0; i < size; i++) {
        free(list[i]);
    }
    free(list);
}
