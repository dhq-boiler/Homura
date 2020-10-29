

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class SearchConditionSyntax<R, R1, R2> : SyntaxBase, ISearchCondition<R, R1, R2> where R : class
                                                                                            where R1 : class
                                                                                            where R2 : class
    {
        private string _name;
        private string _tableAlias;

        internal SearchConditionSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        internal SearchConditionSyntax(SyntaxBase syntaxBase, string name)
            : this(syntaxBase)
        {
            _name = name;
        }

        internal SearchConditionSyntax(SyntaxBase syntaxBase, string tableAlias, string name)
            : this(syntaxBase, name)
        {
            _tableAlias = tableAlias;
        }

        public R1 EqualTo { get { return new EqualToSyntax<R>(this) as R1; } }

        public R1 GreaterThan { get { return new GreaterThanSyntax<R>(this) as R1; } }

        public R1 GreaterThanOrEqualTo { get { return new GreaterThanOrEqualToSyntax<R>(this) as R1; } }

        public IInSyntax In { get { return new InSyntax(this); } }

        public R2 Is { get { return new IsSyntax<R>(this) as R2; } }

        public R1 LessThan { get { return new LessThanSyntax<R>(this) as R1; } }

        public R1 LessThanOrEqualTo { get { return new LessThanOrEqualToSyntax<R>(this) as R1; } }

        public R1 Like { get { return new LikeSyntax<R>(this) as R1; } }

        public INotSyntax Not { get { return new NotSyntax(this); } }

        public R1 NotEqualTo { get { return new NotEqualToSyntax<R>(this) as R1; } }

        public override string Represent()
        {
            return $"{_tableAlias}{(!string.IsNullOrEmpty(_tableAlias) ? "." : "")}{_name}";
        }
    }
}
