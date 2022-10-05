using System.Collections.Generic;

namespace Homura.ORM
{
    public interface ITableBase
    {
        string Alias { get; }
        string Catalog { get; }
        IEnumerable<IColumn> Columns { get; }
        string Name { get; }
        string Schema { get; }
    }
}