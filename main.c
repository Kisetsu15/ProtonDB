#include "storage_engine.h"

int main(void) {
    create_database("helloDB");
    delete_database("hellDB");
    show_database();
    return 0;
}