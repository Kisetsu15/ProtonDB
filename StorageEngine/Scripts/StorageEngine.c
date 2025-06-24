#include <direct.h>
#include <stdio.h>
#include <stdlib.h>
#include "StorageEngine.h"
#include "DatabaseUtils.h"

char path[MAX_PATH_LEN];
char metaFile[MAX_PATH_LEN];
char databaseMeta[MAX_PATH_LEN];
char message[MAX_MESSAGE_LEN];

__declspec(dllexport) const char* CreateDatabase(const char* databaseName) {

    if (strlen(databaseName) + 4 >= sizeof(path)) {
        return "warning: Database name too long";
    }

    if (CheckDatabase(databaseName)) {
        GetMessage(message,"warning: Database '%s' already exists.\n",databaseName);
        return message;
    }

    GetDatabaseDirectory(path, databaseName);
    GetDatabaseMeta(databaseMeta);
    if (_mkdir(path) != 0 || !AppendEntry(databaseMeta, databaseName, path, database)) {
        return "fatal: could not create database";
    } else {
        GetMessage(message, "Database '%s' created.\n", databaseName);
        return message;
    }
}

__declspec(dllexport) void DeleteDatabase(const char* databaseName) {
    if (!CheckDatabase(databaseName)) {
        printf("fatal: Database '%s' doesn't exists.\n", databaseName);
        return;
    }

    GetDatabaseDirectory(path, databaseName);
    GetDatabaseMeta(databaseMeta);
    DeleteDirectoryContent(path);

    if (_rmdir(path) != 0 || !RemoveEntry(databaseMeta, databaseName, database)) {
        perror("fatal: could not remove database");
    } else {
        printf("Database '%s' deleted.\n", databaseName);
    }
}

__declspec(dllexport) void ListDatabase() {
    GetDatabaseMeta(databaseMeta);
    cJSON* databaseMetaConfig = LoadJson(databaseMeta);
    if (!databaseMetaConfig || !cJSON_IsObject(databaseMetaConfig)) {
        fprintf(stderr, "fatal: Failed to load or parse database metadata.\n");
        if (databaseMetaConfig) cJSON_Delete(databaseMetaConfig);
        return;
    }

    cJSON* item = databaseMetaConfig->child;
    if (!item) {
        printf("No Database found.\n");
    }

    while (item) {
        printf("%s\n", item->string);
        item = item->next;
    }

    cJSON_Delete(databaseMetaConfig);
}

__declspec(dllexport) void CreateCollection(const char* databaseName, const char* collectionName) {
    if (!CheckDatabase(databaseName)) {
        printf("fatal: Database '%s' does not exist.\n", databaseName);
        return;
    }

    if (strlen(collectionName) + strlen(databaseName) + 8 >= sizeof(path)) {
        printf("warning: Collection name too long.\n");
        return;
    }

    GetCollectionsMeta(metaFile, databaseName);
    GetCollectionFile(path, databaseName, collectionName);

    if (!AppendEntry(metaFile, collectionName, path, collection)) return;

    cJSON* data = cJSON_CreateArray();
    if (!data || !DumpBinary(path, data)) {
        perror("fatal: Could not create Collection");
    } else {
        printf("Collection '%s' created.\n", collectionName);
    }

    cJSON_Delete(data);
}

__declspec(dllexport) void DeleteCollection(const char* databaseName, const char* collectionName) {
    if (!CheckDatabase(databaseName)) {
        printf("fatal: Database '%s' doesn't exist.\n", databaseName);
        return;
    }
    GetCollectionsMeta(metaFile, databaseName);
    if (RemoveEntry(metaFile, collectionName, collection)) {
        GetCollectionFile(path, databaseName, collectionName);
        remove(path);
        printf("Collection '%s' deleted.\n", collectionName);
    } else {
        printf("fatal: Could not delete collection '%s'\n", collectionName);
    }

}

__declspec(dllexport) void ListCollection(const char* databaseName) {
    GetCollectionsMeta(path, databaseName);

    cJSON* collectionMeta = LoadJson(path);
    if (!collectionMeta || !cJSON_IsObject(collectionMeta)) {
        fprintf(stderr, "fatal: Failed to load or parse collection metadata for '%s'\n", databaseName);
        if (collectionMeta) cJSON_Delete(collectionMeta);
        return;
    }

    cJSON* item = collectionMeta->child;
    if (!item) {
        printf("No collections found in database '%s'.\n", databaseName);
    }

    while (item) {
        printf("%s\n", item->string);
        item = item->next;
    }

    cJSON_Delete(collectionMeta);
}

__declspec(dllexport) void InsertDocument(const char* databaseName, const char* collectionName, const char* document) {
    GetCollectionFile(path, databaseName, collectionName);

    cJSON* root = LoadBinary(path);
    if (!root) {
        CreateCollection(databaseName, collectionName);
        root = cJSON_CreateArray();
    }

    cJSON* parsedDocument = cJSON_Parse(document);
    if (!parsedDocument) {
        fprintf(stderr, "fatal: Failed to parse document.\n");
        cJSON_Delete(root);
        return;
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
        fprintf(stderr, "fatal: Document must be a JSON object or array of objects.\n");
    }

    DumpBinary(path, root);
    printf("Inserted %d\n", insertedCount);

    cJSON_Delete(parsedDocument);
    cJSON_Delete(root);
}


__declspec(dllexport) void PrintAllDocuments(const char* databaseName, const char* collectionName) {
    PrintDocuments(databaseName, collectionName, NULL, NULL, all);
}


__declspec(dllexport) void PrintDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, const Condition condition) {
    GetCollectionFile(path, databaseName, collectionName);
    cJSON* collection = LoadBinary(path);
    if (!collection) {
        fprintf(stderr, "fatal: Collection file for '%s' not found or empty\n", databaseName);
        return;
    }
    if (!cJSON_IsArray(collection)) {
        fprintf(stderr, "fatal: Malformed array in collection '%s'\n", databaseName);
        cJSON_Delete(collection);
        return;
    }
    PrintFilteredDocuments(collection, key, value, condition);
    cJSON_Delete(collection);
}

__declspec(dllexport) void DeleteDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, const Condition condition) {
    GetCollectionFile(path, databaseName, collectionName);
    cJSON* collection = LoadBinary(path);
    if (!collection || !cJSON_IsArray(collection)) {
        fprintf(stderr, "fatal: Collection file for '%s' not found or empty\n", databaseName);
        if (collection) cJSON_Delete(collection);
        return;
    }

    const int deletedCount = DeleteFilteredDocuments(collection, key, value, condition);

    if (deletedCount > 0) {
        DumpBinary(path, collection);
        printf("Document deleted %d\n", deletedCount);
    } else if (deletedCount < 0) {
        fprintf(stderr, "fatal: Failed to delete due to invalid JSON format\n");
    } else {
        printf("No document found for given condition\n");
    }

    cJSON_Delete(collection);
}

__declspec(dllexport) void DeleteAllDocuments(const char* databaseName, const char* collectionName) {
    DeleteDocuments(databaseName, collectionName, NULL, NULL, all);
}

__declspec(dllexport) void UpdateDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, const Condition condition, const Action action, const char* data) {
    GetCollectionFile(path, databaseName, collectionName);
    cJSON* collection = LoadBinary(path);
    if (!collection || !cJSON_IsArray(collection)) {
        fprintf(stderr, "fatal: Collection file for '%s' not found or empty\n", databaseName);
        if (collection) cJSON_Delete(collection);
        return;
    }

    const int updatedCount = UpdateFilteredDocuments(collection, key, value, condition, action, data);

    if (updatedCount > 0) {
        DumpBinary(path, collection);
        printf("Document updated %d\n", updatedCount);
    } else if (updatedCount < 0) {
        fprintf(stderr, "fatal: Failed to update due to invalid format\n");
    } else {
        printf("No document found for given condition\n");
    }

    cJSON_Delete(collection);
}

__declspec(dllexport) void UpdateAllDocuments(const char* databaseName, const char* collectionName, const Action action, const char* data) {
    UpdateDocuments(databaseName, collectionName, NULL, NULL, all, action, data);
}






