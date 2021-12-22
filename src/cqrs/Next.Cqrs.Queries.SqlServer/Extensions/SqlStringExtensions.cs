using System.Collections.Generic;
using System.Linq;

namespace Next.Application.Cqrs.SqlServer.Extensions
{
    internal static class SqlStringExtensions
    {
        public static IEnumerable<string> SelectToQuotedColumns(
            this IEnumerable<string> columns,
            string quotedIdentifierPrefix,
            string quotedIdentifierSuffix)
            => columns.Select(x => GetQuotedColumn(quotedIdentifierPrefix, quotedIdentifierSuffix, x));

        public static IEnumerable<string> SelectToUpdateQuotedColumnsByParameters(
            this IEnumerable<string> columns,
            string quotedIdentifierPrefix,
            string quotedIdentifierSuffix)
            => columns.Select(
                x => $"{GetQuotedColumn(quotedIdentifierPrefix, quotedIdentifierSuffix, x)} = {GetParameter(x)}");

        public static IEnumerable<string> SelectToSqlParameters(this IEnumerable<string> columns)
            => columns.Select(x => $"@{x}");

        public static string JoinToSql(this IEnumerable<string> columns, string separator = ", ")
            => string.Join(separator, columns);

        private static string GetParameter(string value)
            => $"@{value}";

        private static string GetQuotedColumn(
            string quotedIdentifierPrefix,
            string quotedIdentifierSuffix,
            string value)
            => $"{quotedIdentifierPrefix}{value}{quotedIdentifierSuffix}";
    }
}