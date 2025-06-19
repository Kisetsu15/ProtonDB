using Kisetsu.Utils;
using System.Text;
using System.Text.RegularExpressions;
using static MicroDB.Parser;

namespace MicroDB {
    public static class Document {
        public static void Insert(Query query) {
            if (query.Argument == null) {
                Console.WriteLine("Insert requires a document argument");
                return;
            }
            StorageEngine.InsertDocument(Master.currentDatabase, query.Object, query.Argument);
        }

        public static void Remove(Query query) {
            if (query.Argument == null) {
                StorageEngine.DeleteAllDocuments(Master.currentDatabase, query.Object);
                return;
            }
            var data = ConditionParser(query.Argument.Strip(' '));
            if (data == null) {
                Console.WriteLine("Invalid condition format. Use: key<condition>value ");
                return;
            }
            StorageEngine.DeleteDocuments(Master.currentDatabase, query.Object, data.Value.key, data.Value.value, data.Value.condition);
        }

        public static void Print(Query query) {
            if (query.Argument == null) {
                StorageEngine.PrintAllDocuments(Master.currentDatabase, query.Object);
                return;
            }

            var data = ConditionParser(query.Argument.Strip(' '));
            if (data == null) {
                Console.WriteLine("Invalid condition format. Use: key<condition>value ");
                return;
            }
            StorageEngine.PrintDocuments(Master.currentDatabase, query.Object, data.Value.key, data.Value.value, data.Value.condition);
        }

        public static string Strip(this string str, params char[] removeChars) {
            if (string.IsNullOrEmpty(str)) return str;

            var removeSet = new HashSet<char>(removeChars);
            var sb = new StringBuilder(str.Length);
            foreach (var c in str) {
                if (!removeSet.Contains(c)) sb.Append(c);
            }
            return sb.ToString();
        }

        private static (string key, string value, Condition condition)? ConditionParser(string argument) {
            var match = Regex.Match(argument, @"^(?<key>\w+)(?<condition>>=|<=|=|>|<)(?<value>.+)$");

            if (!match.Success)
                return null;

            var key = match.Groups["key"].Value;
            var value = match.Groups["value"].Value.Trim('"');
            var condition = GetCondition(match.Groups["condition"].Value);
            return (key, value, condition);
        }

        private static Condition GetCondition(string op) => op switch {
            "=" => Condition.equal,
            ">" => Condition.greaterThan,
            "<" => Condition.lessThan,
            ">=" => Condition.greaterThanEqual,
            "<=" => Condition.lessThanEqual,
            _ => throw new ArgumentException($"Invalid condition operator: {op}")
        };

        public static void Update(Query query) {
            throw new NotImplementedException();
        }
    }
}
