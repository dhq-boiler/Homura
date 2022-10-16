using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;
using static Homura.QueryBuilder.Iso.Dml.Syntaxes.ReplaceSyntax;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class GroupBySyntax : SyntaxBase, IGroupBySyntax
    {
        internal GroupBySyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IGroupByColumnSyntax Column(string name)
        {
            return new GroupByColumnSyntax(this, name);
        }

        public IGroupByColumnSyntax Column(string tableAlias, string name)
        {
            return new GroupByColumnSyntax(this, tableAlias, name);
        }

        public IGroupByColumnSyntax Columns(params string[] names)
        {
            IGroupByColumnSyntax ret = null;
            foreach (var name in names)
            {
                if (ret == null)
                {
                    ret = new GroupByColumnSyntax(this, name);
                }
                else
                {
                    ret = new GroupByColumnSyntax(ret as SyntaxBase, name, Delimiter.Comma);
                }
            }
            return ret;
        }

        public IGroupByColumnSyntax Columns(IEnumerable<string> names)
        {
            return Columns(names.ToArray());
        }

        public IGroupByColumnSyntax Replace(string expression, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Expression, expression, pattern, replacement);
        }

        public IGroupByColumnSyntax ReplaceColumn(string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, columnName, pattern, replacement);
        }

        public IGroupByColumnSyntax ReplaceColumn(string tableAlias, string columnName, string pattern, string replacement)
        {
            return new ReplaceSyntax(this, EExpression.Column, tableAlias, columnName, pattern, replacement);
        }

        public override string Represent()
        {
            return "GROUP BY";
        }
    }
}
