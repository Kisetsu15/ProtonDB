🐍 PyProtonClient – ProtonDB Python Wrapper (Subprocess Version)
PyProtonClient is a Python wrapper for the ProtonDB database server, designed to communicate directly with the compiled .exe server using standard input/output (subprocess). This approach simulates an interactive session and supports sending JSON-based commands and receiving responses, ideal for automation or integration testing.

🚀 Features
Launches ProtonDB.exe and manages its lifecycle

Performs interactive authentication (host, port, username, password)

Sends JSON commands using ProtonDB's CLI protocol

Supports database, collection, and document operations

Thread-safe output reading and command handling

📦 Requirements
Python 3.6+

ProtonDB compiled .exe (e.g. ProtonDB.exe)

Windows system (tested with PowerShell)

🛠️ Setup
Ensure ProtonDB.exe is present and accessible (e.g., D:/STUDYYY/ProtonDB/ProtonDB.exe)

Place pyproton.py in your project directory

Create a client using:

python
Copy
Edit
from pyproton import PyProtonClient

client = PyProtonClient("D:/STUDYYY/ProtonDB/ProtonDB.exe", username="admin123", password="welcome")
📘 Usage
python
Copy
Edit
client.create_db("mydb")
client.use_db("mydb")
client.create_collection("users")

# Insert a document
client.insert("users", {"name": "Alice", "age": 25})

# Print all documents
print(client.print_docs("users"))

# Update documents
client.update("users", action="set", data={"age": 26})

# Remove documents
client.remove_docs("users", '{"name": "Alice"}')

# Close the connection
client.close()
🔐 Authentication
This wrapper simulates keypress-based authentication prompts:

Sends blank input for host (127.0.0.1)

Sends 9090 as the default port

Then sends the username and password

Uses delays to ensure server reads each line correctly

📑 Methods
Database
create_db(name)

use_db(name)

drop_db(name=None)

list_databases()

Collection
create_collection(name)

drop_collection(name)

list_collections()

Documents
insert(collection, data)

print_docs(collection, condition=None)

remove_docs(collection, condition=None)

update(collection, action, data, condition=None)

Meta
help()

version()

clear()

close()

⚠️ Notes
All communication is done via JSON line commands (e.g., { \"command\": \"QUERY\", \"data\": \"users.insert(...)\" })

subprocess I/O can be slow; keep time.sleep() values as needed

Avoid rapid-fire commands without waiting for a prompt

