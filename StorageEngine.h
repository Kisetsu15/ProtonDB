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
void TruncateDocuments(const char* databaseName, const char* collectionName);
void PrintAllDocuments(const char* databaseName, const char* collectionName);
void PrintDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, Condition condition);

#endif //STORAGE_ENGINE_H
