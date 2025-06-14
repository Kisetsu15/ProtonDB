#include <direct.h>
#include <stdio.h>
#include <stdlib.h>
#include "StorageEngine.h"
#include "DatabaseUtils.h"

char path[128];
char metaFile[128];

void CreateDatabase(const char* databaseName) {
    if (strlen(databaseName) + 4 >= sizeof(path)) {
        printf("Database name too long.\n");
        return;
    }

    if (CheckDatabase(databaseName)) {
        printf("Database '%s' already exists.\n", databaseName);
        return;
    }

    GetDatabaseDirectory(path, databaseName, sizeof(path));

    if (_mkdir(path) != 0 || !AppendEntry(DATABASE_META, databaseName, path, database)) {
        perror("fatal: could not create database");
    } else {
        printf("Database '%s' created.\n", databaseName);
    }
}

void DeleteDatabase(const char* databaseName) {
    if (!CheckDatabase(databaseName)) {
        printf("Database '%s' doesn't exists.\n", databaseName);
        return;
    }

    GetDatabaseDirectory(path, databaseName, sizeof(path));
    DeleteDirectoryContent(path);

    if (_rmdir(path) != 0 || !RemoveEntry(DATABASE_META, databaseName, database)) {
        perror("fatal: could not remove database");
    } else {
        printf("Database '%s' deleted.\n", databaseName);
    }
}

void ListDatabase() {
    cJSON* databaseMeta = LoadJson(DATABASE_META);
    if (!databaseMeta || !cJSON_IsObject(databaseMeta)) {
        fprintf(stderr, "fatal: Failed to load or parse database metadata.\n");
        if (databaseMeta) cJSON_Delete(databaseMeta);
        return;
    }

    cJSON* item = databaseMeta->child;
    if (!item) {
        printf("No Database found.\n");
    }

    while (item) {
        printf("%s\n", item->string);
        item = item->next;
    }

    cJSON_Delete(databaseMeta);
}

void CreateCollection(const char* databaseName, const char* collectionName) {
    if (!CheckDatabase(databaseName)) {
        printf("Database '%s' does not exist.\n", databaseName);
        return;
    }

    if (strlen(collectionName) + strlen(databaseName) + 8 >= sizeof(path)) {
        printf("Collection name too long.\n");
        return;
    }

    GetCollectionsMeta(metaFile, databaseName, sizeof(metaFile));
    GetCollectionFile(path, databaseName, collectionName, sizeof(path));

    if (!AppendEntry(metaFile, collectionName, path, collection)) return;

    cJSON* data = cJSON_CreateArray();
    if (!data || !DumpBinary(path, data)) {
        perror("fatal: could not create Collection");
    } else {
        printf("Collection '%s' created.\n", collectionName);
    }

    cJSON_Delete(data);
}

void DeleteCollection(const char* databaseName, const char* collectionName) {
    if (!CheckDatabase(databaseName)) {
        printf("Database '%s' doesn't exist.\n", databaseName);
        return;
    }
    GetCollectionsMeta(metaFile, databaseName, sizeof(metaFile));
    if (RemoveEntry(metaFile, collectionName, collection)) {
        GetCollectionFile(path, databaseName, collectionName, sizeof(path));
        remove(path);
        printf("Collection '%s' deleted.\n", collectionName);
    } else {
        printf("fatal: could not delete collection '%s'\n", collectionName);
    }

}

void ListCollection(const char* databaseName) {
    GetCollectionsMeta(path, databaseName, sizeof(path));

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

void InsertDocument(const char* databaseName, const char* collectionName, const char* document) {
    GetCollectionFile(path, databaseName, collectionName, sizeof(path));
    cJSON* root = LoadBinary(path);
    if (!root) {
        root = cJSON_CreateArray();
    }

    cJSON* parsedDocument = cJSON_Parse(document);
    if (!parsedDocument || !cJSON_IsObject(parsedDocument)) {
        fprintf(stderr, "Invalid JSON document format.\n");
        cJSON_Delete(parsedDocument);
        cJSON_Delete(root);
        return;
    }
    cJSON_AddItemToArray(root, parsedDocument);
    DumpBinary(path, root);
    cJSON_Delete(root);

    printf("Inserted\n");
}

void PrintAllDocuments(const char* databaseName, const char* collectionName) {
    PrintDocuments(databaseName, collectionName, NULL, NULL, all);
}


void PrintDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, const Condition condition) {
    GetCollectionFile(path, databaseName, collectionName, sizeof(path));
    cJSON* collection = LoadBinary(path);
    if (!collection) {
        fprintf(stderr, "fatal: Collection file for '%s' not found or empty\n", databaseName);
        return;
    }
    if (!cJSON_IsArray(collection)) {
        fprintf(stderr, "fatal: Malformed JSON array in collection '%s'\n", databaseName);
        cJSON_Delete(collection);
        return;
    }
    PrintFilteredDocuments(collection, key, value, condition);
    cJSON_Delete(collection);
}

void DeleteDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, const Condition condition) {
    GetCollectionFile(path, databaseName, collectionName, sizeof(path));
    cJSON* collection = LoadBinary(path);
    if (!collection || !cJSON_IsArray(collection)) {
        fprintf(stderr, "fatal: Failed to load or parse collection for '%s'\n", databaseName);
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

void DeleteAllDocuments(const char* databaseName, const char* collectionName) {
    DeleteDocuments(databaseName, collectionName, NULL, NULL, all);
}






