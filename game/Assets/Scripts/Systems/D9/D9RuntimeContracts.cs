// ============================================================
// D9RuntimeContracts —— D9-S02 统一存档投影
//
// 只保存 D9 自己拥有的任务/流程投影；D3 的货物、市场、天气和 EventLog
// 仍由 D3RuntimeSnapshot 保存，D4 的鱼群/战斗仍由 D4RuntimeSnapshot 保存。
// ============================================================

using System;

namespace Whalebone.Systems.D9
{
    [Serializable]
    public sealed class D9RuntimeSnapshot
    {
        public const int CurrentSchemaVersion = 1;

        public int SchemaVersion = CurrentSchemaVersion;
        public D9TreasureQuestSnapshot TreasureQuest;

        public D9RuntimeSnapshot Clone()
        {
            return new D9RuntimeSnapshot
            {
                SchemaVersion = SchemaVersion,
                TreasureQuest = TreasureQuest == null ? null : TreasureQuest.Clone(),
            };
        }

        public void Validate()
        {
            if (SchemaVersion != CurrentSchemaVersion)
                throw new InvalidOperationException("D9 runtime schema_version mismatch: expected " +
                    CurrentSchemaVersion + ", received " + SchemaVersion + ".");
            if (TreasureQuest != null) TreasureQuest.Validate();
        }
    }
}
