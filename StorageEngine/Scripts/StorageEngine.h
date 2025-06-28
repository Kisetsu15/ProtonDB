#ifndef STORAGE_ENGINE_H
#define STORAGE_ENGINE_H

#include "DatabaseUtils.h"
#define export __declspec(dllexport)

export Output create_database(QueryConfig config);
export Output drop_database(QueryConfig config);
export ArrayOut list_database();

export Output create_collection(QueryConfig config);
export Output drop_collection(QueryConfig config);
export ArrayOut list_collection(QueryConfig config);

export Output insert_document(QueryConfig config);
export Output remove_all_documents(QueryConfig config);
export Output remove_documents(QueryConfig config);
export ArrayOut print_all_documents(QueryConfig config);
export ArrayOut print_documents(QueryConfig config);
export Output update_all_documents(QueryConfig config);
export Output update_documents(QueryConfig config);

export void free_list(char** list, int size);
#endif //STORAGE_ENGINE_H
