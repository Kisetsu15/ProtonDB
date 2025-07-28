# 🧠 ProtonDB Python Wrapper

[![PyPI version](https://img.shields.io/pypi/v/pyprotondb.svg)](https://pypi.org/project/pyprotondb/)

A Python wrapper for interacting with **ProtonDB**, a custom TCP-based NoSQL database.  
This package allows you to send ProtonDB commands over a TCP connection through a clean Python API.

---

## 📦 Installation

Install from PyPI using `pip`:

```bash
pip install pyprotondb
```

---

## 🧪 Complete Command Reference

### 1. 🔧 Initialization

```python
from pyprotondb import ProtonDBClient

client = ProtonDBClient("localhost", 9090, "admin420", "1234")  # Connect
client.close()  # Disconnect
```

---

### 2. 🗃️ Database Commands

```python
client.driver.execute("db.create('mydb')")      # Create database
client.driver.execute("db.drop('mydb')")        # Delete database
client.driver.execute("db.use('mydb')")         # Switch to database
client.driver.execute("db.list()")              # List all databases
```

---

### 3. 📁 Collection Commands

```python
client.driver.execute("collection.create('users')")  # Create collection
client.driver.execute("collection.drop('users')")    # Delete collection
client.driver.execute("collection.list()")           # List collections
```

---

### 4. 📄 Document Commands

**Insert Documents**
```python
client.driver.execute('users.insert({"name":"John"})')                     # Insert single document
client.driver.execute('users.insert([{"name":"Alice"}, {"name":"Bob"}])') # Insert multiple documents
```

**Query Documents**
```python
client.driver.execute("users.print()")                   # Print all documents
client.driver.execute("users.print(age >= 18)")          # Print documents with filter
```

**Update Documents**
```python
client.driver.execute('users.update(add, {"role":"admin"}, name="John")')  # Add field
client.driver.execute('users.update(drop, "temp_field")')                  # Remove field
```

**Delete Documents**
```python
client.driver.execute("users.remove(age < 18)")          # Conditional delete
client.driver.execute("users.remove()")                  # Delete all
```

---

### 5. 👤 Profile (User) Commands

```python
client.driver.execute('profile.create("user1", "pass123", "user")')  # Create user
client.driver.execute('profile.delete("user1")')                     # Delete user
client.driver.execute('profile.grant("user1", "mydb")')              # Grant access
client.driver.execute('profile.revoke("user1", "mydb")')             # Revoke access
client.driver.execute("profile.list()")                              # List users
```

---

### 6. 🧰 Utility Commands

```python
client.driver.execute("--help")      # Show help
client.driver.execute("--version")   # Show version
client.driver.execute("cls")         # Clear screen
```

---

### 7. 🧾 Multi-line Insert Example

```python
client.driver.execute('''users.insert(.
  {"name": "John", "age": 25},
  {"name": "Alice", "age": 30}
.)''')
```

---

### 8. 📝 Notes

- Use `client.driver.execute("<command>")` to send any ProtonDB command string.
- Keep command strings exactly as ProtonDB expects (quotes, parentheses, etc.).
- Always call `client.close()` after your operations to free resources.

---


