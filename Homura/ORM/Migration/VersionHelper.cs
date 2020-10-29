

using Homura.ORM.Mapping;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Homura.ORM.Migration
{
    public static class VersionHelper
    {
        public static int GetInternalVersionNumberFromDefaultVersionAttribute(Type entityClassType)
        {
            var versionAttribute = entityClassType.GetCustomAttribute<DefaultVersionAttribute>();
            if (versionAttribute == null) return -1;
            return GetInternalVersionNumberFromVersionType(versionAttribute.Version);
        }

        public static int GetInternalVersionNumberFromDefaultVersionAttribute<E>() where E : EntityBaseObject
        {
            return GetInternalVersionNumberFromDefaultVersionAttribute(typeof(E));
        }

        public static int GetInternalVersionNumberFromVersionType(Type versionType)
        {
            if (versionType == null)
            {
                return 0;
            }

            int chainCount = 0;

            var chain = versionType;
            while (chain.Name != typeof(VersionOrigin).Name)
            {
                if (chain.BaseType == null)
                {
                    return -1;
                }
                chain = chain.BaseType;
                ++chainCount;
            }

            return chainCount;
        }

        public static int GetInternalVersionNumberFromVersionType(VersionOrigin version)
        {
            return GetInternalVersionNumberFromVersionType(version.GetType());
        }

        [Obsolete]
        public static int GetInternalVersionNumberFromTableName<E>(string tableName) where E : EntityBaseObject
        {
            var table = new Table<E>();
            Regex rgx = new Regex($"{table.EntityName}(_(?<version>\\d+))*");
            if (rgx.IsMatch(tableName))
            {
                var mc = rgx.Match(tableName);
                var value = mc.Groups["version"].Value;
                if (string.IsNullOrWhiteSpace(value))
                {
                    return 0;
                }
                return int.Parse(value);
            }

            throw new ArgumentException($"Unexpected tablename:{tableName}");
        }

        public static Type GetDefaultVersion<E>()
        {
            return GetDefaultVersion(typeof(E));
        }

        public static Type GetDefaultVersion(Type entityClassType)
        {
            var versionAttribute = entityClassType.GetCustomAttribute<DefaultVersionAttribute>();
            return versionAttribute?.Version;
        }

        public static bool IsNewerOrEqual(this Type standard, Type comparison)
        {
            var standardVerNo = GetInternalVersionNumberFromVersionType(standard);
            var comparisonVerNo = GetInternalVersionNumberFromVersionType(comparison);

            if (standardVerNo < 0)
            {
                throw new ArgumentException();
            }

            if (comparisonVerNo < 0)
            {
                throw new ArgumentException();
            }

            return standardVerNo >= comparisonVerNo;
        }

        public static bool IsOlderThan(this Type standard, Type comparison)
        {
            var standardVerNo = GetInternalVersionNumberFromVersionType(standard);
            var comparisonVerNo = GetInternalVersionNumberFromVersionType(comparison);

            if (standardVerNo < 0)
            {
                throw new ArgumentException();
            }

            if (comparisonVerNo < 0)
            {
                throw new ArgumentException();
            }

            return standardVerNo < comparisonVerNo;
        }

        [Obsolete]
        public static IEnumerable<Type> GetVersionHistoryListFromRootTo(Type leafVersion)
        {
            List<Type> versionList = new List<Type>();

            if (leafVersion.Name == typeof(VersionOrigin).Name)
            {
                versionList.Add(typeof(VersionOrigin));
                return versionList;
            }

            var chain = leafVersion;
            versionList.Add(chain);
            while (chain.Name != typeof(VersionOrigin).Name)
            {
                if (chain.BaseType == null)
                {
                    return null;
                }
                chain = chain.BaseType;
                versionList.Add(chain);
            }

            versionList.Reverse();
            return versionList;
        }

        [Obsolete]
        public static IEnumerable<Type> GetVersionHistoryList(Type from, Type to)
        {
            if (to.Name == from.Name)
            {
                List<Type> versionList = new List<Type>();
                versionList.Add(typeof(VersionOrigin));
                return versionList;
            }

            bool willUpgrade = from.IsOlderThan(to);

            if (willUpgrade)
            {
                return ComputeVersionSequence(from, to);
            }
            else
            {
                return ComputeVersionSequence(to, from).Reverse();
            }
        }

        private static IEnumerable<Type> ComputeVersionSequence(Type olderFrom, Type newerTo)
        {
            List<Type> versionList = new List<Type>();
            var chain = newerTo;
            versionList.Add(chain);
            while (chain.Name != olderFrom.Name)
            {
                if (chain.BaseType == null)
                {
                    return null;
                }
                chain = chain.BaseType;
                versionList.Add(chain);
            }

            versionList.Reverse();
            return versionList;
        }

        /// <summary>
        /// Get columns at specified table's version.
        /// </summary>
        /// <typeparam name="E"></typeparam>
        /// <param name="versionType"></param>
        /// <returns></returns>
        public static IEnumerable<IColumn> GetTableColumnsAtThisVersion<E>(Type versionType) where E : EntityBaseObject
        {
            List<IColumn> columns = new List<IColumn>();
            var pinfoList = typeof(E).GetProperties();
            foreach (var pinfo in pinfoList)
            {
                var columnAttribute = pinfo.GetCustomAttribute<ColumnAttribute>();
                bool matchVersion = IsAvaiablePropertyOnVersion(pinfo, versionType);

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

        internal static bool IsAvaiablePropertyOnVersion(PropertyInfo pinfo, Type targetVersion)
        {
            var sinceAttribute = pinfo.GetCustomAttribute<SinceAttribute>();
            var untilAttribute = pinfo.GetCustomAttribute<UntilAttribute>();

            var matchSince = sinceAttribute == null
                || targetVersion == null
                || targetVersion.IsNewerOrEqual(sinceAttribute.VersionSince);
            var matchUntil = untilAttribute == null
                || targetVersion == null
                || targetVersion.IsOlderThan(untilAttribute.VersionUntil);
            return matchSince && matchUntil;
        }

        internal static string GeneratePhysicalTableName(string entityName, int internalVersionNumber)
        {
            return entityName + (internalVersionNumber > 0 ? $"_{internalVersionNumber}" : "");
        }
    }
}
