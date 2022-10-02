﻿

using Homura.ORM.Mapping;
using Homura.ORM.Migration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Homura.ORM.Setup
{
    internal class VersioningStrategyByTick : VersioningStrategy
    {
        private Dictionary<VersionKey, IVersionChangePlan> _planMap;

        internal VersioningStrategyByTick()
        {
            _planMap = new Dictionary<VersionKey, IVersionChangePlan>();
        }

        internal override IVersionChangePlan GetPlan(VersionOrigin targetVersion)
        {
            return _planMap[new VersionKey(targetVersion)];
        }

        internal override IEntityVersionChangePlan GetPlan(Type targetEntityType, VersionOrigin targetVersion)
        {
            throw new NotSupportedException();
        }

        internal override void RegisterChangePlan(IEntityVersionChangePlan plan)
        {
            throw new NotSupportedException();
        }

        internal override void RegisterChangePlan(IVersionChangePlan plan)
        {
            plan.Mode = VersioningMode;
            _planMap.Add(new VersionKey(plan.TargetVersion), plan);
        }

        internal override void Reset()
        {
            _planMap.Clear();
            ModifiedCount = 0;
        }

        internal override void UnregisterChangePlan(VersionOrigin targetVersion)
        {
            _planMap.Remove(new VersionKey(targetVersion));
        }

        internal override void UnregisterChangePlan(Type targetEntityType, VersionOrigin targetVersion)
        {
            throw new NotSupportedException();
        }

        internal override void UpgradeToTargetVersion(IConnection connection)
        {
            //DBに存在するテーブル名を取得
            IEnumerable<string> existingTableNames = DbInfoRetriever.GetTableNames(connection);

            //テーブル名をキーに変換
            var existingTableKey = UpgradeHelper.ConvertTablenameToKey(_planMap, existingTableNames);

            //定義されている変更プランから、指定した基準キーの前方にある変更プランを取得？
            IEnumerable<IVersionChangePlan> plans = UpgradeHelper.GetForwardChangePlans(_planMap, existingTableKey);

            foreach (var plan in plans)
            {
                plan.UpgradeToTargetVersion(connection);
                ModifiedCount += plan.ModifiedCount;
            }
        }
    }
}
