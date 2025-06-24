#include <direct.h>
#include <stdio.h>
#include <stdlib.h>
#include "StorageEngine.h"
#include "DatabaseUtils.h"

static char path[MAX_PATH_LEN];
static char metaFile[MAX_PATH_LEN];
static char databaseMeta[MAX_PATH_LEN];
static char message[MAX_MESSAGE_LEN];
static char error[MAX_ERROR_LEN];

__declspec(dllexport) const char* create_database(const char* databaseName) {

    if (strlen(databaseName) + 4 >= sizeof(path)) return "warning: Database name too long";

    if (check_database(databaseName)) {
        get_message(message,"warning: Database '%s' already exists",databaseName);
        return message;
    }

    get_database_dir(path, databaseName);
    get_database_meta(databaseMeta);
    if (_mkdir(path) != 0 || !append_entry(databaseMeta, databaseName, path, database, error)) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Failed to create database \n%s", error);
        return message;
    }

    get_message(message, "Database '%s' created", databaseName);
    return message;

}

__declspec(dllexport) const char* drop_database(const char* databaseName) {
    if (!check_database(databaseName)) {
        get_message(message,"fatal: Database '%s' doesn't exists", databaseName);
        return message;
    }

    get_database_dir(path, databaseName);
    get_database_meta(databaseMeta);
    delete_dir_content(path);

    if (_rmdir(path) != 0 || !remove_entry(databaseMeta, databaseName, database, error)) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Failed to drop database \n%s", error);
        return message;
    }
    get_message(message,"Database '%s' dropped", databaseName);
    return message;
}

__declspec(dllexport) const char** list_database(int* count) {
    get_database_meta(metaFile);
    char** list = NULL;
    *count = load_list(metaFile, &list);
    return list;
}



__declspec(dllexport) const char* create_collection(const char* databaseName, const char* collectionName) {

    if (!check_database(databaseName)) {
        get_message(message,"fatal: Database '%s' does not exist", databaseName);
        return message;
    }

    if (strlen(collectionName) + strlen(databaseName) + 8 >= sizeof(path)) {
        snprintf(message,MAX_MESSAGE_LEN,"warning: Collection name too long");
        return message;
    }

    get_col_meta(metaFile, databaseName);
    get_col_file(path, databaseName, collectionName);

    if (!append_entry(metaFile, collectionName, path, collection, error)) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Collection could not be created\n%s", error);
        return message;
    }


    cJSON* data = cJSON_CreateArray();
    if (!data || !dump_binary(path, data, error)) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Collection could not be created\n%s", error);
    } else {
        get_message(message,"Collection '%s' created", collectionName);
    }

    cJSON_Delete(data);
    return message;
}

__declspec(dllexport) const char* drop_collection(const char* databaseName, const char* collectionName) {
    if (!check_database(databaseName)) {
        get_message(message,"fatal: Database '%s' doesn't exist\n", databaseName);
        return message;
    }
    get_col_meta(metaFile, databaseName);
    if (remove_entry(metaFile, collectionName, collection, error)) {
        get_col_file(path, databaseName, collectionName);
        remove(path);
        snprintf(message, MAX_MESSAGE_LEN, "Collection '%s' dropped", collectionName);
    } else {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Could not delete collection '%s'\n %s", collectionName, error);
    }

    return message;
}

__declspec(dllexport) const char** list_collection(const char* databaseName, int* count) {
    get_col_meta(path, databaseName);
    char** list = NULL;
    *count = load_list(path, &list);
    return list;
}

__declspec(dllexport) const char* insert_document(const char* databaseName, const char* collectionName, const char* document) {
    get_col_file(path, databaseName, collectionName);

    cJSON* root = load_binary(path, error);
    if (!root) {
        create_collection(databaseName, collectionName);
        root = cJSON_CreateArray();
    }

    cJSON* parsedDocument = cJSON_Parse(document);
    if (!parsedDocument) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Failed to parse document \n%s", error);
        cJSON_Delete(root);
        return message;
    }

    int insertedCount = 0;

    if (cJSON_IsArray(parsedDocument)) {
        cJSON* item = NULL;
        cJSON_ArrayForEach(item, parsedDocument) {
            cJSON* copy = cJSON_Duplicate(item, 1);
            if (copy) {
                cJSON_AddItemToArray(root, copy);
                insertedCount++;
            }
        }
    } else if (cJSON_IsObject(parsedDocument)) {
        cJSON* copy = cJSON_Duplicate(parsedDocument, 1);
        if (copy) {
            cJSON_AddItemToArray(root, copy);
            insertedCount++;
        }
    } else {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Document must be a JSON object or array of objects\n%s", error);
        return  message;
    }

    if (!dump_binary(path, root, error)) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Failed to insert document \n%s", error);
        return message;
    }

    snprintf(message, MAX_MESSAGE_LEN, "Inserted %d", insertedCount);
    cJSON_Delete(parsedDocument);
    cJSON_Delete(root);
    return message;
}


__declspec(dllexport) const char** print_all_documents(const char* databaseName, const char* collectionName, char* message, int* count) {
    return print_documents(databaseName, collectionName, NULL, NULL, all, message, count);
}


__declspec(dllexport) const char** print_documents(const char* databaseName, const char* collectionName, const char* key, const char* value, const Condition condition, char* message, int* count) {
    get_col_file(path, databaseName, collectionName);
    cJSON* collection = load_binary(path, error);
    if (!collection) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Collection file for '%s' not found or empty\n%s", databaseName, error);
        return NULL;
    }
    if (!cJSON_IsArray(collection)) {
        snprintf(message, MAX_MESSAGE_LEN,"fatal: Malformed array in collection '%s'\n%s", databaseName, error);
        cJSON_Delete(collection);
        return NULL;
    }
    char** list = NULL;
    *count = print_filtered_documents(collection, key, value, condition, &list, error);
    cJSON_Delete(collection);
    return list;
}

__declspec(dllexport) const char* remove_documents(const char* databaseName, const char* collectionName, const char* key,
        const char* value, const Condition condition) {

    get_col_file(path, databaseName, collectionName);
    cJSON* collection = load_binary(path, error);
    if (!collection || !cJSON_IsArray(collection)) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Collection file for '%s' not found or empty\n%s", databaseName, error);
        if (collection) cJSON_Delete(collection);
        return message;
    }

    const int deletedCount = remove_filtered_documents(collection, key, value, condition, error);

    if (deletedCount > 0) {
        if (!dump_binary(path, collection, error)) {
            snprintf(message, MAX_MESSAGE_LEN, "fatal: Failed to delete document\n%s", error);
        } else {
            snprintf(message, MAX_MESSAGE_LEN, "Document removed %d", deletedCount);
        }
    } else if (deletedCount < 0) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Failed to delete document\n%s", error);
    } else {
        snprintf(message, MAX_MESSAGE_LEN, "No document found for given condition");
    }

    cJSON_Delete(collection);
    return message;
}

__declspec(dllexport) const char* remove_all_documents(const char* databaseName, const char* collectionName) {
    return remove_documents(databaseName, collectionName, NULL, NULL, all);
}

__declspec(dllexport) const char* update_documents(const char* databaseName, const char* collectionName,
        const char* key, const char* value, const Condition condition, const Action action, const char* data) {

    get_col_file(path, databaseName, collectionName);
    cJSON* collection = load_binary(path, error);
    if (!collection || !cJSON_IsArray(collection)) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Collection file for '%s' not found or empty\n%s", databaseName, error);
        if (collection) cJSON_Delete(collection);
        return message;
    }

    const int updatedCount = update_filtered_documents(collection, key, value, condition, action, data, error);

    if (updatedCount > 0) {
        if (!dump_binary(path, collection, error)) {
            snprintf(message, MAX_MESSAGE_LEN, "fatal: Failed to update document\n%s", error);
            return message;
        }
        snprintf(message, MAX_MESSAGE_LEN, "Document updated %d", updatedCount);
    } else if (updatedCount < 0) {
        snprintf(message, MAX_MESSAGE_LEN, "fatal: Failed to update document\n%s", error);
    } else {
        snprintf(message, MAX_MESSAGE_LEN, "No document found for given condition");
    }

    cJSON_Delete(collection);
    return message;
}

__declspec(dllexport) const char* update_all_documents(const char* databaseName, const char* collectionName, const Action action, const char* data) {
    return update_documents(databaseName, collectionName, NULL, NULL, all, action, data);
}

__declspec(dllexport) void free_list(char** list) {
    free(list);
}

__declspec(dllexport) void free_count(int* count) {
    free(count);
}




