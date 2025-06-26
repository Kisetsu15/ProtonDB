using System.Text.RegularExpressions;

namespace ProtonDB.Server {
    namespace Core {
        public static class Parser {
            public record Query(string Object, string Operation, string? Argument);

            public static string[] Execute(string input) {
                var query = Parse(input);
                if (query == null) {
                    return ["Invalid input"];
                }

                return query.Object switch {
                    Token._database => ExecuteDatabaseCommand(query),
                    Token.collection => ExecuteCollectionCommand(query),
                    Token.profile => ExecuteProfileCommand(query),
                    _ => ExecuteDocumentCommand(query)
                };
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

            private static string[] ExecuteDatabaseCommand(Query query) {
                return query.Operation switch {
                    Token.use => Database.Use(query.Argument!),
                    Token.create => Database.Create(query.Argument!),
                    Token.drop => Database.Drop(query.Argument!),
                    Token.list => Database.List(),
                    _ => ["Invalid database command"]
                };
            }

            private static string[] ExecuteCollectionCommand(Query query) {
                return query.Operation switch {
                    Token.create => Collection.Create(query.Argument!),
                    Token.drop => Collection.Drop(query.Argument!),
                    Token.list => Collection.List(query.Argument!),
                    _ => ["Invalid collection command"]
                };
            }
            private static string[] ExecuteDocumentCommand(Query query) {
                return query.Operation switch {
                    Token.insert => Document.Insert(query),
                    Token.remove => Document.Remove(query),
                    Token.update => Document.Update(query),
                    Token.print => Document.Print(query),
                    _ => ["Invalid document command"]
                };
            }

            private static string[] ExecuteProfileCommand(Query query) {
                return query.Operation switch {
                    Token.create => Profiles.Create(query.Argument!),
                    Token.drop => Profiles.Delete(query.Argument!),
                    Token.list => Profiles.List(),
                    _ => ["Invalid profile command"]
                };
            }
        }
    }
}
