using Homura.QueryBuilder.Iso.Dml.Syntaxes;

namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IWhereTransition<R> where R : class
    {
        IWhereSyntax<R, IOperatorSyntax<R>, IIsSyntax<R>> Where { get; }
    }
}
