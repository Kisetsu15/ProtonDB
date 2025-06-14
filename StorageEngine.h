#ifndef STORAGE_ENGINE_H
#define STORAGE_ENGINE_H

#include "DatabaseUtils.h"

void CreateDatabase(const char* databaseName);
void DeleteDatabase(const char* databaseName);
void ListDatabase();
void CreateCollection(const char* databaseName, const char* collectionName);
void DeleteCollection(const char* databaseName, const char* collectionName);
void ListCollection(const char* databaseName);
void InsertDocument(const char* databaseName, const char* collectionName, const char* document);
void DeleteDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, Condition condition);
void DeleteAllDocuments(const char* databaseName, const char* collectionName);
void PrintAllDocuments(const char* databaseName, const char* collectionName);
void PrintDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, Condition condition);
void UpdateAllDocuments(const char* databaseName, const char* collectionName, const Action action, const char* param);
void UpdateDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, const Condition condition, const Action action, const char* param);

#endif //STORAGE_ENGINE_H
