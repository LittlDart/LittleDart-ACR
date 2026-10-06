using AEAssist;
using AEAssist.Extension;

namespace LittleDart.Paladin.Mitigation.Calc;

public sealed class HpTracker
{
    private readonly List<(long T, float Hp)> _self = new();
    private readonly List<(long T, float Hp)> _target = new();

    private long _lastSelfSampleMs;
    private long _lastTargetSampleMs;
    private ulong _targetId;
    private long _lastSeenTargetMs;

    public void Reset()
    {
        _self.Clear();
        _target.Clear();
        _lastSelfSampleMs = 0;
        _lastTargetSampleMs = 0;
        _targetId = 0;
        _lastSeenTargetMs = 0;
    }

    public void Tick(long now, int sampleIntervalMs, int keepWindowMs)
    {
        if (now - _lastSelfSampleMs >= sampleIntervalMs)
        {
            _lastSelfSampleMs = now;
            _self.Add((now, Core.Me.CurrentHpPercent()));
            Prune(_self, now - (long)keepWindowMs * 3L);
        }

        var target = PldHelper.GetCurrentTarget();
        if (target == null)
        {
            if (_target.Count > 0 && now - _lastSeenTargetMs > 2000)
            {
                _target.Clear();
                _targetId = 0;
            }

            return;
        }

        _lastSeenTargetMs = now;

        if (target.GameObjectId != _targetId)
        {
            _target.Clear();
            _targetId = target.GameObjectId;
        }

        if (now - _lastTargetSampleMs >= sampleIntervalMs)
        {
            _lastTargetSampleMs = now;
            _target.Add((now, target.CurrentHpPercent()));
            Prune(_target, now - (long)keepWindowMs * 3L);
        }
    }

    public float SelfDamageTakenPercent(int windowMs) => CumulativeDropPercent(_self, windowMs);

    public float SelfNetDeclinePerSecond(int windowMs) => NetDeclinePerSecond(_self, windowMs);

    public float TargetNetDeclinePerSecond(int windowMs) => NetDeclinePerSecond(_target, windowMs);

    public float TargetHpPercent()
    {
        var target = PldHelper.GetCurrentTarget();
        return target == null ? -1f : target.CurrentHpPercent();
    }

    public bool HasTargetHistory => _target.Count >= 2;

    private static void Prune(List<(long T, float Hp)> list, long cutoff)
    {
        while (list.Count > 0 && list[0].T < cutoff)
        {
            list.RemoveAt(0);
        }
    }

    private static float CumulativeDropPercent(List<(long T, float Hp)> list, int windowMs)
    {
        long from = Environment.TickCount64 - windowMs;
        float sum = 0f;
        for (int i = 1; i < list.Count; i++)
        {
            var prev = list[i - 1];
            var cur = list[i];
            if (cur.T < from || prev.T < from - 500)
            {
                continue;
            }

            float drop = prev.Hp - cur.Hp;
            if (drop > 0f)
            {
                sum += drop;
            }
        }

        return sum * 100f;
    }

    private static float NetDeclinePerSecond(List<(long T, float Hp)> list, int windowMs)
    {
        if (list.Count < 2)
        {
            return 0f;
        }

        long now = Environment.TickCount64;
        long from = now - windowMs;

        (long T, float Hp) baseline = list[0];
        foreach (var sample in list)
        {
            if (sample.T <= from)
            {
                baseline = sample;
            }
            else
            {
                break;
            }
        }

        var last = list[^1];
        double seconds = (last.T - baseline.T) / 1000d;
        if (seconds < 0.5d)
        {
            return 0f;
        }

        float drop = (baseline.Hp - last.Hp) * 100f;
        if (drop <= 0f)
        {
            return 0f;
        }

        return (float)(drop / seconds);
    }
}
