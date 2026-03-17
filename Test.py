import websocket
import threading
import json
import time

class NeuroClient:
    def __init__(self):
        self.ws = None
        self.connected = False
        self.registered_actions = {}  # name -> description

    def connect(self, url="ws://localhost:8000"):
        """Connect to Neuro/Randy websocket."""
        def on_open(ws):
            print("[WebSocket] Connected")
            self.connected = True

        def on_message(ws, message):
            print("[WebSocket] Received:", message)
            try:
                msg = json.loads(message)
            except json.JSONDecodeError:
                return

            # Respond to Neuro's action execution
            if msg.get("command") == "action":
                self.handle_action(msg)

        def on_error(ws, error):
            print("[WebSocket] Error:", error)

        def on_close(ws, code, reason):
            print("[WebSocket] Closed:", code, reason)
            self.connected = False

        self.ws = websocket.WebSocketApp(
            url,
            on_open=on_open,
            on_message=on_message,
            on_error=on_error,
            on_close=on_close
        )
        thread = threading.Thread(target=self.ws.run_forever)
        thread.daemon = True
        thread.start()

    def send(self, data: dict):
        """Send JSON over websocket."""
        while not self.connected:
            time.sleep(0.01)
        self.ws.send(json.dumps(data))

    # --- Action Handling ---
    def register_actions(self, actions: list):
        """
        Register actions with Neuro/Randy.
        `actions` is a list of dicts: {"name": "button_up", "description": "..."}
        """
        self.registered_actions = {a["name"]: a["description"] for a in actions}
        msg = {
            "command": "actions/register",
            "game": "AI Somnium Files",
            "data": {"actions": actions}
        }
        print("[NeuroClient] Registering actions:", actions)
        self.send(msg)

    def send_force_action(self, query: str, action_names: list):
        """Send a force action to Neuro/Randy."""
        msg = {
            "command": "actions/force",
            "game": "AI Somnium Files",
            "data": {
                "state": "Test scenario",
                "query": query,
                "ephemeral_context": False,
                "priority": "low",
                "action_names": action_names
            }
        }
        print("[NeuroClient] Sending force action:", msg)
        self.send(msg)

    def handle_action(self, msg: dict):
        """Validate and respond to Neuro's action message."""
        action_id = msg["data"]["id"]
        action_name = msg["data"]["name"]

        print(f"[NeuroClient] Neuro requested action: {action_name}")

        success = action_name in self.registered_actions
        message = ""

        if success:
            message = f"Action {action_name} executed successfully"
        else:
            message = f"Action {action_name} is not registered"

        result_msg = {
            "command": "action/result",
            "data": {
                "id": action_id,
                "success": success,
                "message": message
            }
        }
        print("[NeuroClient] Sending action result:", result_msg)
        self.send(result_msg)

# --- Example Usage ---
if __name__ == "__main__":
    client = NeuroClient()
    client.connect("ws://localhost:8000")

    # Wait a short moment for connection
    time.sleep(0.2)

    # 1️⃣ Startup
    startup_msg = {"command": "startup", "game": "AI Somnium Files"}
    client.send(startup_msg)

    # 2️⃣ Register actions
    actions = [
        {"name": "button_up", "description": "Press up"},
        {"name": "button_down", "description": "Press down"},
        {"name": "button_left", "description": "Press left"},
        {"name": "button_right", "description": "Press right"}
    ]
    client.register_actions(actions)

    # 3️⃣ Force an action
    client.send_force_action("Please choose an action", ["button_up", "button_down"])

    time.sleep(5.0)

    client.send_force_action("Please choose an action", ["button_left", "button_right"])

    # Keep alive to receive Randy's action request
    while True:
        time.sleep(1)