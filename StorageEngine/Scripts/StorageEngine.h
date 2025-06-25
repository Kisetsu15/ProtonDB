#ifndef STORAGE_ENGINE_H
#define STORAGE_ENGINE_H

#include "DatabaseUtils.h"

#define EXPORT __declspec(dllexport)

EXPORT Output create_database(QueryConfig config);
EXPORT Output drop_database(QueryConfig config);
EXPORT ArrayOut list_database();

EXPORT Output create_collection(QueryConfig config);
EXPORT Output drop_collection(QueryConfig config);
EXPORT ArrayOut list_collection(QueryConfig config);

EXPORT Output insert_document(QueryConfig config);
EXPORT Output remove_all_documents(QueryConfig config);
EXPORT Output remove_documents(QueryConfig config);
EXPORT ArrayOut print_all_documents(QueryConfig config);
EXPORT ArrayOut print_documents(QueryConfig config);
EXPORT Output update_all_documents(QueryConfig config);
EXPORT Output update_documents(QueryConfig config);

EXPORT void free_list(char** list, int size);
#endif //STORAGE_ENGINE_H
