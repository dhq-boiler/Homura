using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class DistinctColumnSyntax : SyntaxBase, ICountParameterSyntax, INoMarginLeftSyntax
    {
        private string tableAlias;
        private string _name;

        internal DistinctColumnSyntax(string name)
            : base()
        {
            _name = name;
        }

        internal DistinctColumnSyntax(string tableAlias, string name)
            : this(name)
        {
            this.tableAlias = tableAlias;
        }

        public override string Represent()
        {
            return $"DISTINCT {tableAlias}{(!string.IsNullOrEmpty(tableAlias) ? "." : "")}{_name}";
        }
    }
}
