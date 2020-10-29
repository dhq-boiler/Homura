using Homura.QueryBuilder.Core;
using System.Collections.Generic;
using System.Linq;

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

        public override string Represent()
        {
            return "DISTINCT";
        }
    }
}