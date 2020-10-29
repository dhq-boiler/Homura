using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Syntaxes;
using Homura.QueryBuilder.Vendor.SQLite.Dml.Syntaxes;
using System.Data;
using Homura.QueryBuilder.Iso.Dml;

namespace Homura.QueryBuilder.Vendor.SQLite.Dml
{
    public static class Extensions
    {
        public static ILimitSyntax Limit(this ICloseSyntax<IConditionValueSyntax> syntax, int count)
        {
            return new LimitSyntax(syntax as SyntaxBase, count);
        }

        public static ILimitSyntax Limit(this IConditionValueSyntax syntax, int count)
        {
            return new LimitSyntax(syntax as SyntaxBase, count);
        }

        public static ILimitSyntax Limit(this IOrderByColumnSyntax syntax, int count)
        {
            return new LimitSyntax(syntax as SyntaxBase, count);
        }

        public static ILimitSyntax Limit(this IRowSyntax syntax, int count)
        {
            return new LimitSyntax(syntax as SyntaxBase, count);
        }

        public static ILimitSyntax Limit(this ISinkStateSyntax syntax, int count)
        {
            return new LimitSyntax(syntax as SyntaxBase, count);
        }

        public static ILimitSyntax Limit(this IValueExpressionSyntax syntax, int count)
        {
            return new LimitSyntax(syntax as SyntaxBase, count);
        }

        public static ILimitSyntax Limit(this ISetClauseValueSyntax syntax, int count)
        {
            return new LimitSyntax(syntax as SyntaxBase, count);
        }

        public static ILimitSyntax Limit(this IJoinConditionSyntax syntax, int count)
        {
            return new LimitSyntax(syntax as SyntaxBase, count);
        }

        public static void SetParameters(this IInsertOrReplaceSyntax query, IDbCommand command)
        {
            (query as SyntaxBase).SetParameters(command);
        }
    }
}
