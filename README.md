# ⚛️ ProtonDB

**ProtonDB** is a lightweight, portable, NoSQL-inspired database engine designed for simplicity, scriptability, and atomic document operations.
Built in **C (storage engine)** and powered by a sleek **C# CLI frontend**, it delivers fast, JSON-based, document-oriented data management — all without a server.

---

## 🚀 Features

* 🗂️ Lightweight database and collection management
* 🧾 Document insert, query, update, and delete operations
* ⚡ In-memory speed with persistent binary storage
* 📦 JSON-based documents, human-friendly and machine-fast
* 🧠 Conditional filters and smart update actions
* 🖥️ Minimalist CLI interface with expressive syntax
* 🧪 Multi-line input support for scripting

---

## 📦 Example Commands

### 🔧 Database Operations

```bash
db.use("users")           # Use an existing database
db.create("users")        # Create a new database
db.drop()                 # Drop the current database
db.drop("logs")           # Drop a specific database
db.list()                 # List all databases
```

### 📂 Collection Operations

```bash
collection.create("profiles")   # Create a new collection
collection.drop("logs")         # Drop a collection
collection.list()               # List collections in current DB
```

### 📄 Document Operations

```bash
profiles.insert({ "name": "Alice", "age": 30 })             # Insert one document
profiles.insert([{ "name": "Alice" }, { "name": "Ben" }])   # Insert many
profiles.print()                                             # Print all
profiles.print(age>=18)                                      # Filtered print
profiles.remove()                                            # Remove all documents
profiles.remove(name=Bob)                                    # Conditional removal

# Update operations
profiles.update(add, {"score": 100})                         # Add new field to all
profiles.update(drop, {"score"})                             # Remove field
profiles.update(alter, {"name": "John"}, name=Tom)           # Conditionally alter
```

---

## 🧾 Multi-Line Input

```bash
db.
create(
  "school"
)
---
profiles.remove(
  age > 19
)
---
collection.
list()
```

---

## 📘 Syntax Reference

| Element       | Example                    |
| ------------- | -------------------------- |
| **Action**    | `add`, `drop`, `alter`     |
| **Data**      | `{ "key": value }`         |
| **Condition** | `key<op>value` → `age>=18` |
| **Operators** | `<`, `<=`, `>`, `>=`, `=`  |

📝 **Note:** Use `{ "key" }` format for `drop` actions.

---

## 💻 CLI Meta Commands

| Command           | Description              |
| ----------------- | ------------------------ |
| `protondb`        | Start the ProtonDB shell |
| `:h`, `--help`    | Show help message        |
| `:v`, `--version` | Show version info        |
| `:q`, `quit`      | Quit the CLI             |
| `cls`             | Clear console            |

---

## 📄 License

Licensed under the [MIT License](LICENSE).
Use it, fork it, extend it — no restrictions.

---

## ✨ Author

Built with purpose by **Kisetsu**
From bytes to brilliance 🧊

