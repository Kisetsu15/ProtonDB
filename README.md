# ProtonDB ⚛️

A modular, embedded NoSQL database engine built with C and C#. It includes a TCP server, shell interface, secure profile system, and multi-language client bindings.

---

## 🗃 Features

- Embedded document store (binary-based)
- C# interop via `DllImport`
- Secure profile-based access (admin/user)
- TCP server with command routing
- Python & C# client wrappers
- Built-in command-line shell
- NSSM/Windows Service ready
- Setup script via Inno Installer

---

## 🧭 Structure

| Folder                | Description                            |
|----------------------|----------------------------------------|
| `ProtonDB.Client`    | C# TCP client interface                |
| `ProtonDB.Server`    | TCP server and command router          |
| `ProtonDB.Shell`     | Terminal shell for interactive usage   |
| `ProtonDB.Service`   | Background service variant             |
| `StorageEngine`      | Core NoSQL engine (C)                  |
| `Wrapper/Python/`    | Client wrappers                        |
| `Setup/`             | Inno Setup script                      |

---

## 🚀 Getting Started

### Build the storage engine

```bash
cd StorageEngine
make
````

### Run the server

```bash
dotnet run --project ProtonDB.Server
```

Or install with `nssm` or use the installer from Releases.

---

## 🔌 Protocol

Communication is done via **UTF-8 JSON lines** over TCP:

```json
// Request
{ "command": "QUERY", "data": "document.insert(...)" }

// Response
{ "status": "ok", "message": "Saved", "result": ["..."] }
```

---

## 🔐 Auth Model

* Admins can manage all databases and profiles
* Users can access only allowed databases
* Passwords are never stored, only salted checksums

---

## 📦 Packaging

* C# Client: `ProtonDB.Client`
* Setup via Inno Installer
* Service version: `ProtonDB.Service`

---

## 🧪 Example

```csharp
var conn = Connection.Connect("localhost", 9090, "admin", "admin");
conn.Query("document.insert({ \"name\": \"Kisetsu\" })");
var data = conn.FetchAll();
```

---

## 📄 License

MIT — Free to modify and redistribute.

---

## 🤖 Author

### Kisetsu

---

## 🤝 Contributions

Fork and PR welcome. Please test against TCP + Shell modes.
