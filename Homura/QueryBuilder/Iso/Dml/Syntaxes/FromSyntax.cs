using Homura.QueryBuilder.Core;

namespace Homura.QueryBuilder.Iso.Dml.Syntaxes
{
    internal class FromSyntax<Return1, Return2> : SyntaxBase, IFromSyntax<Return1> where Return1 : class
                                                                                   where Return2 : class
    {
        internal FromSyntax(SyntaxBase syntaxBase)
            : base(syntaxBase)
        { }

        public Return1 SubQuery(IConditionValueSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as Return1;
        }

        public Return1 SubQuery(IJoinConditionSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as Return1;
        }

        public Return1 SubQuery(IOrderByColumnSyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as Return1;
        }

        public Return1 SubQuery(IOrderBySyntax subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as Return1;
        }

        public Return1 SubQuery(ICloseSyntax<IConditionValueSyntax> subquery)
        {
            var begin = new BeginSubquerySyntax(this);
            var end = new EndSubquerySyntax(begin);
            end.Relay.AddRange((subquery as SyntaxBase).PassRelay());
            return end as Return1;
        }

        public Return1 Table(Table table)
        {
            return new TableSyntax<Return2>(table.Catalog, table.Schema, table.Name, this, table.Alias) as Return1;
        }

        public Return1 Table(string tableName)
        {
            return new TableSyntax<Return2>(tableName, this) as Return1;
        }

        public Return1 Table(string tableName, string alias)
        {
            return new TableSyntax<Return2>(tableName, this, alias) as Return1;
        }

        public Return1 Table(string schemaName, string name, string alias)
        {
            return new TableSyntax<Return2>(schemaName, name, this, alias) as Return1;
        }

        public Return1 Table(string catalogName, string schemaName, string name, string alias)
        {
            return new TableSyntax<Return2>(catalogName, schemaName, name, this, alias) as Return1;
        }

        public override string Represent()
        {
            return "FROM";
        }
    }
}
