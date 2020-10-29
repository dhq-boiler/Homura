

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class OrderBySyntax : SyntaxBase, IOrderBySyntax
    {
        internal OrderBySyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IOrderByColumnSyntax Column(string name)
        {
            return new OrderByColumnSyntax(this, name);
        }

        public IOrderByColumnSyntax Column(string tableAlias, string name)
        {
            return new OrderByColumnSyntax(this, tableAlias, name);
        }

        public override string Represent()
        {
            return "ORDER BY";
        }

        public string ToSql()
        {
            return Relay.RelayQuery(this);
        }
    }
}
