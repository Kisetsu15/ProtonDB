from typing import Optional
from .connection import Connection
from .ProtonRequest import ProtonRequest
from .ProtonResponse import ProtonResponse
from .protondbdriver import ProtonDBDriver

class ProtonDBClient:
    def __init__(self, host, port, username, password):
        self.connection = Connection(host, port, username, password)
        self.driver = ProtonDBDriver(self.connection)
    
    def close(self):
        self.connection.close()
