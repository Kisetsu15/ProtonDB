using Kisetsu.Utils;
using System.Text;
using System.Text.RegularExpressions;
using static ProtonDB.Parser;

namespace ProtonDB {
    public static class Document {
        public static void Insert(Query query) {
            if (query.Argument == null) {
                Terminal.WriteLine("Insert requires a document argument");
                return;
            }
            StorageEngine.InsertDocument(Master.CurrentDatabase, query.Object, query.Argument);
        }

        public static void Remove(Query query) {
            if (query.Argument == null) {
                StorageEngine.DeleteAllDocuments(Master.CurrentDatabase, query.Object);
                return;
            }
            var condition = ConditionParser(query.Argument.Strip(' '));
            if (condition == null) {
                Terminal.WriteLine("Invalid condition format. Use: key<condition>value ");
                return;
            }
            StorageEngine.DeleteDocuments(Master.CurrentDatabase, query.Object, condition.Value.key, condition.Value.value, condition.Value.condition);
        }

        public static void Print(Query query) {
            if (query.Argument == null) {
                StorageEngine.PrintAllDocuments(Master.CurrentDatabase, query.Object);
                return;
            }

            var condition = ConditionParser(query.Argument.Strip(' '));
            if (condition == null) {
                Terminal.WriteLine("Invalid condition format. Use: key<condition>value ");
                return;
            }
            StorageEngine.PrintDocuments(Master.CurrentDatabase, query.Object, condition.Value.key, condition.Value.value, condition.Value.condition);
        }
            
        public static void Update(Query query) {
            if (query.Argument == null) {
                Terminal.WriteLine("Update requires a condition argument");
                return;
            }
            string argument = query.Argument;
            if (!argument.Contains('{') || !argument.Contains('}')) {
                Terminal.WriteLine("Invalid update format. data must be a object like {\"key\": value}");
                return;
            }
            var component = ParseUpdateArgument(argument);
            if (component == null) return;

            Action action = component.Value.action;
            string data = component.Value.data;
            if (component.Value.condition == null) {
                StorageEngine.UpdateAllDocuments(Master.CurrentDatabase, query.Object, action, data);
                return;
            }
            var condition = ConditionParser(component.Value.condition);
            if (condition == null) {
                Terminal.WriteLine("Invalid condition format. Use: key<condition>value ");
                return;
            }
            StorageEngine.UpdateDocuments(Master.CurrentDatabase, query.Object, condition.Value.key, condition.Value.value, condition.Value.condition, action, data);
        }

        private enum State {
            Action,
            Data,
            Condition
        };

        private static (Action action, string data, string? condition)? ParseUpdateArgument(string argument, bool filterEnabled = true) {
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

            if (state == State.Condition && brackets.Count == 0) return (GetAction(action.ToString()), data.ToString(), condition.ToString());

            if (state == State.Data && brackets.Count == 0) return (GetAction(action.ToString()), data.ToString(), null);

            Terminal.WriteLine("Invalid update format. Use: action,data,condition or action,data");
            return null;
        }

        private static string Strip(this string str, params char[] removeChars) {
            if (string.IsNullOrEmpty(str)) return str;

            var removeSet = new HashSet<char>(removeChars);
            var sb = new StringBuilder(str.Length);
            foreach (var c in str) {
                if (!removeSet.Contains(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        private static (string key, string value, Condition condition)? ConditionParser(string argument) {
            argument = argument.Strip(' ');
            var match = Regex.Match(argument, @"^(?<key>\w+)(?<condition>>=|<=|=|>|<)(?<value>.+)$");

            if (!match.Success)
                return null;

            var key = match.Groups["key"].Value;
            var value = match.Groups["value"].Value.Trim('"');
            var condition = GetCondition(match.Groups["condition"].Value);
            return (key, value, condition);
        }

        private static Action GetAction(string action) => action switch {
            "add" => Action.add,
            "drop" => Action.drop,
            "alter" => Action.alter,
            _ => throw new ArgumentException($"Invalid action: {action}")
        };

        private static Condition GetCondition(string op) => op switch {
            "=" => Condition.equal,
            ">" => Condition.greaterThan,
            "<" => Condition.lessThan,
            ">=" => Condition.greaterThanEqual,
            "<=" => Condition.lessThanEqual,
            _ => throw new ArgumentException($"Invalid condition operator: {op}")
        };

    }
}
