// -------------------------------------------------------------------------------------------------
//  File: Document.cs
//  Namespace: ProtonDB.Server.Core
//  Description:
//      Provides static methods for document-level operations within a collection, including
//      insert, remove, print (query), and update. Supports conditional and unconditional
//      operations, and parses user arguments for flexible document manipulation. Integrates
//      with StorageEngine for persistence and uses Parser utilities for argument parsing.
//
//  Public Methods:
//      - Insert: Inserts a document into a collection.
//      - Remove: Removes documents from a collection, with optional condition support.
//      - Print: Retrieves documents from a collection, with optional condition support.
//      - Update: Updates documents in a collection, supporting actions (add, drop, alter)
//        and optional conditions.
//
//  Internal Methods:
//      - ParseUpdateArgument: Parses update command arguments into action, data, and condition.
//      - ConditionParser: Parses a condition string into key, value, and condition operator.
//      - GetAction: Maps string to Action enum.
//      - GetCondition: Maps string to Condition enum.
//
//  Dependencies:
//      - Query: Represents a parsed user query (object, argument, etc.).
//      - QuerySession: Represents the current user session and context.
//      - StorageEngine: Handles low-level document operations.
//      - Parser: Provides parsing utilities for conditions and actions.
//      - Kisetsu.Utils: Utility extensions (e.g., Strip).
//
//  Usage Example:
//      var result = Document.Insert(query, session);
// -------------------------------------------------------------------------------------------------

using Kisetsu.Utils;
using System.Text;
using System.Text.RegularExpressions;
using static ProtonDB.Server.Core.Parser;

namespace ProtonDB.Server {
    namespace Core {

        /// <summary>
        /// Provides static methods for document-level operations within a collection.
        /// </summary>
        public static class Document {
            /// <summary>
            /// Inserts a document into the specified collection.
            /// </summary>
            /// <param name="query">The query containing the collection and document data.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Status or error messages.</returns>
            public static string[] Insert(Query query, QuerySession session) {
                if (query.Argument == null) {
                    return ["Insert requires a document argument"];
                }

                Result result = StorageEngine.Link(
                    new QueryConfig {
                        databaseName = session.CurrentDatabase,
                        collectionName = query.Object,
                        data = query.Argument
                    },
                    StorageEngine.insert_document
                );

                return result.GetOutput();
            }

            /// <summary>
            /// Removes documents from the specified collection.
            /// If no argument is provided, removes all documents; otherwise, removes documents matching the condition.
            /// </summary>
            /// <param name="query">The query containing the collection and optional condition.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Status or error messages.</returns>
            public static string[] Remove(Query query, QuerySession session) {
                Result result = new();
                if (query.Argument == null) {
                    result = StorageEngine.Link(
                        new QueryConfig {
                            databaseName = session.CurrentDatabase,
                            collectionName = query.Object,
                        },
                        StorageEngine.remove_all_documents
                    );
                    return result.GetOutput();
                }
                var condition = ConditionParser(query.Argument);

                if (condition == null) {
                    return ["Invalid condition format. Use: key<condition>value"];
                }

                result = StorageEngine.Link(
                        new QueryConfig {
                            databaseName = session.CurrentDatabase,
                            collectionName = query.Object,
                            key = condition.Value.key,
                            value = condition.Value.value,
                            condition = condition.Value.condition
                        },
                        StorageEngine.remove_documents
                    );
                return result.GetOutput();
            }

            /// <summary>
            /// Retrieves documents from the specified collection.
            /// If no argument is provided, prints all documents; otherwise, prints documents matching the condition.
            /// </summary>
            /// <param name="query">The query containing the collection and optional condition.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Document data or error messages.</returns>
            public static string[] Print(Query query, QuerySession session) {
                Result result = new();
                if (query.Argument == null) {
                    result = StorageEngine.Link(
                        new QueryConfig {
                            databaseName = session.CurrentDatabase,
                            collectionName = query.Object
                        },
                        StorageEngine.print_all_documents
                    );
                    return result.GetOutput();
                }

                var condition = ConditionParser(query.Argument);
                if (condition == null) {
                    return ["Invalid condition format. Use: key<condition>value "];
                }

                result = StorageEngine.Link(
                        new QueryConfig {
                            databaseName = session.CurrentDatabase,
                            collectionName = query.Object,
                            key = condition.Value.key,
                            value = condition.Value.value,
                            condition = condition.Value.condition
                        },
                        StorageEngine.print_documents
                    );
                return result.GetOutput();
            }

            /// <summary>
            /// Updates documents in the specified collection.
            /// Supports actions (add, drop, alter) and optional conditions.
            /// </summary>
            /// <param name="query">The query containing the collection, update data, and optional condition.</param>
            /// <param name="session">The current query session.</param>
            /// <returns>Status or error messages.</returns>
            public static string[] Update(Query query, QuerySession session) {
                if (query.Argument == null) {
                    return ["Update requires a document argument"];
                }
                string argument = query.Argument;
                if (!argument.Contains('{') || !argument.Contains('}')) {
                    return ["Invalid update format. data must be a object like {\"key\": value}"];
                }
                var component = ParseUpdateArgument(argument, out string[] error);
                if (component == null) return error;

                Result result = new();
                Action action = component.Value.action;
                string data = component.Value.data;
                if (component.Value.condition == null) {
                    result = StorageEngine.Link(
                        new QueryConfig {
                            databaseName = session.CurrentDatabase,
                            collectionName = query.Object,
                            data = data,
                            action = action
                        },
                        StorageEngine.update_all_documents
                    );
                    return result.GetOutput();
                }
                var condition = ConditionParser(component.Value.condition);
                if (condition == null) {
                    return ["Invalid condition format. Use: key<condition>value "];
                }

                result = StorageEngine.Link(
                    new QueryConfig {
                        databaseName = session.CurrentDatabase,
                        collectionName = query.Object,
                        key = condition.Value.key,
                        value = condition.Value.value,
                        condition = condition.Value.condition,
                        action = action,
                        data = data
                    },
                    StorageEngine.update_documents
                );
                return result.GetOutput();
            }

            /// <summary>
            /// Represents the parsing state for update arguments.
            /// </summary>
            private enum State {
                Action,
                Token,
                Data,
                Condition
            };

            /// <summary>
            /// Parses the update argument into action, data, and optional condition.
            /// </summary>
            /// <param name="argument">The update argument string.</param>
            /// <param name="error">Output error messages if parsing fails.</param>
            /// <returns>Tuple of action, data, and optional condition; or null if invalid.</returns>
            private static (Action action, string data, string? condition)? ParseUpdateArgument(string argument, out string[] error) {
                error = [];
                State state = State.Action;
                StringBuilder action = new(), data = new(), condition = new();
                Stack<char> brackets = new();
                foreach (char i in argument) {
                    switch (state) {
                        case State.Action:
                            if (i == ',') {
                                state = State.Data;
                            } else if (i == ' ') {
                                continue;
                            } else if (i != ' ') {
                                action.Append(i);
                                state = State.Token;
                            } else {
                                action.Append(i);
                            }
                            break;
                        case State.Token:
                            if (i == ',') {
                                state = State.Data;
                            } else {
                                action.Append(i);
                            }
                            break;
                        case State.Data:
                            if (i == '{') {
                                brackets.Push(i);
                                data.Append(i);
                            } else if (i == '}') {
                                if (brackets.Count > 0 && brackets.Peek() == '{') brackets.Pop();
                                data.Append(i);
                            } else if (i == ',' && brackets.Count == 0) {
                                state = State.Condition;
                            } else if (i == ' ' && brackets.Count == 0) {
                                continue;
                            } else {
                                data.Append(i);
                            }
                            break;
                        case State.Condition:
                            if (i == ' ') {
                                continue;
                            } else {
                                condition.Append(i);
                            }
                            break;
                    }
                }

                Action actionInstance = GetAction(action.ToString().Trim());
                if (actionInstance == Action.invalid) {
                    error = ["Invalid action. Use: add, drop, alter"];
                    return null;
                }

                if (state == State.Condition && brackets.Count == 0) return (actionInstance, data.ToString(), condition.ToString());

                if (state == State.Data && brackets.Count == 0) return (actionInstance, data.ToString(), null);

                error = ["Invalid update format. Use: action,data,condition or action,data"];
                return null;
            }

            /// <summary>
            /// Parses a condition string into key, value, and condition operator.
            /// </summary>
            /// <param name="argument">The condition string.</param>
            /// <returns>Tuple of key, value, and condition; or null if invalid.</returns>
            private static (string key, string value, Condition condition)? ConditionParser(string argument) {
                argument = argument.Strip(' ');
                var match = Regex.Match(argument, @"^(?<key>\w+)(?<condition>>=|<=|=|>|<)(?<value>.+)$");

                if (!match.Success)
                    return null;

                var key = match.Groups["key"].Value;
                var value = match.Groups["value"].Value.Trim('"');
                var condition = GetCondition(match.Groups["condition"].Value);
                if (condition == Condition.invalid) return null;
                return (key, value, condition);
            }

            /// <summary>
            /// Maps a string to the corresponding Action enum value.
            /// </summary>
            /// <param name="action">The action string.</param>
            /// <returns>The Action enum value.</returns>
            private static Action GetAction(string action) => action switch {
                "add" => Action.add,
                "drop" => Action.drop,
                "alter" => Action.alter,
                _ => Action.invalid
            };

            /// <summary>
            /// Maps a string to the corresponding Condition enum value.
            /// </summary>
            /// <param name="op">The condition operator string.</param>
            /// <returns>The Condition enum value.</returns>
            private static Condition GetCondition(string op) => op switch {
                "=" => Condition.equal,
                ">" => Condition.greaterThan,
                "<" => Condition.lessThan,
                ">=" => Condition.greaterThanEqual,
                "<=" => Condition.lessThanEqual,
                _ => Condition.invalid,
            };

        }
    }
}