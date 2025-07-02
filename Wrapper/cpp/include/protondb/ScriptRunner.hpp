// wrapper/include/protondb/ScriptRunner.hpp
#pragma once

#include <functional>
#include <string>
#include <istream>
#include "protondb/Connection.hpp"
#include "protondb/Exception.hpp"

namespace protondb {

/// Executes a sequence of ProtonDB commands from a file or input stream.
/// Reports script‐level errors via a user‐supplied callback.
class ScriptRunner {
public:
    /// Signature for script error callbacks.
    /// line: the text of the command that failed
    /// err:  the exception thrown while processing that line
    using ErrorHandler = std::function<void(const std::string& line,
                                            const ProtonException& err)>;

    /// Construct a runner bound to an authenticated Connection.
    explicit ScriptRunner(Connection& conn);

    ScriptRunner(const ScriptRunner&) = delete;
    ScriptRunner& operator=(const ScriptRunner&) = delete;
    ScriptRunner(ScriptRunner&&) = default;
    ScriptRunner& operator=(ScriptRunner&&) = default;

    /// Execute each non‐empty, non‐comment line from the given file.
    /// Throws ScriptParseError if the file cannot be opened.
    /// Any line that fails will invoke the error handler.
    void executeScript(const std::string& filename);

    /// Execute each non‐empty, non‐comment line read from the input stream.
    /// Lines are delimited by '\n'. Errors are reported via the handler.
    void executeStream(std::istream& input);

    /// Install a callback invoked on per‐line errors.
    /// By default, exceptions are rethrown immediately.
    void onScriptError(ErrorHandler handler);

private:
    Connection&   conn_;
    ErrorHandler  errorHandler_;

    /// Process a single command line: send via Connection, ignore blank/comment.
    void processLine_(const std::string& line);
};

} // namespace protondb
