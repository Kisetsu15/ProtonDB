using Kisetsu.Utils;
using System.Text.RegularExpressions;

namespace MicroDB {

    public static class Parser {
        public record Query(string Object, string Operation, string? Argument);

        public static void Execute(string input) {
            var query = Parse(input);
            if (query == null) {
                Terminal.WriteLine("Invalid input", ConsoleColor.Red);
                return;
            }

            switch (query.Object) {
                case Token._database:  ExecuteDatabaseCommand(query);   break;
                case Token.collection: ExecuteCollectionCommand(query); break;
                default:    ExecuteDocumentCommand(query);  break;
            }
        }

        private static Query? Parse(string input) {
            var match = Regex.Match(input, pattern: @"^(?<object>\w+)\.(?<operation>\w+)\((?<argument>.*)\)$");
            if (!match.Success) return null;

            return new Query(
                match.Groups[Token.@object].Value,
                match.Groups[Token.operation].Value,
                string.IsNullOrWhiteSpace(match.Groups[Token.argument].Value) ? null : match.Groups[Token.argument].Value.Trim('"')
            );
        }

        private static void ExecuteDatabaseCommand(Query query) {
            switch (query.Operation) {
                case Token.use:    Database.Use(query.Argument!);    break;
                case Token.create: Database.Create(query.Argument!); break;
                case Token.drop:   Database.Drop(query.Argument!);   break;
                case Token.list:   Database.List();                  break;
                default:    Terminal.WriteLine("Invalid database operation", ConsoleColor.Red); break;
            }
        }

        private static void ExecuteCollectionCommand(Query query) {
            switch (query.Operation) {
                case Token.create: Collection.Create(query.Argument!); break;
                case Token.drop:   Collection.Drop(query.Argument!);   break;
                case Token.list:   Collection.List();                  break;
                default:    Terminal.WriteLine("Invalid collection operation", ConsoleColor.Red); break;
            }
        }
        private static void ExecuteDocumentCommand(Query query) {
            switch (query.Operation) {
                case Token.insert: Document.Insert(query); break;
                case Token.remove: Document.Remove(query); break;
                case Token.update: Document.Update(query); break;
                case Token.print:  Document.Print(query);  break;
                default:    Terminal.WriteLine("Invalid document operation", ConsoleColor.Red); break;
            }
        }
    }
}
