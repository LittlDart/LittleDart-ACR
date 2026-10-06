using Dalamud.Game.Text;
using Dalamud.Game.Text.SeStringHandling;
using Dalamud.Game.Text.SeStringHandling.Payloads;
using ECommons.DalamudServices;
using Lumina.Excel;
using Lumina.Excel.Sheets;

namespace LittleDart.Paladin;

public static class PldLogTag
{
    public const string Plain = "[LittleDart-PLD]";

    public const uint LittleDartRgb = 0x6CA0DC;

    public const uint PldRgb = 0x00FFFF;

    private const ushort MessageColorKey = 24;

    private const ushort IconColorKey = 14;

    private const string TagHead = "[LittleDart-PLD";

    private static ushort? _keyLittleDart;
    private static ushort? _keyPld;

    public static ushort KeyLittleDart => _keyLittleDart ??= ResolveColorKey(LittleDartRgb);

    public static ushort KeyPld => _keyPld ??= ResolveColorKey(PldRgb);

    public static SeString BuildChatMessage(string taggedMessage)
    {
        SeStringBuilder sb = new();

        sb.AddUiForeground(SeIconChar.BoxedLetterA.ToIconString(), IconColorKey);
        sb.AddUiForeground(SeIconChar.BoxedLetterE.ToIconString(), IconColorKey);

        if (!taggedMessage.StartsWith(TagHead, StringComparison.Ordinal))
        {
            return sb.AddUiForeground(" " + taggedMessage, MessageColorKey).Build();
        }

        string rest = taggedMessage[TagHead.Length..];

        sb.AddUiForeground(" [", MessageColorKey);
        sb.AddUiForeground("LittleDart", KeyLittleDart);
        sb.AddUiForeground("-", MessageColorKey);
        sb.AddUiForeground("PLD", KeyPld);
        sb.AddUiForeground(rest, MessageColorKey);

        return sb.Build();
    }

    private static ushort ResolveColorKey(uint targetRgb)
    {
        try
        {
            ExcelSheet<UIColor>? sheet = Svc.Data?.GetExcelSheet<UIColor>();
            if (sheet is null)
            {
                return MessageColorKey;
            }

            ushort bestKey = MessageColorKey;
            long bestDistance = long.MaxValue;

            foreach (UIColor row in sheet)
            {
                uint packed = row.Dark;

                long d1 = RgbDistance(packed >> 8, targetRgb);
                long d2 = RgbDistance(packed, targetRgb);
                long d = Math.Min(d1, d2);

                if (d < bestDistance)
                {
                    bestDistance = d;
                    bestKey = (ushort)row.RowId;
                }
            }

            return bestKey;
        }
        catch
        {
            return MessageColorKey;
        }
    }

    private static long RgbDistance(uint packed, uint targetRgb)
    {
        long r = (long)((packed >> 16) & 0xFF) - ((targetRgb >> 16) & 0xFF);
        long g = (long)((packed >> 8) & 0xFF) - ((targetRgb >> 8) & 0xFF);
        long b = (long)(packed & 0xFF) - (targetRgb & 0xFF);
        return (r * r) + (g * g) + (b * b);
    }
}
