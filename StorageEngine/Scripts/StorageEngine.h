#ifndef STORAGE_ENGINE_H
#define STORAGE_ENGINE_H

#include "DatabaseUtils.h"

__declspec(dllexport) const char* CreateDatabase(const char* databaseName);
__declspec(dllexport) void DeleteDatabase(const char* databaseName);
__declspec(dllexport) void ListDatabase();
__declspec(dllexport) void CreateCollection(const char* databaseName, const char* collectionName);
__declspec(dllexport) void DeleteCollection(const char* databaseName, const char* collectionName);
__declspec(dllexport) void ListCollection(const char* databaseName);
__declspec(dllexport) void InsertDocument(const char* databaseName, const char* collectionName, const char* document);
__declspec(dllexport) void DeleteDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, Condition condition);
__declspec(dllexport) void DeleteAllDocuments(const char* databaseName, const char* collectionName);
__declspec(dllexport) void PrintAllDocuments(const char* databaseName, const char* collectionName);
__declspec(dllexport) void PrintDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, Condition condition);
__declspec(dllexport) void UpdateAllDocuments(const char* databaseName, const char* collectionName, const Action action, const char* data);
__declspec(dllexport) void UpdateDocuments(const char* databaseName, const char* collectionName, const char* key, const char* value, const Condition condition, const Action action, const char* data);

#endif //STORAGE_ENGINE_H
