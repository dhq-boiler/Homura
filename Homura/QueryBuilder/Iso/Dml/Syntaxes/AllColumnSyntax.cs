using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class AllColumnSyntax : SyntaxBase, ICountParameterSyntax, INoMarginLeftSyntax
    {
        private string _name;
        private string _tableAlias;

        internal AllColumnSyntax(string name)
            : base()
        {
            _name = name;
        }

        internal AllColumnSyntax(string tableAlias, string name)
            : this(name)
        {
            _tableAlias = tableAlias;
        }

        public override string Represent()
        {
            return $"ALL {_tableAlias}{(!string.IsNullOrEmpty(_tableAlias) ? "." : "")}{_name}";
        }
    }
}
