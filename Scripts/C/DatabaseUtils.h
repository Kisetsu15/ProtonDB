#ifndef DATABASE_UTILS_H
#define DATABASE_UTILS_H

#define MAX_DATABASES 100
#define MAX_DB_NAME 64
#define MAX_DB_PATH 128
#define DATABASE "db"
#define DATABASE_META "db/.database.meta"
#define COLLECTION_META ".collection.meta"

#include "cJSON.h"
#include <stdbool.h>

typedef enum {
    collection,
    database
} FileType;

typedef enum {
    greaterThan,
    greaterThanEqual,
    lessThan,
    lessThanEqual,
    equal,
    all,
} Condition;

typedef enum {
    add,
    drop,
    alter
} Action;


bool CheckDatabase(const char* databaseName);
void DeleteDirectoryContent(const char* directory);

bool DumpBinary(const char* fileName, const cJSON* data);
cJSON* LoadBinary(const char* fileName);

void PrintItem(const cJSON* item);
void PrintFilteredDocuments(const cJSON* collection, const char* key, const char* value, Condition condition);
int DeleteFilteredDocuments(cJSON* collection, const char* key, const char* value, Condition condition);
int UpdateFilteredDocuments(cJSON *collection, const char *key, const char *value, const Condition condition, const Action action, const char *param);
bool IsRelated(double value1, double value2, Condition condition);

void AddAction(cJSON* item, const char* param);
void DropAction(cJSON* item, const char* param);
void AlterAction(cJSON* item, const char* param);

bool AppendEntry(const char* metaFile, const char* name, const char* path, FileType fileType);
bool RemoveEntry(const char* metaFile, const char* name, FileType fileType);
const char* FileTypeString(FileType fileType);

cJSON* LoadJson(const char* file_name);
bool SaveJson(const char* filename, cJSON* config);

void GetCollectionFile(char* array, const char* databaseName, const char* collectionName, int size);
void GetCollectionsMeta(char* array, const char* databaseName, int size);
void GetDatabaseDirectory(char* array, const char* databaseName, int size);

#endif //DATABASE_UTILS_H
