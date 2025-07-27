from connection import Connection

def test_protondb_operations():
    conn = None
    try:
        # 1. Connect to ProtonDB with admin credentials
        conn = Connection("localhost", 9090, "admin123", "welcome")
        print("✅ Connected to ProtonDB server")

        # 2. Database operations
        print("\n=== DATABASE OPERATIONS ===")
        
        # Create database
        response = conn.send_request("QUERY", "db.create('school_db')")
        print(f"Create database: {response.get('Message', response)}")
        
        # Switch to database context
        response = conn.send_request("QUERY", "db.use('school_db')")
        print(f"Use database: {response.get('Message', response)}")
        
        # List databases
        response = conn.send_request("QUERY", "db.list()")
        print(f"Database list: {response.get('Result', response)}")

        # 3. Collection operations
        print("\n=== COLLECTION OPERATIONS ===")
        
        # Create collection
        response = conn.send_request("QUERY", "collection.create('students')")
        print(f"Create collection: {response.get('Message', response)}")
        
        # List collections
        response = conn.send_request("QUERY", "collection.list()")
        print(f"Collection list: {response.get('Result', response)}")

        # 4. Document operations
        print("\n=== DOCUMENT OPERATIONS ===")
        
        # Insert single document (correct JSON format)
        doc1 = '{"name": "John Doe", "age": 16, "grade": "10A", "subjects": ["Math", "Science"]}'
        response = conn.send_request("QUERY", f"students.insert({doc1})")
        print(f"Insert document: {response.get('Message', response)}")
        
        # Insert multiple documents
        docs = '[{"name": "Alice Smith", "age": 15, "grade": "9B"}, {"name": "Bob Johnson", "age": 17, "grade": "11C"}]'
        response = conn.send_request("QUERY", f"students.insert({docs})")
        print(f"Insert multiple: {response.get('Message', response)}")
        
        # Print all documents
        response = conn.send_request("QUERY", "students.print()")
        print("\nAll documents:")
        print(response.get('Result', response))
        
        # Print filtered documents (age >= 16)
        response = conn.send_request("QUERY", "students.print(age >= 16)")
        print("\nFiltered documents (age >= 16):")
        print(response.get('Result', response))
        
        # Update document
        response = conn.send_request("QUERY", 'students.update(add, {"status": "active"}, name = "John Doe")')
        print(f"\nUpdate document: {response.get('Message', response)}")
        
        # Verify update
        response = conn.send_request("QUERY", "students.print(name = 'John Doe')")
        print("\nUpdated document:")
        print(response.get('Result', response))
        
        # 5. Cleanup (optional)
        print("\n=== CLEANUP ===")
        response = conn.send_request("QUERY", "collection.drop('students')")
        print(f"Drop collection: {response.get('Message', response)}")
        
        response = conn.send_request("QUERY", "db.drop('school_db')")
        print(f"Drop database: {response.get('Message', response)}")

    except Exception as e:
        print(f"❌ Error: {str(e)}")
    finally:
        if conn:
            conn.close()
            print("\n✅ Connection closed")

if __name__ == "__main__":
    test_protondb_operations()