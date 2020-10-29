
using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Homura.ORM.Setup
{
    public static class UpgradeHelper
    {
        public static IEnumerable<Type> EnumDefinedEntities(Dictionary<EntityVersionKey, IEntityVersionChangePlan> planMap)
        {
            return (from p in planMap
                    group p by p.Key.TargetEntityType into t
                    select t.Key).Distinct();
        }

        public static IEnumerable<Type> EnumDefinedEntitiesByTick(Dictionary<VersionKey, IVersionChangePlan> planMap)
        {
            return (from p in planMap
                    from q in p.Value.VersionChangePlanList
                    group q by q.TargetEntityType into t
                    select t.Key).Distinct();
        }

        public static IEnumerable<VersionOrigin> EnumDefinedVersions(Dictionary<EntityVersionKey, IEntityVersionChangePlan> planMap)
        {
            return (from p in planMap
                    group p by p.Key.TargetVersion into v
                    orderby VersionHelper.GetInternalVersionNumberFromVersionType(v.Key)
                    select v.Key).Distinct(new VersionOrigin());
        }

        public static IEnumerable<VersionOrigin> EnumDefinedVersionsByTick(Dictionary<VersionKey, IVersionChangePlan> planMap)
        {
            return (from p in planMap
                    from q in p.Value.VersionChangePlanList
                    group q by q.TargetVersion into v
                    orderby VersionHelper.GetInternalVersionNumberFromVersionType(v.Key)
                    select v.Key).Distinct(new VersionOrigin());
        }

        public static Type GetTableType(Type entity)
        {
            return Type.GetType($"Homura.ORM.Table`1[[{entity.AssemblyQualifiedName}]]");
        }

        public static IEnumerable<EntityVersionKey> ConvertTablenameToKey(Dictionary<EntityVersionKey, IEntityVersionChangePlan> _planMap, IEnumerable<string> existingTableNames)
        {
            //アプリケーションコードに定義されているエンティティを列挙
            var definedEntities = EnumDefinedEntities(_planMap);

            //アプリケーションコードに定義されているバージョンを列挙
            var definedVersions = EnumDefinedVersions(_planMap);

            List<EntityVersionKey> ret = new List<EntityVersionKey>();

            foreach (var existingTableName in existingTableNames)
            {
                foreach (var entity in definedEntities)
                {
                    foreach (var version in definedVersions)
                    {
                        var tableType = GetTableType(entity);
                        var tableSpecifiedVersion = (ITable)Activator.CreateInstance(GetTableType(entity), new object[] { version.GetType() });
                        var physicalTableName = tableSpecifiedVersion.Name;
                        if (physicalTableName == existingTableName)
                        {
                            ret.Add(new EntityVersionKey(entity, version));
                        }
                    }
                }
            }

            return ret;
        }

        public static IEnumerable<EntityVersionKey> ConvertTablenameToKey(Dictionary<VersionKey, IVersionChangePlan> planMap, IEnumerable<string> existingTableNames)
        {
            List<EntityVersionKey> ret = new List<EntityVersionKey>();

            foreach (var plan in planMap)
            {
                var pMap = new Dictionary<VersionKey, IVersionChangePlan>();
                pMap.Add(plan.Key, plan.Value);

                var definedEntities = EnumDefinedEntitiesByTick(pMap);

                var definedVersions = EnumDefinedVersionsByTick(pMap);

                foreach (var existingTableName in existingTableNames)
                {
                    foreach (var entity in definedEntities)
                    {
                        foreach (var version in definedVersions)
                        {
                            var tableType = GetTableType(entity);
                            var tableSpecifiedVersion = (ITable)Activator.CreateInstance(GetTableType(entity), new object[] { version.GetType() });
                            var physicalTableName = tableSpecifiedVersion.Name;
                            if (physicalTableName == existingTableName)
                            {
                                ret.Add(new EntityVersionKey(entity, version));
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public static IEnumerable<IEntityVersionChangePlan> GetForwardChangePlans(Dictionary<EntityVersionKey, IEntityVersionChangePlan> _planMap, IEnumerable<EntityVersionKey> existingTableKeys)
        {
            //(1) 変更プランから差分を取得
            Dictionary<EntityVersionKey, IEntityVersionChangePlan> diff = new Dictionary<EntityVersionKey, IEntityVersionChangePlan>(_planMap);
            foreach (var key in existingTableKeys)
            {
                if (diff.ContainsKey(key))
                {
                    diff.Remove(key);
                }
            }

            //(2) 差分から前方の変更プランのみを取得
            var existingLatestTables = ExtractLatestVersionExistingTables(existingTableKeys);
            Dictionary<EntityVersionKey, IEntityVersionChangePlan> forwardKeys = new Dictionary<EntityVersionKey, IEntityVersionChangePlan>();
            foreach (var key in diff)
            {
                foreach (var latest in existingLatestTables)
                {
                    if (key.Key.TargetEntityType.FullName == latest.TargetEntityType.FullName
                        && key.Key.TargetVersion.GetIndex() > latest.TargetVersion.GetIndex())
                    {
                        forwardKeys.Add(key.Key, key.Value);
                    }
                }
            }

            //(3) 存在しないテーブルの変更プランを取得
            Dictionary<EntityVersionKey, IEntityVersionChangePlan> _new = new Dictionary<EntityVersionKey, IEntityVersionChangePlan>();
            var entityTypeNames = existingTableKeys.Select(k => k.TargetEntityType.FullName).Distinct();
            foreach (var key in _planMap)
            {
                if (!entityTypeNames.Contains(key.Key.TargetEntityType.FullName))
                {
                    _new.Add(key.Key, key.Value);
                }
            }

            //(4) return (2)+(3)
            return forwardKeys.Union(_new).OrderBy(k => k.Key.TargetEntityType.FullName).OrderBy(k => k.Key.TargetVersion.GetIndex()).Select(k => k.Value);
        }

        public static IEnumerable<IVersionChangePlan> GetForwardChangePlans(Dictionary<VersionKey, IVersionChangePlan> _planMap, IEnumerable<EntityVersionKey> existingTableKey)
        {
            List<IVersionChangePlan> ret = new List<IVersionChangePlan>();

            Dictionary<VersionKey, IVersionChangePlan> satisfactoryPlan = new Dictionary<VersionKey, IVersionChangePlan>();

            //(1)条件を満たすVersionChangePlanを検索する
            foreach (var plan in _planMap)
            {
                if (IsFull(existingTableKey, plan))
                {
                    satisfactoryPlan.Add(plan.Key, plan.Value);
                }
            }

            //(2) (1)の結果から最も新しい、条件を満たすVersionChangePlanを取得
            var latestSatisfactoryPlan = (from p in satisfactoryPlan
                                          orderby VersionHelper.GetInternalVersionNumberFromVersionType(p.Key.TargetVersion)
                                          select p).LastOrDefault();

            Dictionary<VersionKey, IVersionChangePlan> forwardKeys = new Dictionary<VersionKey, IVersionChangePlan>();
            if (!latestSatisfactoryPlan.Equals(default(KeyValuePair<VersionKey, IVersionChangePlan>)))
            {
                //(3-a) (2)が存在する場合、(2)より新しいVersionChangePlanを取得
                foreach (var key in _planMap)
                {
                    if (key.Key.TargetVersion.GetIndex() > latestSatisfactoryPlan.Key.TargetVersion.GetIndex())
                    {
                        forwardKeys.Add(key.Key, key.Value);
                    }
                }
            }
            else
            {
                //(3-b) (2)が存在しない場合、全てのVersionChangePlanを取得する
                foreach (var plan in _planMap)
                {
                    forwardKeys.Add(plan.Key, plan.Value);
                }
            }

            return forwardKeys.Select(a => a.Value).ToList();
        }

        private static bool IsFull(IEnumerable<EntityVersionKey> existingTableKey, KeyValuePair<VersionKey, IVersionChangePlan> plan)
        {
            foreach (var eplan in plan.Value.VersionChangePlanList)
            {
                var entityVersionKey = new EntityVersionKey(eplan.TargetEntityType, eplan.TargetVersion);
                if (!existingTableKey.Contains(entityVersionKey))
                {
                    return false;
                }
            }
            return true;
        }

        public static IEnumerable<EntityVersionKey> ExtractLatestVersionExistingTables(IEnumerable<EntityVersionKey> existingTableKeys)
        {
            if (existingTableKeys.Count() == 0) return new List<EntityVersionKey>();

            return from k in existingTableKeys
                   orderby VersionHelper.GetInternalVersionNumberFromVersionType(k.TargetVersion)
                   group k by k.TargetEntityType into v
                   let last = v.Last()
                   orderby last.TargetEntityType.Name
                   select last;
        }
    }
}
