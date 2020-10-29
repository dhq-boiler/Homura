

using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using NLog;
using System;
using System.Collections.Generic;

namespace Homura.ORM.Setup
{
    internal class VersioningStrategyByTable : VersioningStrategy
    {
        private static readonly Logger s_logger = LogManager.GetCurrentClassLogger();

        private Dictionary<EntityVersionKey, IEntityVersionChangePlan> _planMap;

        internal VersioningStrategyByTable()
        {
            _planMap = new Dictionary<EntityVersionKey, IEntityVersionChangePlan>();
        }

        internal override IVersionChangePlan GetPlan(VersionOrigin targetVersion)
        {
            throw new NotSupportedException();
        }

        internal override IEntityVersionChangePlan GetPlan(Type targetEntityType, VersionOrigin targetVersion)
        {
            return _planMap[new EntityVersionKey(targetEntityType, targetVersion)];
        }

        internal override void RegisterChangePlan(IEntityVersionChangePlan plan)
        {
            _planMap.Add(new EntityVersionKey(plan.TargetEntityType, plan.TargetVersion), plan);
        }

        internal override void RegisterChangePlan(IVersionChangePlan plan)
        {
            throw new NotSupportedException();
        }

        internal override void Reset()
        {
            _planMap.Clear();
            ModifiedCount = 0;
        }

        internal override void UnregisterChangePlan(VersionOrigin targetVersion)
        {
            throw new NotSupportedException();
        }

        internal override void UnregisterChangePlan(Type targetEntityType, VersionOrigin targetVersion)
        {
            _planMap.Remove(new EntityVersionKey(targetEntityType, targetVersion));
        }

        internal override void UpgradeToTargetVersion(IConnection connection)
        {
            //DBに存在するテーブル名を取得
            IEnumerable<string> existingTableNames = DbInfoRetriever.GetTableNames(connection);

            //テーブル名をキーに変換
            IEnumerable<EntityVersionKey> existingTableKey = UpgradeHelper.ConvertTablenameToKey(_planMap, existingTableNames);

            //定義されている変更プランから、指定した基準キーの前方にある変更プランを取得
            IEnumerable<IEntityVersionChangePlan> plans = UpgradeHelper.GetForwardChangePlans(_planMap, existingTableKey);

            //変更プランを実行
            foreach (var plan in plans)
            {
                plan.UpgradeToTargetVersion(connection);
                ModifiedCount += plan.ModifiedCount;
            }
        }
    }
}
