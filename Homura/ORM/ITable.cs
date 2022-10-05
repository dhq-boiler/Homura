

using System;
using System.Collections.Generic;

namespace Homura.ORM
{
    public interface ITable : ITableBase, ICloneable
    {
        IEnumerable<IColumn> PrimaryKeyColumns { get; }

        IEnumerable<IColumn> ColumnsWithoutPrimaryKeys { get; }

        string AttachedDatabaseAlias { get; }

        bool HasAttachedDatabaseAlias { get; }
        string Catalog { get; }

        string Schema { get; }

        string Name { get; }

        string Alias { get; }

        bool HasAlias { get; }

        Type EntityClassType { get; }

        Type DefaultVersion { get; }

        Type SpecifiedVersion { get; set; }
        string EntityName { get; }

        ITable SetAttachedDatabaseAliasName(string attachedDbAliasName);

        ITable SetAlias(string aliasName);
    }
}
