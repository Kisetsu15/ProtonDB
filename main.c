#include "storage_engine.h"

int main(void) {
    //create_database("helloDB");
    //create_database("schoolDB");
    //create_database("officeDB");
    //delete_database("helloDB");
    show_database();

    create_collection("helloDB", "demo");
    return 0;
}