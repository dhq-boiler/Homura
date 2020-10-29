using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System.Collections.Generic;
using System.Linq;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class AllSyntax : SyntaxBase, ISetQuantifierSyntax, ICorrespondingTransition
    {
        internal AllSyntax(SyntaxBase syntaxBase) : base(syntaxBase)
        { }

        public ICorrespondingSyntax Corresponding { get { return new CorrespondingSyntax(this); } }

        public ICrossSyntax Cross { get { return new CrossSyntax(this); } }

        public IOuterJoinTypeSyntax Full { get { return new FullSyntax(this); } }

        public IJoinTypeSyntax Inner { get { return new InnerSyntax(this); } }

        public IOuterJoinTypeSyntax Left { get { return new LeftSyntax(this); } }

        public INaturalSyntax Natural { get { return new NaturalSyntax(this); } }

        public IOuterJoinTypeSyntax Right { get { return new RightSyntax(this); } }

        public ISelectSyntax Select { get { return new SelectSyntax(this); } }

        public IUnionSyntax Union { get { return new UnionSyntax(this); } }

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
            return "ALL";
        }
    }
}