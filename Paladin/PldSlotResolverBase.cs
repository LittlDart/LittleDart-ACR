using AEAssist.CombatRoutine.Module;
using LittleDart.Paladin.Setting;

namespace LittleDart.Paladin;

public abstract class PldSlotResolverBase : ISlotResolver
{
    protected abstract string ResolverDescription { get; }

    protected abstract int CheckReal();

    public abstract void Build(Slot slot);

    public int Check()
    {
        if (LittleDartRotationEntry.QT.GetQt(PldQt.StopAttack))
        {
            return -1;
        }

        int result = CheckReal();

        if (Diagnostics.Enabled && result >= 0)
        {
            Diagnostics.OnHit(ResolverDescription, result);
        }

        return result;
    }

    protected static int SkipNotImplemented()
    {
        return -1;
    }
}
