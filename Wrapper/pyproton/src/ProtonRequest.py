class ProtonRequest:
    @staticmethod
    def build_query(command: str, data: str = "") -> dict:
        return {"Command": command, "Data": data}
    
    @staticmethod
    def login_request(username: str, password: str) -> dict:
        return {"Command": "LOGIN", "Data": f"{username},{password}"}