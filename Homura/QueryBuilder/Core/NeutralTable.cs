using Homura.ORM;
using System.Collections.Generic;

namespace Homura.QueryBuilder.Core
{
    public class NeutralTable : ITableBase
    {
        public virtual string Catalog { get; set; }

        public virtual string Schema { get; set; }

        public virtual string Name { get; set; }

        public virtual string Alias { get; set; }

        public IEnumerable<IColumn> Columns { get; set; }

        public NeutralTable()
        { }

        public NeutralTable(string name)
        {
            Name = name;
        }
    }
}
