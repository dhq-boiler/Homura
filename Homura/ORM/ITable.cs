

using System;
using System.Collections.Generic;

namespace Homura.ORM
{
    public interface ITable : ICloneable
    {
        IEnumerable<IColumn> Columns { get; }

        IEnumerable<IColumn> PrimaryKeyColumns { get; }

        IEnumerable<IColumn> ColumnsWithoutPrimaryKeys { get; }

        string AttachedDatabaseAlias { get; }

        bool HasAttachedDatabaseAlias { get; }

        string Name { get; }

        string Alias { get; }

        bool HasAlias { get; }

        Type EntityClassType { get; }

        Type DefaultVersion { get; }

        Type SpecifiedVersion { get; set; }

        ITable SetAttachedDatabaseAliasName(string attachedDbAliasName);

        ITable SetAlias(string aliasName);
    }
}
