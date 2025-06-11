#include <stdlib.h>
#include <string.h>
#include <direct.h>
#include <io.h>
#include <stdio.h>
#include "database_utils.h"

int check_database(const char* db_name) {
    DatabaseInfo dbs[MAX_DATABASES];
    const int db_count = load_databases(DATABASE_META, dbs, MAX_DATABASES);

    for (int i = 0; i < db_count; ++i) {
        if (strcmp(dbs[i].name, db_name) == 0) return 1;
    }
    return 0;
}

void delete_dir_content(const char* dir_path) {
    char search_path[256];
    snprintf(search_path, sizeof(search_path), "%s\\*.*", dir_path);

    struct _finddata_t file;
    intptr_t h = _findfirst(search_path, &file);
    if (h == -1) return;

    do {
        if (strcmp(file.name, ".") != 0 && strcmp(file.name, "..") != 0) {
            char full_path[256];
            snprintf(full_path, sizeof(full_path), "%s\\%s", dir_path, file.name);
            remove(full_path);
        }
    } while (_findnext(h, &file) == 0);

    _findclose(h);
}

int remove_database_entry(const char* filename, const char* entry) {
    FILE* src = fopen(filename, "r");
    if (!src) return 0;

    FILE* temp = fopen("temp.meta", "w");
    if (!temp) {
        fclose(src);
        return 0;
    }

    char buffer[256];
    int removed = 0;

    while (fgets(buffer, sizeof(buffer), src)) {
        buffer[strcspn(buffer, "\n")] = '\0';

        if (strcmp(buffer, entry) == 0) {
            removed = 1;
            continue;
        }

        fprintf(temp, "%s\n", buffer);
    }

    fclose(src);
    fclose(temp);

    remove(filename);
    rename("temp.meta", filename);

    return removed;
}

void append_database_entry(const char* filename, const char* name, const char* path) {
    FILE* file = fopen(filename, "a");
    if (!file) {
        perror("Failed to open database registry file");
        return;
    }

    fprintf(file, "%s|%s\n", name, path);
    fclose(file);
}


int load_databases(const char* filename, DatabaseInfo* db_list, const int max_dbs) {
    FILE* file = fopen(filename, "r");
    if (!file) return 0;

    char line[256];
    int count = 0;

    while (fgets(line, sizeof(line), file) && count < max_dbs) {
        line[strcspn(line, "\n")] = '\0'; // Remove newline
        char* sep = strchr(line, '|');
        if (!sep) continue;

        *sep = '\0';

        strncpy(db_list[count].name, line, MAX_DB_NAME);
        strncpy(db_list[count].path, sep + 1, MAX_DB_PATH);
        count++;
    }

    fclose(file);
    return count;
}
