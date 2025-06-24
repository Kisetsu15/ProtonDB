#ifndef STORAGE_ENGINE_H
#define STORAGE_ENGINE_H

#include "DatabaseUtils.h"

__declspec(dllexport) const char* create_database(const char* databaseName);
__declspec(dllexport) const char* drop_database(const char* databaseName);
__declspec(dllexport) const char** list_database(int* count);
__declspec(dllexport) const char* create_collection(const char* databaseName, const char* collectionName);
__declspec(dllexport) const char* drop_collection(const char* databaseName, const char* collectionName);
__declspec(dllexport) const char** list_collection(const char* databaseName, int* count);
__declspec(dllexport) const char* insert_document(const char* databaseName, const char* collectionName, const char* document);
__declspec(dllexport) const char* remove_all_documents(const char* databaseName, const char* collectionName);
__declspec(dllexport) const char* remove_documents(const char* databaseName, const char* collectionName, const char* key, const char* value, Condition condition);
__declspec(dllexport) const char** print_all_documents(const char* databaseName, const char* collectionName, char* message, int* count);
__declspec(dllexport) const char** print_documents(const char* databaseName, const char* collectionName, const char* key, const char* value, Condition condition, char* message, int* count);
__declspec(dllexport) const char* update_all_documents(const char* databaseName, const char* collectionName, Action action, const char* data);
__declspec(dllexport) const char* update_documents(const char* databaseName, const char* collectionName, const char* key, const char* value, Condition condition, Action action, const char* data);
__declspec(dllexport) void free_list(char** list);
__declspec(dllexport) void free_count(int* count);
#endif //STORAGE_ENGINE_H
