using AEAssist;
using AEAssist.CombatRoutine.Module.Target;
using AEAssist.Extension;
using Dalamud.Game.ClientState.Objects.Enums;
using Dalamud.Game.ClientState.Objects.Types;

namespace LittleDart.Paladin.Mitigation.Calc;

public static class EnemyCounter
{
    public static int CountWithin(float radius) => PldHelper.GetNearbyEnemyCount((int)Math.Round(radius));

    public static int CountAggroOnMe()
    {
        try
        {
            ulong meId = Core.Me.GameObjectId;
            int count = 0;
            foreach (var enemy in TargetMgr.Instance.EnemysIn25.Values)
            {
                if (enemy == null || !enemy.IsEnemy())
                {
                    continue;
                }

                if (enemy.TargetObjectId == meId)
                {
                    count++;
                }
            }

            return count;
        }
        catch
        {
            return PldHelper.HasTankStance() ? CountWithin(10f) : 0;
        }
    }

    public static IBattleChara? PickPullTarget(float range)
    {
        try
        {
            ulong meId = Core.Me.GameObjectId;
            IBattleChara? best = null;
            float bestDistance = float.MaxValue;
            int bestRank = 0;

            foreach (var enemy in TargetMgr.Instance.EnemysIn25.Values)
            {
                if (enemy == null || !enemy.IsEnemy() || !enemy.IsTargetable)
                {
                    continue;
                }

                bool notMyAggro = enemy.TargetObjectId != meId;
                bool notInCombat = (enemy.StatusFlags & StatusFlags.InCombat) == 0;

                if (!notMyAggro && !notInCombat)
                {
                    continue;
                }

                int rank = notInCombat ? 2 : 1;

                float distance = PldHelper.GetDistanceTo(enemy);
                if (range > 0f && distance > range)
                {
                    continue;
                }

                if (rank > bestRank || (rank == bestRank && distance < bestDistance))
                {
                    best = enemy;
                    bestRank = rank;
                    bestDistance = distance;
                }
            }

            return best;
        }
        catch
        {
            return null;
        }
    }
}
