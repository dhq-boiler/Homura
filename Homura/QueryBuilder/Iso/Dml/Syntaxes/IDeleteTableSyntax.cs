using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    public interface IDeleteTableSyntax<R> : ISyntaxBase, ISql where R : class
    {
        IWhereSyntax<R, IOperatorSyntax<R>, IIsSyntax<R>> Where { get; }
    }
}
