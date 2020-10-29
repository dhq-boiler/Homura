

using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using Homura.QueryBuilder.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Homura.ORM
{
    public sealed class Table<E> : Table, ITable, ICloneable where E : EntityBaseObject
    {
        private int _internalVersionNumber;

        private Type _SpecifiedVersion;

        public Table()
        {
            SpecifiedVersion = null;
        }

        public Table(Type versionType)
        {
            if (VersionHelper.GetInternalVersionNumberFromVersionType(versionType) == -1)
            {
                throw new ArgumentException("Not inherited VersionOrigin class.");
            }
            SpecifiedVersion = versionType;
        }

        public Table(string aliasName)
            : this()
        {
            Alias = aliasName;
        }

        public string AttachedDatabaseAlias { get; set; }

        public bool HasAttachedDatabaseAlias { get { return !string.IsNullOrWhiteSpace(AttachedDatabaseAlias); } }

        public bool HasAlias { get { return !string.IsNullOrWhiteSpace(Alias); } }

        public string EntityName
        {
            get { return typeof(E).Name; }
        }

        public Type EntityClassType
        {
            get { return typeof(E); }
        }

        public override string Name
        {
            get
            {
                return VersionHelper.GeneratePhysicalTableName(EntityName, _internalVersionNumber);
            }
        }

        public Type DefaultVersion
        {
            get { return VersionHelper.GetDefaultVersion<E>(); }
        }

        public Type SpecifiedVersion
        {
            get { return _SpecifiedVersion; }
            set
            {
                _SpecifiedVersion = value;
                if (value != null)
                {
                    _internalVersionNumber = VersionHelper.GetInternalVersionNumberFromVersionType(value);
                }
                else if (DefaultVersion != null && DefaultVersion != typeof(VersionOrigin))
                {
                    _internalVersionNumber = VersionHelper.GetInternalVersionNumberFromVersionType(DefaultVersion);
                }
                else
                {
                    _internalVersionNumber = 0;
                }
            }
        }

        public IEnumerable<IColumn> Columns
        {
            get
            {
                List<IColumn> columns = new List<IColumn>();
                var pinfoList = typeof(E).GetProperties();

                foreach (var pinfo in pinfoList)
                {
                    var columnAttribute = pinfo.GetCustomAttribute<ColumnAttribute>();
                    bool matchVersion = IsAvaiablePropertyOnVersion(pinfo);

                    if (columnAttribute != null && matchVersion)
                    {
                        var constraintAttrs = from a in pinfo.GetCustomAttributes()
                                              where a is IDdlConstraintAttribute
                                              select (a as IDdlConstraintAttribute).ToConstraint();

                        columns.Add(new Column(columnAttribute.ColumnName,
                                               columnAttribute.ColumnType,
                                               constraintAttrs,
                                               columnAttribute.Order,
                                               pinfo));
                    }
                }

                return columns.OrderBy(a => a.Order).ToList();
            }
        }

        public IEnumerable<IColumn> PrimaryKeyColumns
        {
            get
            {
                var classPkAttr = typeof(E).GetCustomAttribute<PrimaryKeyAttribute>();

                List<IColumn> columns = new List<IColumn>();
                var pinfoList = typeof(E).GetProperties();

                foreach (var pinfo in pinfoList)
                {
                    var pkAttr = pinfo.GetCustomAttribute<PrimaryKeyAttribute>();
                    var columnAttr = pinfo.GetCustomAttribute<ColumnAttribute>();
                    bool matchVersion = IsAvaiablePropertyOnVersion(pinfo);

                    if ((classPkAttr != null && columnAttr != null && matchVersion && classPkAttr.ColumnNames.Contains(columnAttr.ColumnName))
                        || (pkAttr != null && columnAttr != null))
                    {
                        var constraintAttrs = from a in pinfo.GetCustomAttributes()
                                              where a is IDdlConstraintAttribute
                                              select (a as IDdlConstraintAttribute).ToConstraint();

                        columns.Add(new Column(columnAttr.ColumnName,
                                               columnAttr.ColumnType,
                                               constraintAttrs,
                                               columnAttr.Order,
                                               pinfo));
                    }
                }

                return columns.OrderBy(a => a.Order).ToList();
            }
        }

        public IEnumerable<IColumn> ColumnsWithoutPrimaryKeys
        {
            get
            {
                List<IColumn> columns = new List<IColumn>();
                var pinfoList = typeof(E).GetProperties();

                foreach (var pinfo in pinfoList)
                {
                    var pkAttr = pinfo.GetCustomAttribute<PrimaryKeyAttribute>();
                    if (pkAttr != null)
                    {
                        continue;
                    }

                    var columnAttr = pinfo.GetCustomAttribute<ColumnAttribute>();
                    bool matchVersion = IsAvaiablePropertyOnVersion(pinfo);
                    if (columnAttr != null && matchVersion)
                    {
                        var constraintAttrs = from a in pinfo.GetCustomAttributes()
                                              where a is IDdlConstraintAttribute
                                              select (a as IDdlConstraintAttribute).ToConstraint();

                        columns.Add(new Column(columnAttr.ColumnName,
                                               columnAttr.ColumnType,
                                               constraintAttrs,
                                               columnAttr.Order,
                                               pinfo));
                    }
                }

                return columns.OrderBy(a => a.Order).ToList();
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
            var newTable = new Table<E>();
            newTable.Alias = Alias;
            newTable.AttachedDatabaseAlias = AttachedDatabaseAlias;
            newTable.SpecifiedVersion = SpecifiedVersion;
            return newTable;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Table<E>)) return false;
            var o = obj as Table<E>;
            return AttachedDatabaseAlias == o.AttachedDatabaseAlias
                && Alias == o.Alias
                && SpecifiedVersion == o.SpecifiedVersion
                && _internalVersionNumber == o._internalVersionNumber
                && EntityName == o.EntityName;
        }

        public override int GetHashCode()
        {
            int hash = 0;
            if (HasAttachedDatabaseAlias)
            {
                hash ^= AttachedDatabaseAlias.GetHashCode();
            }
            if (SpecifiedVersion != null)
            {
                hash ^= SpecifiedVersion.GetHashCode();
            }

            hash ^= _internalVersionNumber.GetHashCode();
            hash ^= EntityName.GetHashCode();

            return hash;
        }

        private bool IsAvaiablePropertyOnVersion(PropertyInfo pinfo)
        {
            var version = (SpecifiedVersion != null ? SpecifiedVersion : DefaultVersion);
            return VersionHelper.IsAvaiablePropertyOnVersion(pinfo, version);
        }
    }
}
