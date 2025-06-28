using System.Text.RegularExpressions;

namespace ProtonDB.Server {
    namespace Core {
        public static class Parser {
            public record Query(string Object, string Operation, string? Argument);

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

            private static Query? Parse(string input) {
                var match = Regex.Match(input, pattern: @"^(?<object>\w+)\.(?<operation>\w+)\((?<argument>.*)\)$");
                if (!match.Success) return null;

                return new Query(
                    match.Groups[Entity.@object].Value,
                    match.Groups[Entity.operation].Value,
                    string.IsNullOrWhiteSpace(match.Groups[Entity.argument].Value) ? null : match.Groups[Entity.argument].Value.Trim('"')
                );
            }

            private static string[] ExecuteDatabaseCommand(Query query, QuerySession s) {
                return query.Operation switch {
                    Token.use => Database.Use(query.Argument!, s),
                    Token.create => Database.Create(query.Argument!, s),
                    Token.drop => Database.Drop(query.Argument!, s),
                    Token.list => Database.List(),
                    _ => ["Invalid database command"]
                };
            }

            private static string[] ExecuteCollectionCommand(Query query, QuerySession s) {
                return query.Operation switch {
                    Token.create => Collection.Create(query.Argument!, s),
                    Token.drop => Collection.Drop(query.Argument!, s),
                    Token.list => Collection.List(query.Argument!, s),
                    _ => ["Invalid collection command"]
                };
            }
            private static string[] ExecuteDocumentCommand(Query query, QuerySession s) {
                return query.Operation switch {
                    Token.insert => Document.Insert(query, s),
                    Token.remove => Document.Remove(query, s),
                    Token.update => Document.Update(query, s),
                    Token.print => Document.Print(query, s),
                    _ => ["Invalid document command"]
                };
            }

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
