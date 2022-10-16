using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;
using static Homura.QueryBuilder.Iso.Dml.Syntaxes.ReplaceSyntax;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class DistinctSyntax : SyntaxBase, ISetQuantifierSyntax
    {
        internal DistinctSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IColumnSyntax Column(string name)
        {
            return new ColumnSyntax(this, name);
        }

        public IColumnSyntax Column(string tableAlias, string name)
        {
            return new ColumnSyntax(this, tableAlias, name);
        }

        public IColumnSyntax Columns(IEnumerable<string> names)
        {
            return Columns(names.ToArray());
        }

        public IColumnSyntax Columns(params string[] names)
        {
            IColumnSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new ColumnSyntax(this, name);
                }
                else
                {
                    ret = new ColumnSyntax(ret as SyntaxBase, name);
                }
            }
            return ret;
        }

        public IColumnSyntax Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        public IColumnSyntax ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        public IColumnSyntax ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }

        public override string Represent()
        {
            return "DISTINCT";
        }
    }
}