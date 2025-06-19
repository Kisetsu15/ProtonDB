using Kisetsu.Utils;
using System.Text.RegularExpressions;

namespace MicroDB {

    public static class Parser {
        public record Query(string Object, string Action, string? Argument);

        public static void Execute(string input) {
            var query = Parse(input);
            if (query == null) {
                Terminal.WriteLine("Invalid input", ConsoleColor.Red);
                return;
            }

            switch (query.Object) {
                case Clause._database:
                    ExecuteDatabaseCommand(query);
                    break;
                case Clause.collection:
                    ExecuteCollectionCommand(query);
                    break;
                default:
                    ExecuteDocumentCommand(query);
                    break;
            }
        }

        private static Query? Parse(string input) {
            var match = Regex.Match(input, pattern: @"^(?<object>\w+)\.(?<action>\w+)\((?<argument>.*)\)$");
            if (!match.Success) return null;

            return new Query(
                match.Groups[Clause.@object].Value,
                match.Groups[Clause.action].Value,
                string.IsNullOrWhiteSpace(match.Groups[Clause.argument].Value) ? null : match.Groups[Clause.argument].Value.Trim('"')
            );
        }

        private static void ExecuteDatabaseCommand(Query query) {
            switch (query.Action) {
                case Clause.use:    Database.Use(query.Argument!); break;
                case Clause.create: Database.Create(query.Argument!); break;
                case Clause.drop:   Database.Drop(query.Argument!); break;
                case Clause.list:   Database.List(); break;
                default: Terminal.WriteLine("Invalid database action", ConsoleColor.Red); break;
            }
        }

        private static void ExecuteCollectionCommand(Query query) {
            switch (query.Action) {
                case Clause.create: Collection.Create(query.Argument!); break;
                case Clause.drop:   Collection.Drop(query.Argument!); break;
                case Clause.list:   Collection.List(); break;
                default: ExecuteDocumentCommand(query); break;
            }
        }
        private static void ExecuteDocumentCommand(Query query) {
            switch (query.Action) {
                case Clause.insert: Document.Insert(query); break;
                case Clause.remove: Document.Remove(query); break;
                case Clause.update: Document.Update(query); break;
                case Clause.print:  Document.Print(query); break;
                default: Terminal.WriteLine("Invalid collection action", ConsoleColor.Red); break;
            }
        }



    }
}
