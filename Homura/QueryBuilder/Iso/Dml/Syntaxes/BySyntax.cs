using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;
using static Homura.QueryBuilder.Iso.Dml.Syntaxes.ReplaceSyntax;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class BySyntax : SyntaxBase, IBySyntax
    {
        internal BySyntax(SyntaxBase syntax)
            : base(syntax)
        { }

        public ICorrespondingColumnSyntax Column(string name)
        {
            return new ColumnSyntax(this, name, Delimiter.OpenedParenthesis);
        }

        public ICorrespondingColumnSyntax Column(string tableAlias, string name)
        {
            return new ColumnSyntax(this, tableAlias, name, Delimiter.OpenedParenthesis);
        }

        public ICorrespondingColumnSyntax Columns(params string[] names)
        {
            ICorrespondingColumnSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new ColumnSyntax(this, name, Delimiter.OpenedParenthesis);
                }
                else
                {
                    ret = new ColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        public ICorrespondingColumnSyntax Columns(IEnumerable<string> names)
        {
            return Columns(names.ToArray());
        }

        public ICorrespondingColumnSyntax Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        public ICorrespondingColumnSyntax ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        public ICorrespondingColumnSyntax ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }

        public override string Represent()
        {
            return "BY";
        }
    }
}
