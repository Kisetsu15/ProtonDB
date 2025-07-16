
# ProtonDB C++ Client API Documentation

## 1. Overview

This documentation provides a detailed overview of the **ProtonDB C++ Client**, a wrapper library designed for interacting with the **ProtonDB** via TCP. The library enables developers to manage connections, execute queries, and handle responses efficiently. It also simplifies socket management and provides an abstraction for error handling.

This client library is structured similarly to how `psycopg2` functions for PostgreSQL, but for ProtonDB using C++.

---

## 2. File Structure

```
/src
    /Connection.cpp        -> Connection management implementation.
    /Cursor.cpp            -> Query execution logic.
    /SocketHandle.cpp      -> Low-level socket management.
    /SocketIO.cpp          -> Socket I/O operations (send/recv).
    /ScriptRunner.cpp      -> Script execution (batch query processing).
    /Exception.cpp         -> Exception class implementations.

    /internal/
        /SocketHandle.cpp  -> Internal socket management utilities.
        /SocketIO.cpp      -> Internal socket I/O operations.

/include
    /protondb/
        /Connection.hpp    -> Header for connection class.
        /Cursor.hpp        -> Header for cursor class.
        /SocketHandle.hpp  -> Header for socket handle.
        /SocketIO.hpp      -> Header for socket I/O operations.
        /ScriptRunner.hpp  -> Header for script runner.
        /Exception.hpp     -> Header for exceptions.

    /tests/
        /test_connection.cpp -> Unit tests for connection handling.
        /test_cursor.cpp     -> Unit tests for query execution.
        /test_scriptRunner.cpp -> Unit tests for script execution.
        /test_exception.cpp  -> Unit tests for exception handling.

    /examples/
        /helloWorld/
            /example_basic.cpp -> Example for basic usage.
            /CMakeLists.txt     -> CMake build configuration.
        
        /library_management/
            /library.cpp       -> Example for library management system.
            /CMakeLists.txt     -> CMake build configuration.
```
## Core Classes
```
## `Connection`
Manages TCP/TLS connection, login, retries.

- `Connect(host, port, user, pass)`
- `close()`, `isConnected()`
- `sendLine()`, `readLine()`
- `setTimeout()`, `setRetries()`, `enableTLS()`

## `Cursor`
Executes queries, fetches responses.

- `execute(command)`
- `executeRaw(json)`
- `fetch()`
- `response()`, `result()`, `status()`, `message()`

## `SocketHandle`
RAII socket wrapper.

- `fd()`, `isValid()`

## `SocketIO`
Reliable socket send/recv.

- `sendAll()`
- `readUntil()`

## `ScriptRunner`
Runs scripts, handles errors.

- `executeScript()`
- `executeStream()`
- `onScriptError()`

## Exceptions
- `ConnectionError`
- `TimeoutError`
- `ProtocolError`
- `ScriptParseError`
```
---


## 3. Connection Class

The **`Connection`** class is responsible for managing a **TCP** connection (and optionally **TLS**) to the **ProtonDB** server. It handles the **LOGIN** handshake, connection retries, timeouts, and socket communication.

### Key Methods and Members

#### **Constructor and Destructor**

- **`Connection()`**  
  Default constructor. Initializes the connection instance without connecting to a server.

- **`Connection(Connection&& other) noexcept`**  
  Move constructor. Transfers the resources (e.g., socket, user credentials) from one `Connection` instance to another.

- **`Connection& operator=(Connection&& other) noexcept`**  
  Move assignment operator. Assigns resources from one `Connection` to another while ensuring the previous connection is properly closed.

- **`~Connection()`**  
  Destructor. Ensures proper cleanup of resources by closing the connection.

#### **Static Methods**

- **`static Connection Connect(const std::string& host, int port, const std::string& username, const std::string& password)`**  
  Establishes a connection to the ProtonDB server:
  1. Resolves the server address.
  2. Creates and connects a socket.
  3. Applies timeouts and performs the `LOGIN` handshake with the server.
  
  Throws exceptions like `ConnectionError`, `TimeoutError`, or `ProtocolError` in case of failures. This method returns a `Connection` instance if successful.

#### **Connection State**

- **`void close()`**  
  Closes the connection by releasing the socket handle and cleaning up any associated resources. This includes releasing the **`SocketHandle`** resource if the connection is closed.

- **`bool isConnected() const noexcept`**  
  Checks if the connection is active and the socket is valid. Returns `true` if connected, `false` otherwise.

#### **Timeouts and Retries**

- **`void setTimeout(int timeoutMs)`**  
  Sets a unified timeout for read, write, and connect operations. This is for legacy compatibility.

- **`void setTimeouts(int connectMs, int sendMs, int recvMs)`**  
  Sets individual timeouts for connection, send, and receive operations. Timeouts are applied after the socket is successfully connected.

- **`void setRetry(int retries)`**  
  Configures the number of retry attempts to handle transient failures during send/receive operations.

- **`void enableAutoReconnect(bool enable)`**  
  Enables or disables automatic reconnection in case the connection is lost. If enabled, the client will attempt to reconnect to the server and reauthenticate if the socket is closed.

#### **TLS and Secure Communication**

- **`void enableTLS(const std::string& certPath)`**  
  A placeholder function to enable TLS support. Currently unimplemented but can be used to specify the certificate path for secure communication with the server.

#### **Sending and Receiving Data**

- **`std::string sendLine(const std::string& jsonLine)`**  
  Sends a JSON-encoded line to the server and waits for a response. If the connection is closed, it will attempt to reconnect if `autoReconnect_` is enabled. If no reconnection occurs, it throws a `ConnectionError`.

- **`std::string readLine()`**  
  Reads a single line from the server. If the connection is closed, it throws a `ConnectionError`.

#### **Login Process**

- **`bool login_(const std::string& user, const std::string& pass)`**  
  Sends a `LOGIN` request to the ProtonDB server and verifies the response. If successful, it returns `true`; otherwise, it logs a warning and returns `false`.

### Detailed Workflow: How `Connect()` Works

1. **Address Resolution**:  
   The `Connect` method first resolves the server address using `getaddrinfo()`, handling both IPv4 and IPv6 addresses.

2. **Socket Creation**:  
   A socket is created using `socket()` based on the resolved address.

3. **Connection Attempt**:  
   The connection is made using `connect()`, and the socket is wrapped in a `SocketHandle` object for proper resource management.

4. **Timeout Configuration**:  
   Timeouts for connecting, sending, and receiving are applied using `setsockopt()`.

5. **Initial Banner Read**:  
   The first line (banner) from the server is read, and any failure during this step results in a warning but not an immediate failure.

6. **Login**:  
   The client sends a `LOGIN` request and verifies the server’s response. If the login fails, the connection is closed, and a `ProtocolError` is thrown.

### Socket Management: `SocketHandle` and `SocketIO`

- The `Connection` class relies on **`SocketHandle`** (defined in `SocketHandle.hpp` and implemented in `SocketHandle.cpp`) for low-level socket management.
  - **`SocketHandle`** wraps the socket file descriptor and ensures that the socket is properly closed when the connection is terminated.
  
- The **`SocketIO`** class (referenced in `SocketIO.cpp`) provides the ability to send and receive data from the socket in a reliable manner.

#### Connection Lifecycle: A Closer Look

When a connection is established via `Connect()`, the following happens:
- A socket is created and connected to the target host/port.
- Timeouts are applied to the socket for operations like sending and receiving data.
- After a successful connection, the login process authenticates the client using the provided credentials.
- If the connection is closed or lost, it can be automatically re-established if `autoReconnect_` is enabled.

### Exception Handling

- The `Connection` class throws various exceptions in case of errors:
  - **`ConnectionError`**: Thrown for connection issues like socket failure or problems during sending/receiving data.
  - **`TimeoutError`**: Thrown if an operation exceeds the configured timeout.
  - **`ProtocolError`**: Thrown when the server’s response is malformed or violates the expected protocol.

---

## 4. Cursor Class

The **`Cursor`** class manages a live query session over an active **ProtonDB** connection. It supports executing queries (in DSL or raw JSON format), fetching results, and parsing server responses in **JSON** format.

This class operates on a **ProtonDB** connection object (`Connection`) and provides methods for sending commands to the server and handling the responses. The `Cursor` class is tightly coupled with the `Connection` class and relies on it for sending data and receiving responses.

### Key Methods and Members

#### **Constructor and Destructor**

- **`Cursor(Connection& conn)`**  
  Constructor. Binds the `Cursor` to an existing `Connection`. This connection is used for sending commands and receiving responses.

- **`~Cursor()`**  
  Destructor. The destructor doesn't need to perform any special cleanup, as the connection and its resources are managed by the `Connection` class.

#### **Query Execution**

- **`std::string execute(const std::string& command)`**  
  Executes a DSL command (e.g., `SELECT * FROM foo`) as a JSON-encoded query. The command is wrapped in a JSON object with the `"Command"` field set to `"QUERY"`, and the `"Data"` field containing the actual DSL query.
  - **Throws**: `ProtocolError` if the response is invalid or if the server responds with an error status.

- **`std::string executeRaw(const std::string& rawJson)`**  
  Executes a raw JSON command that conforms to the ProtonDB protocol. This method is useful for sending complex commands that don't follow the standard DSL format.
  - **Throws**: `ProtocolError` if the payload is empty or the response is malformed.

- **`std::string fetch()`**  
  Sends a `FETCH` command to the server to retrieve the next batch of query results. This is typically used for long-running queries or paginated results.
  - **Throws**: `ProtocolError` if the server responds with an error.

#### **Response Handling**

- **`const std::string& response() const`**  
  Returns the last raw response from the server. This is a string containing the entire response received after executing a command.

- **`std::string result() const`**  
  Extracts and returns the `result` field from the last server response (if present). This is usually the main data returned by a query.
  - **Throws**: `ProtocolError` if the `result` field is not present in the response.

- **`std::string status() const`**  
  Extracts and returns the `status` field from the last response. This field typically indicates the success or failure of the query execution.
  - **Throws**: `ProtocolError` if the `status` field is not present in the response.

- **`std::string message() const`**  
  Extracts and returns the `message` field from the last response, if present. This field contains any error message or additional information from the server.
  - Returns an empty string if the `message` field is not present.

#### **Timeouts**

- **`void setTimeouts(int connectMs, int sendMs, int recvMs)`**  
  Sets the timeouts for the connection used by the `Cursor`. These timeouts are delegated to the underlying `Connection` object.

#### **Private Helper Methods**

- **`void parseResponse_()`**  
  Parses the last response (`lastResponse_`) into a JSON object (`lastJson_`). This method also validates the protocol and throws a `ProtocolError` if the response is malformed or the server indicates an error.
  - It checks for the `status` field and validates that it is `"ok"`. If not, it throws an error with the appropriate message.

### Exception Handling

The `Cursor` class leverages custom exceptions for error handling:
- **`ProtocolError`**: Thrown for invalid responses from the server or if a required field (like `status`, `result`, etc.) is missing from the response.
- **`ConnectionError`**: Thrown for communication failures, such as when the connection is closed or sending/receiving data fails.

---

### Detailed Workflow

1. **Query Execution (`execute` and `executeRaw`)**:  
   - The query or raw JSON command is sent to the server via the `Connection`'s `sendLine` method.
   - The response is stored in `lastResponse_`, and `parseResponse_` is called to validate and parse the server's response.

2. **Fetching Results (`fetch`)**:  
   - A `FETCH` command is sent to the server to retrieve the next batch of results for an ongoing query. The response is processed in the same way as the other commands.

3. **Response Parsing (`parseResponse_`)**:  
   - The raw response is parsed into a JSON object. If the response does not contain the required fields (`status`, `result`), or if the server indicates an error, an exception is thrown.

4. **Timeout Management**:  
   - The `setTimeouts` method allows fine-grained control over the timeouts for connection, sending, and receiving data. This is useful for handling slow responses or preventing the application from hanging due to network issues.

---


## 5. Exception Handling

The **ProtonDB C++ Client** library uses custom exception classes to handle errors in a structured way. These exceptions are derived from a base exception class, **`ProtonException`**, which supports chaining of exceptions using `std::nested_exception`.

Each exception class provides a specific type of error message, allowing the user to identify the problem clearly.

### **ProtonException** (Base Class)

`ProtonException` is the base class for all exceptions in the ProtonDB client. It inherits from `std::runtime_error` and `std::nested_exception` to support exception chaining and detailed error messages.

#### Constructor

- **`ProtonException(const std::string& prefix, const std::string& message, const std::string& response = "")`**  
  Initializes the exception with a `prefix`, `message`, and optional `response`. The `prefix` is typically the name of the error type (e.g., "ConnectionError"). The `message` provides the description of the error, and the `response` is the raw server response (if available) to help with debugging.

#### Methods

- **`const std::string& response() const noexcept`**  
  Returns the server response associated with the exception (if any).


### Derived Exception Classes

The following exception classes are derived from **`ProtonException`** and represent specific error conditions:

#### **ConnectionError**

Thrown when a connection or socket-level error occurs, such as a failure in establishing or maintaining the connection to the ProtonDB server.

- **Constructor**:  
  - **`ConnectionError(const std::string& msg)`**: Initializes the exception with the given error message.

#### **TimeoutError**

Thrown when an operation exceeds the configured timeout (e.g., connect, send, or receive operations).
- **Constructor**:  
  - Inherits from `ProtonException`.

#### **ConnectTimeoutError**

A subclass of **`TimeoutError`** specifically thrown when the connection attempt exceeds the configured timeout.

- **Constructor**:  
  - Inherits from `TimeoutError`.

#### **ProtocolError**

Thrown when the server’s response is malformed or violates the expected protocol (e.g., missing fields or invalid JSON).

- **Constructor**:  
  - **`ProtocolError(const std::string& msg)`**: Initializes the exception with the given error message.

#### **ScriptParseError**

Thrown when there is an error while parsing or executing a `.pdb` script.

- **Constructor**:  
  - Inherits from `ProtonException`.



---

## 6. Socket Handling

The **ProtonDB C++ Client** provides low-level socket handling through the `SocketHandle` and `SocketIO` classes. These classes manage socket connections, ensuring that resources are properly handled, and data is transmitted reliably over the network.

### **SocketHandle** Class

The **`SocketHandle`** class is a RAII (Resource Acquisition Is Initialization) wrapper for a socket file descriptor. It ensures that the socket is properly closed when the `SocketHandle` object is destroyed.

#### Key Methods

- **`SocketHandle(socket_t fd) noexcept`**  
  Constructor that initializes the `SocketHandle` with a socket descriptor (`fd`), typically returned from the `socket()` system call.

- **`~SocketHandle()`**  
  Destructor. Ensures that the socket is closed when the `SocketHandle` object goes out of scope, preventing socket leaks.

- **`SocketHandle(SocketHandle&& other) noexcept`**  
  Move constructor that transfers ownership of the socket descriptor from another `SocketHandle` object.

- **`SocketHandle& operator=(SocketHandle&& other) noexcept`**  
  Move assignment operator that transfers ownership of the socket descriptor from one `SocketHandle` to another.

- **`socket_t fd() const noexcept`**  
  Returns the raw socket descriptor.

- **`bool isValid() const noexcept`**  
  Returns `true` if the socket descriptor is valid (i.e., not equal to `invalid_socket`), otherwise returns `false`.

#### Example Usage

```cpp
protondb::internal::SocketHandle sock(rawfd);
// Use sock.fd() to access the socket descriptor
```

### **SocketIO** Class

The **`SocketIO`** class provides functions for reliably sending and receiving data over a socket. These methods handle partial sends, retries, and errors, ensuring robust communication.

#### Key Methods

- **`bool sendAll(socket_t fd, const char* data, size_t len, int retryCount = 0)`**  
  Sends all bytes of data over the socket, handling partial sends and retries in case of transient errors (e.g., `EINTR` or `EAGAIN`).
  - **Parameters**: 
    - `fd`: The socket descriptor.
    - `data`: Pointer to the data buffer.
    - `len`: The number of bytes to send.
    - `retryCount`: The number of retry attempts for transient errors (default is 0).
  - **Returns**: `true` if all bytes are sent successfully.
  - **Throws**: `ConnectionError` if the send operation fails or the connection is closed.

- **`std::string readUntil(socket_t fd, char delimiter = '
')`**  
  Reads from the socket until a delimiter is encountered (default is the newline character `'
'`). It reads the data into a string and returns it.
  - **Parameters**: 
    - `fd`: The socket descriptor.
    - `delimiter`: The character to stop reading at (default is `'
'`).
  - **Returns**: A string containing the data read up to (but excluding) the delimiter.
  - **Throws**: `ConnectionError` if the socket is closed or if there is a system-level error during reading.

#### Example Usage

```cpp
// Sending data
const char* data = "Hello, ProtonDB!";
if (sendAll(fd, data, strlen(data), 3)) {
    std::cout << "Data sent successfully!" << std::endl;
}

// Receiving data
std::string result = readUntil(fd, '
');
std::cout << "Received: " << result << std::endl;
```

### Utility Functions

- **`std::string systemErrorString()`**  
  A utility function that returns a system error string based on the current platform. It helps convert the system-specific error codes (e.g., `WSAGetLastError()` on Windows or `errno` on Unix-based systems) into readable error messages.

- **`bool isRetryableError()`**  
  A utility function that checks if the current error is retryable. This is used by `sendAll` and `readUntil` to decide whether to retry the operation in case of errors like `EINTR` or `EAGAIN`.

### Example of Reliable Send and Receive

```cpp
protondb::internal::SocketHandle sock(fd);
const char* data = "Sample Data";
if (sendAll(sock.fd(), data, strlen(data), 3)) {
    std::cout << "Data sent successfully" << std::endl;
}

std::string response = readUntil(sock.fd(), '
');
std::cout << "Response: " << response << std::endl;
```

### Error Handling

Both `sendAll` and `readUntil` functions throw a `ConnectionError` if there is a failure. The error message is captured from system-specific error codes, providing meaningful context for the failure.

#### Common Errors:
- **`EINTR` (Windows: `WSAEINTR`)**: Interrupted system call. Typically a transient error, which can be retried.
- **`EAGAIN` (Windows: `WSAEWOULDBLOCK`)**: The operation would block. Can be retried after some delay.
- **`ECONNRESET` (Windows: `WSAECONNRESET`)**: Connection was forcibly closed by the remote host.


---

## 7. ScriptRunner Class

The **`ScriptRunner`** class is responsible for executing a sequence of commands in the **ProtonDB** server, either from a file or an input stream. This class provides a mechanism to process commands one by one and reports any errors encountered during script execution via a user-defined callback.

### Key Methods and Members

#### **Constructor and Destructor**

- **`ScriptRunner(Connection& conn)`**  
  Constructs a `ScriptRunner` object bound to an existing authenticated `Connection` instance. This connection will be used to send commands to the server.
  
- **`~ScriptRunner()`**  
  Destructor. Does not need to do anything specific as the connection is managed by the `Connection` class.

#### **Error Handling**

- **`using ErrorHandler = std::function<void(const std::string& line, const ProtonException& err)>`**  
  A type alias for a callback function that handles errors encountered while processing individual script lines. The callback takes two arguments:
  - `line`: The command line that caused the error.
  - `err`: The exception thrown while processing the line.

- **`void onScriptError(ErrorHandler handler)`**  
  Sets a callback to handle errors encountered during script execution. By default, exceptions are rethrown immediately if no handler is set.

#### **Script Execution**

- **`void executeScript(const std::string& filename)`**  
  Loads a script file and executes each valid line. Each line is processed individually, and any errors encountered will invoke the error handler (if set). If the file cannot be opened, a `ScriptParseError` is thrown.
  - **Throws**: `ScriptParseError` if the file cannot be opened or if there is an error while reading the script.

- **`void executeStream(std::istream& input)`**  
  Executes each valid line from the provided input stream. Lines are processed one by one, and any errors encountered will invoke the error handler (if set).
  - **Throws**: `ScriptParseError` if an error occurs while reading from the input stream.

#### **Private Helper Methods**

- **`void processLine_(const std::string& line)`**  
  Processes a single line of the script by trimming whitespace, ignoring comments, and sending the command to the server via the `Connection`. If the line cannot be processed (due to exceptions), the error handler is invoked if set, otherwise, the exception is rethrown.

### Detailed Workflow

1. **Script Loading (`executeScript`)**:  
   - The script file is opened and read line by line.
   - Each line is processed by calling `processLine_`.
   - Errors are handled via the `ErrorHandler` callback if set.

2. **Stream Execution (`executeStream`)**:  
   - The method reads commands from an input stream and processes each line using the same logic as `executeScript`.
   - It handles errors by invoking the callback or rethrowing exceptions.

3. **Line Processing (`processLine_`)**:  
   - Leading and trailing whitespace is trimmed from each line.
   - Comment lines (starting with `#`) are ignored.
   - The valid command is sent to the server using the `Connection`'s `sendLine` method.

4. **Error Handling**:  
   - If an error occurs while processing a line (e.g., a command fails), the exception is passed to the `ErrorHandler` callback.
   - If no callback is set, the exception is rethrown.

---

## 8. Example Usage

### Example 1: Basic Usage (`example_basic.cpp`)

This example shows how to establish a connection with the ProtonDB server, execute commands, and handle responses.

**How to Build**:

Go to the `/examples/helloWorld/` directory and then run the following:

```bash
cmake -S . -B build
cd build
make example_basic
./example_basic
```

### Example 2: Library Management System (`library.cpp`)

This example is a simple library management system using ProtonDB and the wrapper.

**How to Build**:

Go to the `/examples/library_management/` directory and then run the following:
```bash
cmake -S . -B build
cd build
make library
./library
```
---

## 9. Testing

Unit tests are located in the `/tests` directory. To build and run tests:

```bash
cmake -DTEST_NAME="connection" -S . -B build
cd build
make
./test_connection
```
Change "connection" to "cursor" or "scriptRunner" to run `./test_cursor` and `./test_scriptRunner` 

---
