#ifndef STORAGE_ENGINE_H
#define STORAGE_ENGINE_H

#define MAX_KEY_LENGTH 32
#define MAX_VALUE_LENGTH 128

typedef struct {
    char key[MAX_KEY_LENGTH];
    char value[MAX_VALUE_LENGTH];
} Item;

typedef struct {
    int item_count;
    Item* items;
} Document;

typedef struct {
    char* name;
    int document_count;
    Document* documents;
} Collection;

void create_database(const char* db_name);
void delete_database(const char* db_name);
void create_collection(const char* db_name, const char* collection_name);
void insert_document(const char* db_name, const char* collection_name, Document* document);

void free_document(Document* document);
void free_collection(Collection* collection);

#endif //STORAGE_ENGINE_H
