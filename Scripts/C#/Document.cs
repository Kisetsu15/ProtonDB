using Kisetsu.Utils;
using System.Text;
using System.Text.RegularExpressions;
using static MicroDB.Parser;

namespace MicroDB {
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

            string argument = query.Argument.Trim();
            if (!argument.Contains('{') || !argument.Contains('}')) {
                Terminal.WriteLine("Invalid update format. data must be a object like {\"key\": value}");
                return;
            }

            var match = Regex.Match(argument, @"^(?<action>\w+)\,\{(?<data>.+)\}\,(?<condition>.+)$");

            string action;
            string data;

            if (!match.Success) {
                match = Regex.Match(argument, @"^(?<action>\w+)\,\{(?<data>.+)\}$");
                if (!match.Success) {
                    Terminal.WriteLine("Invalid update format. Use: action,data,condition or action,data");
                    return;
                }
                action = match.Groups[Token.action].Value;
                data = match.Groups[Token.data].Value;

                data = (action == Token.drop) ? $"\"{data.Trim('"')}\"" : "{" + data + "}";

                StorageEngine.UpdateAllDocuments(Master.CurrentDatabase, query.Object, GetAction(action.Trim()), data);
                return;
            }


            action = match.Groups[Token.action].Value;
            data = match.Groups[Token.data].Value;
            var condition = ConditionParser(match.Groups[Token.condition].Value);

            data = (action == Token.drop) ? data.Strip('"') : "{" + data + "}";

            if (condition == null) {
                Terminal.WriteLine("Invalid condition format. Use: key<condition>value ");
                return;
            }

            StorageEngine.UpdateDocuments(Master.CurrentDatabase, query.Object, condition.Value.key, condition.Value.value, condition.Value.condition,
                GetAction(action.Trim()), data);

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
