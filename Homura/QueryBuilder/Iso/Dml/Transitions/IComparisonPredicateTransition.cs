namespace Homura.QueryBuilder.Iso.Dml.Transitions
{
    public interface IComparisonPredicateTransition<Return> : IEqualToTransition<Return> where Return : class
    {
        Return NotEqualTo { get; }

        Return GreaterThan { get; }

        Return LessThan { get; }

        Return GreaterThanOrEqualTo { get; }

        Return LessThanOrEqualTo { get; }
    }
}
