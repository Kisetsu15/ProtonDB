#ifndef DATABASE_UTILS_H
#define DATABASE_UTILS_H

#define MAX_PATH_LEN 512
#define PROTON_DB "ProtonDB"
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
int UpdateFilteredDocuments(cJSON *collection, const char *key, const char *value, const Condition condition, const Action action, const char *data);
bool IsRelated(double value1, double value2, Condition condition);

bool AddAction(cJSON* item, const char* param);
bool DropAction(cJSON* item, const char* param);
bool AlterAction(cJSON* item, const char* param);

bool AppendEntry(const char* metaFile, const char* name, const char* path, FileType fileType);
bool RemoveEntry(const char* metaFile, const char* name, FileType fileType);
const char* FileTypeString(FileType fileType);

cJSON* LoadJson(const char* file_name);
bool SaveJson(const char* filename, cJSON* config);

void GetCollectionFile(char* array, const char* databaseName, const char* collectionName);
void GetCollectionsMeta(char* array, const char* databaseName);
void GetDatabaseDirectory(char* array, const char* databaseName);
void GetDatabaseMeta(char* array);

#endif //DATABASE_UTILS_H
