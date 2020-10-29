using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class UpdateColumnSyntax : RepeatableSyntax, ISetClauseSyntax
    {
        private string _name;
        private string _tableAlias;

        internal UpdateColumnSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal UpdateColumnSyntax(SyntaxBase syntaxBase, string name)
            : this(syntaxBase)
        {
            _name = name;
        }

        internal UpdateColumnSyntax(SyntaxBase syntaxBase, string name, Delimiter prefix)
            : base(syntaxBase, prefix)
        {
            _name = name;
        }

        internal UpdateColumnSyntax(SyntaxBase syntaxBase, string tableAlias, string name)
            : this(syntaxBase, name)
        {
            _tableAlias = tableAlias;
        }

        internal UpdateColumnSyntax(SyntaxBase syntaxBase, string tableAlias, string name, Delimiter prefix)
            : this(syntaxBase, name, prefix)
        {
            _tableAlias = tableAlias;
        }

        public IUpdateSourceTransition<IValueExpressionSyntax> EqualTo { get { return new EqualToSyntax<IValueExpressionSyntax>(this); } }

        public override string Represent()
        {
            return $"{Delimiter.ToString()}{_tableAlias}{(!string.IsNullOrEmpty(_tableAlias) ? "." : "")}{_name}";
        }
    }
}
