using Kisetsu.Utils;
using System.Text;
using System.Text.RegularExpressions;
using static ProtonDB.Server.Core.Parser;

namespace ProtonDB.Server {
    namespace Core {

        public static class Document {
            public static string[] Insert(Query query) {
                if (query.Argument == null) {
                    return ["Insert requires a document argument"];
                }

                Result result = StorageEngine.Link(
                    new QueryConfig {
                        databaseName = Meta.CurrentDatabase,
                        collectionName = query.Object,
                        data = query.Argument
                    },
                    StorageEngine.insert_document
                );

                return result.GetOutput();
            }

            public static string[] Remove(Query query) {
                Result result = new();
                if (query.Argument == null) {
                    result = StorageEngine.Link(
                        new QueryConfig {
                            databaseName = Meta.CurrentDatabase,
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
                            databaseName = Meta.CurrentDatabase,
                            collectionName = query.Object,
                            key = condition.Value.key,
                            value = condition.Value.value,
                            condition = condition.Value.condition
                        },
                        StorageEngine.remove_documents
                    );
                return result.GetOutput();
            }

            public static string[] Print(Query query) {
                Result result = new();
                if (query.Argument == null) {
                    result = StorageEngine.Link(
                        new QueryConfig {
                            databaseName = Meta.CurrentDatabase,
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
                            databaseName = Meta.CurrentDatabase,
                            collectionName = query.Object,
                            key = condition.Value.key,
                            value = condition.Value.value,
                            condition = condition.Value.condition
                        },
                        StorageEngine.print_documents
                    );
                return result.GetOutput();
            }

            public static string[] Update(Query query) {
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
                            databaseName = Meta.CurrentDatabase,
                            collectionName = query.Object,
                            data = data,
                            action = action
                        },
                        StorageEngine.remove_documents
                    );
                    return result.GetOutput();
                }
                var condition = ConditionParser(component.Value.condition);
                if (condition == null) {
                    return ["Invalid condition format. Use: key<condition>value "];
                }

                result = StorageEngine.Link(
                    new QueryConfig {
                        databaseName = Meta.CurrentDatabase,
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

            private enum State {
                Action,
                Token,
                Data,
                Condition
            };

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

            private static Action GetAction(string action) => action switch {
                "add" => Action.add,
                "drop" => Action.drop,
                "alter" => Action.alter,
                _ => Action.invalid
            };

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
