using AEAssist.Helper;

namespace LittleDart.Paladin.Mitigation.Calc;

public static class DeathSentenceDetector
{
    public static bool IsCastingDeathSentence()
    {
        var target = PldHelper.GetCurrentTarget();
        return target != null && TargetHelper.targetCastingIsDeathSentence(target);
    }
}
