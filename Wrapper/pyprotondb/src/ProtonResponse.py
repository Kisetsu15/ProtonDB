import json
class ProtonResponse:
    @staticmethod
    def parse(response: str) -> dict:
        try:
            return json.loads(response)
        except json.JSONDecodeError:
            return {"Status": "error", "Message": "Invalid response format"}
    
    @staticmethod
    def is_success(response: dict) -> bool:
        return response.get("Status", "").lower() == "ok"