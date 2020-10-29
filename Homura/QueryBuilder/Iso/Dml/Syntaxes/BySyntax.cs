using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

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

        public override string Represent()
        {
            return "BY";
        }
    }
}
