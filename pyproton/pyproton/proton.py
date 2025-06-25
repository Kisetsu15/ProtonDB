import subprocess
import threading
import time
import json

class PyProtonClient:
    def __init__(self, executable_path):
        self.process = subprocess.Popen(
            [executable_path],
            stdin=subprocess.PIPE,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE,
            text=True,
            bufsize=1
        )
        self.output = []
        self.lock = threading.Lock()

        self.reader_thread = threading.Thread(target=self._read_output, daemon=True)
        self.reader_thread.start()

    def _read_output(self):
        for line in self.process.stdout:
            print("DEBUG:", line.strip())  # For live feedback
            with self.lock:
                self.output.append(line.strip())

    def _send_command(self, command):
        if self.process.poll() is not None:
            raise RuntimeError("ProtonDB process has exited.")
        self.output.clear()
        self.process.stdin.write(command + "\n")
        self.process.stdin.flush()
        time.sleep(1.0)
        with self.lock:
            return list(self.output)

    # --- Database methods ---
    def create_db(self, name):
        return self._send_command(f'db.create("{name}")')

    def use_db(self, name):
        return self._send_command(f'db.use("{name}")')

    def drop_db(self, name=None):
        if name:
            return self._send_command(f'db.drop("{name}")')
        return self._send_command('db.drop()')

    def list_databases(self):
        return self._send_command('db.list()')

    # --- Collection methods ---
    def create_collection(self, name):
        return self._send_command(f'collection.create("{name}")')

    def drop_collection(self, name):
        return self._send_command(f'collection.drop("{name}")')

    def list_collections(self):
        return self._send_command('collection.list()')

    # --- Document methods ---
    

    def insert(self, collection, data):
        data_str = json.dumps(data)
        return self._send_command(f'{collection}.insert({data_str})')

    def print_docs(self, collection, condition=None):
        if condition:
            return self._send_command(f'{collection}.print({condition})')
        return self._send_command(f'{collection}.print()')

    def remove_docs(self, collection, condition=None):
        if condition:
            return self._send_command(f'{collection}.remove({condition})')
        return self._send_command(f'{collection}.remove()')

    def update(self, collection, action, data, condition=None):
        if action == 'drop':
            if isinstance(data, dict):
                keys = ', '.join([f'"{key}"' for key in data.keys()])
                return self._send_command(f'{collection}.update({action}, {{{keys}}})')
        data_str = json.dumps(data)
        if condition:
            return self._send_command(f'{collection}.update({action}, {data_str}, {condition})')
        return self._send_command(f'{collection}.update({action}, {data_str})')


    def help(self):
        return self._send_command(':h')

    def version(self):
        return self._send_command(':v')

    def clear(self):
        return self._send_command('cls')

    def close(self):
        self._send_command(':q')
        time.sleep(0.5)
        self.process.terminate()
