namespace LittleDart.Paladin.Mitigation.Calc;

public static class TargetDeathPredictor
{
    public static bool TryPredictSecondsToDeath(HpTracker hp, int windowMs, out double seconds)
    {
        seconds = double.PositiveInfinity;

        float hpPercent = hp.TargetHpPercent();
        if (hpPercent < 0f)
        {
            return false;
        }

        float declinePerSecond = hp.TargetNetDeclinePerSecond(windowMs);
        if (declinePerSecond <= 0.01f)
        {
            return false;
        }

        seconds = hpPercent * 100f / declinePerSecond;
        return true;
    }

    public static bool WillDieWithin(HpTracker hp, double seconds, int windowMs)
        => TryPredictSecondsToDeath(hp, windowMs, out double ttd) && ttd <= seconds;
}
