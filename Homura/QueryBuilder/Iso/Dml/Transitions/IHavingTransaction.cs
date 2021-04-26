using Homura.QueryBuilder.Iso.Dml.Syntaxes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IHavingTransaction
    {
        IHavingSyntax<IComparisonPredicateTransition<IOperatorSyntax<IConditionValueSyntax>>> Having { get; }
    }
}
