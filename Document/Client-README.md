# 🧠 ProtonDB.Client

**ProtonDB.Client** is a lightweight .NET library that allows you to connect and interact with a ProtonDB server using simple query commands — just like working with MySQL or SQLite connectors.

> 🔌 Plug it into your apps, CLI tools, or services to send queries and retrieve results from ProtonDB over TCP.

---

## 🚀 Features

* Connect to a running ProtonDB server via TCP
* Send queries, fetch results, and handle errors easily
* Supports login with privilege-based access
* Fetch full result sets or individual entries
* Toggle debug logs
* Gracefully close connections
* Easily serializable JSON-based responses

---

## 📦 Installation

If published to NuGet:

```bash
dotnet add package ProtonDB.Client
```

Or reference it manually in your project:

```xml
<ProjectReference Include="path/to/ProtonDB.Client.csproj" />
```

---

## 🛠️ Usage

```csharp
using ProtonDB.Client;

var conn = Connection.Connect("127.0.0.1", 9090, "admin", "yourpassword");
var cursor = new Cursor(conn);

// Send a query
cursor.Query("select(name: 'Alice')");

// Fetch all results
var results = cursor.FetchAll();
foreach (var row in results) {
    Console.WriteLine(row);
}

// Fetch one result
var single = cursor.FetchOne();

// Map result to object
var student = cursor.Map<Student>();

// Quit session
cursor.Quit();
```

---

## 🔐 Authentication

Login is handled via:

```csharp
Connection.Connect("127.0.0.1", 9090, "username", "password");
```

* Credentials are never sent in plain text — only a checksum is verified on the server.
* Privileges (guest, user, admin) restrict access to certain commands or databases.

---

## 🧪 API Summary

### `Connection`

* `Connect(host, port, username?, password?)` — opens session and logs in
* `Reconnect()` — reestablishes dropped connection
* `Dispose()` — safely closes the session

### `Cursor`

* `Query(string)` — submits a query
* `SafeQuery(string)` — submits a query and auto-reconnects if disconnected
* `FetchAll()` → `string[]` — gets all results
* `FetchOne()` → `string` — gets the first result
* `Map<T>()` → `T` — deserializes one result to object
* `MapAll<T>()` → `T[]` — deserializes all results
* `Debug(bool)` — enables or disables debug logs
* `Profile()` — gets details about the current profile 
* `Quit()` — clean disconnect

---

## 🧩 Requirements

* .NET 6.0 or later
* Running instance of [ProtonDB.Server](https://github.com/Kisetsu15/ProtonDB)

---

## 🛡️ Security Notes

* No raw SQL or command strings are interpreted by the client.
* Authentication uses checksum verification with salted hashes.

---

## 📄 License

MIT — free to use, modify, or distribute.

---

## 🤖 Author

Created by Kisetsu (Dharshik)