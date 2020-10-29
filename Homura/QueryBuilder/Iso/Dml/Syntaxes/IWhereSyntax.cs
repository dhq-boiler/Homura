using Homura.QueryBuilder.Core;
using Homura.QueryBuilder.Iso.Dml.Transitions;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IWhereSyntax<R, R1, R2> : ISyntaxBase, IWhereColumnTransition<R, R1, R2> where R : class
                                                                                              where R1 : class
                                                                                              where R2 : class
    {
        IExistsSyntax<R> Exists { get; }

        IWhereNotSyntax<R> Not { get; }

        R KeyEqualToValue(Dictionary<string, object> conditions);
    }
}
