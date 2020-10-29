
using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface ISearchCondition<R, R1, R2> : ISyntaxBase, IComparisonPredicateTransition<R1> where R : class
                                                                                                   where R1 : class
                                                                                                   where R2 : class
    {
        R2 Is { get; }

        IInSyntax In { get; }

        R1 Like { get; }

        INotSyntax Not { get; }
    }
}
