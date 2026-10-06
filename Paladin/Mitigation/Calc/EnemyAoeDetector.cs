using AEAssist.CombatRoutine;
using AEAssist.Helper;

namespace LittleDart.Paladin.Mitigation.Calc;

public static class EnemyAoeDetector
{
    public static bool IsIncomingAoe(int warnMs)
    {
        var target = PldHelper.GetCurrentTarget();
        if (target == null)
        {
            return false;
        }

        if (TargetHelper.targetCastingIsBossAOE(target, warnMs))
        {
            return true;
        }

        return target.IsCasting && Data.AoeActions.Contains(target.CastActionId);
    }
}
