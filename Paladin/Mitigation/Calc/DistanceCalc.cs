using AEAssist;
using AEAssist.Extension;

namespace LittleDart.Paladin.Mitigation.Calc;

public static class DistanceCalc
{
    public static float ToTarget()
    {
        var target = PldHelper.GetCurrentTarget();
        return target == null ? float.MaxValue : Core.Me.Distance(target);
    }

    public static bool InRange(float min, float max)
    {
        float d = ToTarget();
        return d >= min && d <= max;
    }
}
