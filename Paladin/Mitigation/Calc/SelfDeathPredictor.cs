using AEAssist;
using AEAssist.Extension;

namespace LittleDart.Paladin.Mitigation.Calc;

public static class SelfDeathPredictor
{
    public static bool TryPredictSecondsToDeath(HpTracker hp, int windowMs, out double seconds)
    {
        seconds = double.PositiveInfinity;

        float hpPercent = 100f;
        float declinePerSecond = hp.SelfNetDeclinePerSecond(windowMs);
        if (declinePerSecond <= 0.01f)
        {
            return false;
        }

        hpPercent = CurrentSelfHpPercent();
        if (hpPercent < 0f)
        {
            return false;
        }

        seconds = hpPercent * 100f / declinePerSecond;
        return true;
    }

    public static bool WillDieWithin(HpTracker hp, double seconds, int windowMs)
        => TryPredictSecondsToDeath(hp, windowMs, out double ttd) && ttd <= seconds;

    private static float CurrentSelfHpPercent()
    {
        try
        {
            return Core.Me.CurrentHpPercent();
        }
        catch
        {
            return -1f;
        }
    }
}
