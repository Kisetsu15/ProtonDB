import socket
import json
from typing import Dict, Any

class Connection:
    def __init__(self, host: str, port: int, username: str, password: str):
        self.socket = socket.socket(socket.AF_INET, socket.SOCK_STREAM)
        self.socket.connect((host, port))
        self.reader = self.socket.makefile('r', encoding='utf-8')
        self.writer = self.socket.makefile('w', encoding='utf-8')
        
        # Receive welcome message
        welcome = self._receive()
        print(f"Server: {welcome}")
        
        # Authenticate (Java-style)
        login_data = f"{username},{password}"
        login_request = json.dumps({
            "Command": "LOGIN",
            "Data": login_data
        })
        self._send(login_request)
        
        auth_response = self._receive()
        print(f"Auth Response: {auth_response}")
        
        if not self._is_success(auth_response):
            self.close()
            raise ConnectionError(f"Authentication failed: {auth_response}")

    def _is_success(self, response: str) -> bool:
        try:
            data = json.loads(response)
            return data.get("Status", "").lower() == "ok"
        except:
            return False

    def _send(self, data: str):
        self.writer.write(data + '\n')
        self.writer.flush()

    def _receive(self) -> str:
        return self.reader.readline().strip()

    def send_request(self, command: str, data: str) -> Dict[str, Any]:
        """Java-style request handling with automatic FETCH"""
        request = json.dumps({
            "Command": command,
            "Data": data
        })
        self._send(request)
        
        # Get initial response
        response = self._receive()
        response_data = json.loads(response)
        
        # Handle FETCH if needed (matches Java behavior)
        if response_data.get("Message", "").startswith("Query accepted"):
            self._send(json.dumps({"Command": "FETCH"}))
            response = self._receive()
            return json.loads(response)
            
        return response_data

    def close(self):
        try:
            self.writer.close()
            self.reader.close()
            self.socket.close()
        except Exception:
            pass

    def __enter__(self):
        return self

    def __exit__(self, exc_type, exc_val, exc_tb):
        self.close()