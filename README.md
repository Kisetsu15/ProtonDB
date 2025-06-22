# ⚛ ProtonDB

**ProtonDB** is a lightweight, portable NoSQL-like database engine designed for simplicity, scriptability, and atomic operations. Built in C and driven via a powerful C# CLI frontend, ProtonDB offers a document-oriented approach with table-like structure.

---

## 🚀 Features

- 🗃️ Database and collection management
- 📄 Document insertion, printing, updating, and removal
- ⚡ In-memory speed with binary storage
- ✨ JSON-based documents
- 🧪 Conditional filtering and update expressions
- 💻 CLI-driven interface

---

## 📦 Example Commands

### 🔧 Database Operations
```bash
db.use("users")         # Select an active database
db.create("users")      # Create a new database
db.drop()               # Drop the current database
db.drop("logs")         # Drop a specific database
db.list()               # List all databases
````

### 📂 Collection Operations

```bash
collection.create("profiles")   # Create a new collection
collection.drop("logs")         # Drop a collection
collection.list()               # List all collections in current database
```

### 📝 Document Operations

```bash
profiles.insert({ "name": "Alice", "age": 30 })      # Insert document
profiles.print()                                     # Print all documents
profiles.print(age>=18)                              # Filtered print
profiles.remove()                                    # Remove all documents
profiles.remove(name=Bob)                            # Conditional removal

# Document Updates
profiles.update(add, {"score": 100})                 # Add new field to all
profiles.update(drop, {"score"})                     # Drop field from all
profiles.update(alter, {"name": "John"}, name=Tom)   # Alter conditionally
```

---

## 📘 Syntax Reference

| Concept       | Syntax Example                  |
| ------------- | ------------------------------- |
| **action**    | `add`, `drop`, `alter`          |
| **data**      | `{ "key": value }`              |
| **condition** | `key<op>value` (e.g., `age>30`) |
| **operators** | `<`, `<=`, `>`, `>=`, `=`       |

📌 **Note:** For `drop`, use data as `{ "key" }`

---

## 🧠 Commands

| Command    | Description            |
| ---------- | ---------------------- |
| `protondb` | Start the ProtonDB CLI |
| `:h`       | Show help message      |
| `:v`       | Show current version   |
| `:q`       | Quit the CLI           |

---

## 📄 License

MIT License — Use, modify, and distribute freely.

---

## ✨ Author

Created with purpose by Kisetsu
