

using Homura.ORM;
using System;
using System.Collections.Generic;

namespace Homura.Test.TestFixture
{
    internal abstract class DummyAbstractTable : ITable
    {
        public DummyAbstractTable()
        { }

        public DummyAbstractTable(string aliasName)
        {
            Alias = aliasName;
        }

        public abstract IEnumerable<IColumn> Columns { get; }

        public abstract string Name { get; }

        public string AttachedDatabaseAlias { get; set; }

        public bool HasAttachedDatabaseAlias { get { return !string.IsNullOrWhiteSpace(AttachedDatabaseAlias); } }

        public string Alias { get; set; }

        public bool HasAlias { get { return !string.IsNullOrWhiteSpace(Alias); } }

        public IEnumerable<IColumn> PrimaryKeyColumns
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public IEnumerable<IColumn> ColumnsWithoutPrimaryKeys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type DefaultVersion
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type EntityClassType
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public Type SpecifiedVersion
        {
            get
            {
                throw new NotImplementedException();
            }

            set
            {
                throw new NotImplementedException();
            }
        }

        public ITable SetAttachedDatabaseAliasName(string attachedDbAliasName)
        {
            AttachedDatabaseAlias = attachedDbAliasName;
            return this;
        }

        public ITable SetAlias(string aliasName)
        {
            Alias = aliasName;
            return this;
        }

        public object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
