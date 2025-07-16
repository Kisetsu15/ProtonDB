# ProtonDB ⚛️

A modular, embedded NoSQL database engine built with C and C#. It includes a TCP server, shell interface, secure profile system, and multi-language client bindings.

---

## 🗃 Features

- Embedded document store (binary-based)
- C# interop via `DllImport`
- Secure profile-based access (admin/user)
- TCP server with command routing
- Python, C++, Java client wrappers
- Built-in command-line shell
- NSSM/Windows Service ready
- Setup script via Inno Installer

---

## 🧭 Structure

| Folder               | Description                            |
|----------------------|----------------------------------------|
| `ProtonDB.Client`    | C# TCP client interface                |
| `ProtonDB.Server`    | TCP server and command router          |
| `ProtonDB.Shell`     | Terminal shell for interactive usage   |
| `ProtonDB.Service`   | Background service variant             |
| `StorageEngine`      | Core NoSQL engine (C)                  |
| `Wrapper`            | Client wrappers                        |
| `Setup`              | Inno Setup script                      |

---

## 🚀 Getting Started

### 1. Download & Install

- Run the provided installer
- The ProtonDB service starts running in the background after installation
- Use the interactive ProtonDB Shell or connect via client packages

### (OR) Build Manually

### 1. Clone ProtonDb

```
git clone https://github.com/Kisetsu15/ProtonDB
```

### 2. Build the storage engine

```
cd StorageEngine
make
````

### 3. Run the server

```
dotnet run --project ProtonDB.Service (or) ProtonDB.Server
```

Or install with `nssm` or use the installer from Releases.

---

## 💬 Query Format

All query follow the format:

> object.operation(argument)

---

## 🔌 Protocol

Communication is done via **UTF-8 JSON lines** over TCP:

```
// Request
{ "Command": "QUERY", "Data": "document.insert(...)" }

// Response
{ "Status": "ok", "Message": "Saved", "Result": ["..."] }
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

```
profile.create("john","mypassword","user")
database.create("StoreDB")
collection.create("Items")
Items.insert("{
  "item": "Notebook",
  "price": 10
}")
Items.print()
```

## Documentation

* [User Documentation](https://github.com/Kisetsu15/ProtonDB/blob/master/Document/ProtonDB_User_Documentation.pdf)
* [C# Client](https://github.com/Kisetsu15/ProtonDB/blob/master/ProtonDB.Client/README.md)
* [pyproton](https://github.com/Kisetsu15/ProtonDB/blob/master/Wrapper/Python/pyproton/README.md)
* [C++ Client](https://github.com/Kisetsu15/ProtonDB/blob/master/Wrapper/cpp/README.md)
* [Java Wrapper](https://github.com/Kisetsu15/ProtonDB/blob/master/Wrapper/Java/README.md)
  

---

## 📄 License

MIT — Free to modify and redistribute.

---

## 🤖 Author

### Kisetsu

---

## 🤝 Contributions
* pyproton by [@sarveshsarvs](https://github.com/sarveshsarvs) in https://github.com/Kisetsu15/ProtonDB/pull/4
* C++ Client by [@allanhanan](https://github.com/allanhanan) in https://github.com/Kisetsu15/ProtonDB/pull/8
* Java Wrapper by [@StellarStacker](https://github.com/StellarStacker) in https://github.com/Kisetsu15/ProtonDB/pull/6

Fork and PR welcome. Please test against TCP + Shell modes.
