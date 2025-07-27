import unittest
from pyprotondb import ProtonDBClient
import random
import string

# Change these to match your test server
HOST = "127.0.0.1"
PORT = 9090
USERNAME = "admin123"
PASSWORD = "welcome"

def random_db_name(prefix="testdb"):
    suffix = ''.join(random.choices(string.ascii_lowercase + string.digits, k=6))
    return f"{prefix}_{suffix}"

class TestPyProtonDB(unittest.TestCase):
    @classmethod
    def setUpClass(cls):
        cls.client = ProtonDBClient(HOST, PORT, USERNAME, PASSWORD)
        cls.db_name = random_db_name()
        cls.collection = f"{cls.db_name}.users"

    @classmethod
    def tearDownClass(cls):
        cls.client.close()

    def test_01_create_db(self):
        response = self.client.driver.create_db(self.db_name)
        self.assertEqual(response["Status"].lower(), "ok", msg=response)

    def test_02_insert_document(self):
        doc = {"name": "Sarvesh", "role": "developer"}
        response = self.client.driver.insert_document(self.collection, doc)
        self.assertEqual(response["Status"].lower(), "ok", msg=response)

    def test_03_insert_another_document(self):
        doc = {"name": "Alice", "age": 30}
        response = self.client.driver.insert_document(self.collection, doc)
        self.assertEqual(response["Status"].lower(), "ok", msg=response)

if __name__ == "__main__":
    unittest.main()
