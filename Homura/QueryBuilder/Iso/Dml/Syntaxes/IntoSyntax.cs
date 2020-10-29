using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class IntoSyntax : SyntaxBase, IIntoSyntax
    {
        internal IntoSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public IInsertColumnSyntax Table(Table table)
        {
            return new TableSyntax<SyntaxBase>(table.Catalog, table.Schema, table.Name, this, table.Alias);
        }

        public IInsertColumnSyntax Table(string name)
        {
            return new TableSyntax<SyntaxBase>(name, this);
        }

        public IInsertColumnSyntax Table(string name, string alias)
        {
            return new TableSyntax<SyntaxBase>(name, this, alias);
        }

        public IInsertColumnSyntax Table(string schemaName, string name, string alias)
        {
            return new TableSyntax<SyntaxBase>(schemaName, name, this, alias);
        }

        public IInsertColumnSyntax Table(string catalogName, string schemaName, string name, string alias)
        {
            return new TableSyntax<SyntaxBase>(catalogName, schemaName, name, this, alias);
        }

        public override string Represent()
        {
            return $"INTO";
        }
    }
}
