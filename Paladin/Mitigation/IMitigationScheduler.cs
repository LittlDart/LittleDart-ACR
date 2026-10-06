namespace LittleDart.Paladin.Mitigation;

public interface IMitigationScheduler
{
    bool Enabled { get; }

    void Tick();

    bool ShouldCast(uint skillId);

    void NotifyCast(uint skillId);

    void Invalidate();

    void Reset();

    MitigationDecision? GetCurrentDecision();
}

public readonly struct MitigationDecision
{
    public readonly uint SkillId;

    public readonly MitigationCategory Category;

    public readonly string Reason;

    public MitigationDecision(uint skillId, MitigationCategory category, string reason)
    {
        SkillId = skillId;
        Category = category;
        Reason = reason;
    }
}
