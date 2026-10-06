namespace LittleDart.Paladin.Mitigation;

public interface IBossTimer
{
    double? GetSecondsToBoss();
}

public sealed class UnknownBossTimer : IBossTimer
{
    public double? GetSecondsToBoss() => null;
}

public static class MitigationSignals
{
    public static IBossTimer BossTimer { get; set; } = new UnknownBossTimer();

    public static double? ManualSecondsToBoss { get; set; }

    public static double? GetSecondsToBoss() => ManualSecondsToBoss ?? BossTimer.GetSecondsToBoss();

    public static void ClearManual() => ManualSecondsToBoss = null;

    public static void SetManualSecondsToBoss(double? seconds) => ManualSecondsToBoss = seconds;
}
