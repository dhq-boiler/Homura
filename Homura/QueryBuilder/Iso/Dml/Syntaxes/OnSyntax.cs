

using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class OnSyntax : SyntaxBase, IOnSyntax
    {
        internal OnSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public ISearchCondition<IJoinConditionSyntax, IOperatorColumnSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>> Column(string name)
        {
            return new SearchConditionSyntax<IJoinConditionSyntax, IOperatorColumnSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>>(this, name);
        }

        public ISearchCondition<IJoinConditionSyntax, IOperatorColumnSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>> Column(string tableAlias, string name)
        {
            return new SearchConditionSyntax<IJoinConditionSyntax, IOperatorColumnSyntax<IJoinConditionSyntax>, IIsSyntax<IJoinConditionSyntax>>(this, tableAlias, name);
        }

        public override string Represent()
        {
            return "ON";
        }
    }
}
