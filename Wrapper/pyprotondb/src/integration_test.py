import json
from ProtonDBclient import ProtonDBClient
from connection import Connection
from ProtonRequest import ProtonRequest
from ProtonResponse import ProtonResponse
from protondbdriver import ProtonDBDriver

def run_integration_test():
    print("🚀 Starting ProtonDB Integration Test\n")
    
    # Test 1: Connection and Authentication
    print("=== Testing Connection.py ===")
    try:
        conn = Connection("localhost", 9090, "admin123", "welcome")
        print("✅ Connection and authentication successful")
    except Exception as e:
        print(f"❌ Connection failed: {str(e)}")
        return
    
    # Test 2: ProtonRequest
    print("\n=== Testing ProtonRequest.py ===")
    request = ProtonRequest.build_query("QUERY", "db.list()")
    if isinstance(request, dict) and "Command" in request:
        print("✅ ProtonRequest working correctly")
    else:
        print("❌ ProtonRequest failed")
    
    # Test 3: ProtonResponse
    print("\n=== Testing ProtonResponse.py ===")
    test_response = '{"Status":"ok","Message":"Success"}'
    parsed = ProtonResponse.parse(test_response)
    if ProtonResponse.is_success(parsed):
        print("✅ ProtonResponse working correctly")
    else:
        print("❌ ProtonResponse failed")
    
    # Test 4: ProtonDBDriver
    print("\n=== Testing protondbdriver.py ===")
    driver = ProtonDBDriver(conn)
    db_result = driver.create_db("test_db")
    if ProtonResponse.is_success(db_result):
        print("✅ Database operations working")
    else:
        print(f"❌ Database operation failed: {db_result}")
    
    # Test 5: Full Client
    print("\n=== Testing ProtonDBClient.py ===")
    client = ProtonDBClient("localhost", 9090, "admin123", "welcome")
    try:
        doc_result = client.driver.insert_document("test_collection", {"name":"Test"})
        if ProtonResponse.is_success(doc_result):
            print("✅ Full client workflow working")
        else:
            print(f"❌ Client operation failed: {doc_result}")
    finally:
        client.close()
    
    print("\n🧪 Integration Test Complete")

if __name__ == "__main__":
    run_integration_test()