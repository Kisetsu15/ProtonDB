import json
from typing import Optional, Dict, Any, Union
from connection import Connection
from ProtonResponse import ProtonResponse

class ProtonDBDriver:
    def __init__(self, connection):
        self.conn = connection
        
    def create_db(self, name: str):
        return self.conn.send_request("QUERY", f"db.create('{name}')")
    
    def insert_document(self, collection: str, document: dict):
        return self.conn.send_request("QUERY", f"{collection}.insert({json.dumps(document)})")