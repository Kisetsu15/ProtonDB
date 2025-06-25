#ifndef DATABASE_UTILS_H
#define DATABASE_UTILS_H

#define MAX_PATH_LEN 512
#define MAX_MESSAGE_LEN 384
#define MAX_ERROR_LEN 256
#define PROTON_DB "ProtonDB"
#define DB "db"
#define DATABASE_META "db/.database.meta"
#define COLLECTION_META ".collection.meta"
#define NEW_OUTPUT ((Output){0})
#define NEW_ARRAY_OUT ((ArrayOut){0})

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
    all
} Condition;

typedef enum {
    add,
    drop,
    alter
} Action;

typedef struct {
    bool success;
    char message[MAX_MESSAGE_LEN];
} Output;

typedef struct {
    int size;
    char message[MAX_MESSAGE_LEN];
    char** list;
} ArrayOut;

typedef struct {
    const char* databaseName;
    const char* collectionName;
    // Conditional arguments
    const char* key;
    const char* value;
    // Data to store
    const char* data;
    Condition condition;
    Action action;
} QueryConfig;

bool check_database(const char* databaseName);
void delete_dir_content(const char* directory);
int load_list(const char* metaFile, char*** list, char* error);
bool dump_binary(const char* fileName, const cJSON* data, char* error);
cJSON* load_binary(const char* fileName, char* error);

void print_item(char** document, int index, const cJSON* item);
int print_filtered_documents(cJSON* collection, const char* key, const char* value, Condition condition, char*** list, char* error);
int remove_filtered_documents(cJSON* collection, const char* key, const char* value, Condition condition, char* error);
int update_filtered_documents(cJSON *collection, const char *key, const char *value, Condition condition, Action action, const char *data, char* error);
bool is_related(double value1, double value2, Condition condition);

bool add_action(cJSON* item, const char* data, char* error);
bool drop_action(cJSON* item, const char* data, char* error);
bool alter_action(cJSON* item, const char* data, char* error);

bool append_entry(const char* metaFile, const char* name, const char* path, FileType fileType, char* error);
bool remove_entry(const char* metaFile, const char* name, FileType fileType, char* error);
const char* file_type_string(FileType fileType);

cJSON* load_json(const char* file_name);
bool save_json(const char* filename, cJSON* config, char* error);

void get_col_file(char* array, const char* databaseName, const char* collectionName);
void get_col_meta(char* array, const char* databaseName);
void get_database_dir(char* array, const char* databaseName);
void get_database_meta(char* array);
void get_message(char* buffer, const char* format, ...);
void get_error(char* buffer, const char* format, ...);
#endif //DATABASE_UTILS_H
