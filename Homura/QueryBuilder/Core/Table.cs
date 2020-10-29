namespace Homura.QueryBuilder.Core
{
    public class Table
    {
        public virtual string Catalog { get; set; }

        public virtual string Schema { get; set; }

        public virtual string Name { get; set; }

        public virtual string Alias { get; set; }
    }
}
