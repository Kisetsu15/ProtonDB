// -------------------------------------------------------------------------------------------------
//  File: Parser.cs
//  Namespace: ProtonDB.Server.Core
//  Description:
//      Provides static methods for parsing and executing user queries in ProtonDB. The Parser
//      interprets input strings, extracts the target object, operation, and arguments, and
//      dispatches the command to the appropriate database, collection, document, or profile
//      handler. Supports extensible command routing and error handling for invalid queries.
//
//  Public Types:
//      - Query: Record representing a parsed query with object, operation, and optional argument.
//
//  Public Methods:
//      - Execute: Parses an input string and executes the corresponding command, returning the result.
//
//  Internal Methods:
//      - Parse: Uses regular expressions to extract the object, operation, and argument from the input string.
//      - ExecuteDatabaseCommand: Handles database-level commands (use, create, drop, list).
//      - ExecuteCollectionCommand: Handles collection-level commands (create, drop, list).
//      - ExecuteDocumentCommand: Handles document-level commands (insert, remove, update, print).
//      - ExecuteProfileCommand: Handles profile-level commands (create, delete, grant, revoke, list).
//
//  Dependencies:
//      - QuerySession: Represents the current user session and context.
//      - Token: Provides string constants for command routing.
//      - Database, Collection, Document, Profiles: Command handlers for each object type.
//      - Kisetsu.Utils: Utility extensions.
//
//  Usage Example:
//      var result = Parser.Execute("database.create(mydb)", session);
// -------------------------------------------------------------------------------------------------

using Kisetsu.Utils;
using System.Text.RegularExpressions;

namespace ProtonDB.Server {
    namespace Core {
        /// <summary>
        /// Provides static methods for parsing and executing user queries in ProtonDB.
        /// </summary>
        public static class Parser {
            /// <summary>
            /// Represents a parsed query with object, operation, and optional argument.
            /// </summary>
            /// <param name="Object">The target object (e.g., database, collection).</param>
            /// <param name="Operation">The operation to perform (e.g., create, drop).</param>
            /// <param name="Argument">The argument for the operation, if any.</param>
            public record Query(string Object, string Operation, string? Argument);

            /// <summary>
            /// Parses an input string and executes the corresponding command, returning the result.
            /// </summary>
            /// <param name="input">The user input string.</param>
            /// <param name="s">The current query session.</param>
            /// <returns>Result messages from the executed command.</returns>
            public static string[] Execute(string input, QuerySession s) {
                var query = Parse(input);
                if (query == null) {
                    return ["Invalid Query"];
                }
                return query.Object switch {
                    Token._database => ExecuteDatabaseCommand(query, s),
                    Token.collection => ExecuteCollectionCommand(query, s),
                    Token.profile => ExecuteProfileCommand(query, s),
                    _ => ExecuteDocumentCommand(query, s)
                };
            }

            /// <summary>
            /// Parses the input string into a Query object using regular expressions.
            /// </summary>
            /// <param name="input">The user input string.</param>
            /// <returns>A Query object if parsing is successful; otherwise, null.</returns>
            private static Query? Parse(string input) {
                var match = Regex.Match(input, pattern: @"^(?<object>\w+)\.(?<operation>\w+)\((?<argument>.*)\)$");
                if (!match.Success) return null;

                return new Query(
                    match.Groups[Entity.@object].Value,
                    match.Groups[Entity.operation].Value,
                    string.IsNullOrWhiteSpace(match.Groups[Entity.argument].Value) ? null : match.Groups[Entity.argument].Value.Trim('"')
                );
            }

            /// <summary>
            /// Executes a database-level command based on the parsed query.
            /// </summary>
            /// <param name="query">The parsed query.</param>
            /// <param name="s">The current query session.</param>
            /// <returns>Result messages from the database command.</returns>
            private static string[] ExecuteDatabaseCommand(Query query, QuerySession s) {
                return query.Operation switch {
                    Token.use => Database.Use(query.Argument!, s),
                    Token.create => Database.Create(query.Argument!, s),
                    Token.drop => Database.Drop(query.Argument!, s),
                    Token.list => Database.List(query.Argument!),
                    _ => ["Invalid database command"]
                };
            }

            /// <summary>
            /// Executes a collection-level command based on the parsed query.
            /// </summary>
            /// <param name="query">The parsed query.</param>
            /// <param name="s">The current query session.</param>
            /// <returns>Result messages from the collection command.</returns>
            private static string[] ExecuteCollectionCommand(Query query, QuerySession s) {
                return query.Operation switch {
                    Token.create => Collection.Create(query.Argument!, s),
                    Token.drop => Collection.Drop(query.Argument!, s),
                    Token.list => Collection.List(query.Argument!, s),
                    _ => ["Invalid collection command"]
                };
            }

            /// <summary>
            /// Executes a document-level command based on the parsed query.
            /// </summary>
            /// <param name="query">The parsed query.</param>
            /// <param name="s">The current query session.</param>
            /// <returns>Result messages from the document command.</returns>
            private static string[] ExecuteDocumentCommand(Query query, QuerySession s) {
                return query.Operation switch {
                    Token.insert => Document.Insert(query, s),
                    Token.remove => Document.Remove(query, s),
                    Token.update => Document.Update(query, s),
                    Token.print => Document.Print(query, s),
                    _ => ["Invalid document command"]
                };
            }

            /// <summary>
            /// Executes a profile-level command based on the parsed query.
            /// </summary>
            /// <param name="query">The parsed query.</param>
            /// <param name="s">The current query session.</param>
            /// <returns>Result messages from the profile command.</returns>
            private static string[] ExecuteProfileCommand(Query query, QuerySession s) {
                return query.Operation switch {
                    Token.create => Profiles.Create(query.Argument!, s),
                    Token.delete => Profiles.Delete(query.Argument!, s),
                    Token.grant => Profiles.Grant(query.Argument!, s),
                    Token.revoke => Profiles.Revoke(query.Argument!, s),
                    Token.list => Profiles.List(s),
                    _ => ["Invalid profile command"]
                };
            }
        }
    }
}