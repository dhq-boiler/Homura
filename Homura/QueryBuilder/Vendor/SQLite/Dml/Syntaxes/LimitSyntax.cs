using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Vendor.SQLite.Dml.Syntaxes
{
    internal class LimitSyntax : SyntaxBase, ILimitSyntax
    {
        private int _count;

        internal LimitSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal LimitSyntax(SyntaxBase syntaxBase, int count)
            : base(syntaxBase)
        {
            _count = count;
        }

        public override string Represent()
        {
            return $"LIMIT {_count}";
        }

        public string ToSql()
        {
            return RelayQuery(this);
        }
    }
}
