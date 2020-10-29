using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class UpdateSyntax : SyntaxBase, IUpdateSyntax
    {
        internal UpdateSyntax()
            : base()
        { }

        public override string Represent()
        {
            return $"UPDATE";
        }

        public IUpdateTableSyntax Table(string name)
        {
            return new TableSyntax<SyntaxBase>(name, this);
        }

        public IUpdateTableSyntax Table(string name, string alias = null)
        {
            return new TableSyntax<SyntaxBase>(name, this, alias);
        }

        public IUpdateTableSyntax Table(string schemaName, string name, string alias = null)
        {
            return new TableSyntax<SyntaxBase>(schemaName, name, this, alias);
        }

        public IUpdateTableSyntax Table(string catalogName, string schemaName, string name, string alias = null)
        {
            return new TableSyntax<SyntaxBase>(catalogName, schemaName, name, this, alias);
        }

        public IUpdateTableSyntax Table(Table table)
        {
            return new TableSyntax<SyntaxBase>(table.Catalog, table.Schema, table.Name, this, table.Alias);
        }
    }
}
