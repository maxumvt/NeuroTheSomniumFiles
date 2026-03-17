import websocket
import threading
import json
import time

class NetworkClient:
    def __init__(self):
        self.ws = None
        self.on_message_received = None
        self.connected = False  # <-- flag

    def connect(self):
        def on_open(ws):
            print("[WebSocket] Connected")
            self.connected = True

        def on_message(ws, message):
            print("[WebSocket] Received:", message)
            if self.on_message_received:
                self.on_message_received(message)

        def on_error(ws, error):
            print("[WebSocket] Error:", error)

        def on_close(ws, close_status_code, close_msg):
            print("[WebSocket] Closed:", close_status_code, close_msg)
            self.connected = False

        self.ws = websocket.WebSocketApp(
            "ws://localhost:8000",
            on_open=on_open,
            on_message=on_message,
            on_error=on_error,
            on_close=on_close
        )

        thread = threading.Thread(target=self.ws.run_forever)
        thread.daemon = True
        thread.start()

    def send_string(self, json_str):
        # wait until connected
        if self.ws:
            while not self.connected:
                time.sleep(0.01)
            self.ws.send(json_str)

# ---- Test harness ----

def handle_message(msg):
    print("[HANDLER] Got message:", msg)

if __name__ == "__main__":
    client = NetworkClient()
    client.on_message_received = handle_message

    client.connect()

    # Send startup
    startup_msg = {
        "command": "startup",
        "game": "AI Somnium Files"
    }
    client.send_string(json.dumps(startup_msg))

    # Register actions
    register_msg = {
        "command": "actions/register",
        "game": "AI Somnium Files",
        "data": {
            "actions": [
                {"name": "button_up", "description": "Press up"},
                {"name": "button_down", "description": "Press down"}
            ]
        }
    }
    client.send_string(json.dumps(register_msg))

    # Force action
    force_msg = {
        "command": "actions/force",
        "game": "AI Somnium Files",
        "data": {
            "query": "Pick an action",
            "action_names": ["button_up", "button_down"]
        }
    }
    client.send_string(json.dumps(force_msg))

    # Keep alive
    while True:
        time.sleep(1)