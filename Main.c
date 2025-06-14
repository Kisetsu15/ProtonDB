#include "StorageEngine.h"

int main(void) {

    //InsertDocument("schoolDB", "students","{\"name\": \"hen\", \"age\": 10}");
    //InsertDocument("schoolDB", "students","{\"name\": \"elephant\", \"age\": 30}");
    //DeleteDocument("schoolDB", "students", "name", "elephant", equal);
    UpdateAllDocuments("schoolDB", "students", alter, "{\"gpa\":4.4}");
    //PrintDocuments("schoolDB", "students", "age", "15", lessThanEqual);
    //DeleteAllDocuments("schoolDB", "students");
    return 0;
}