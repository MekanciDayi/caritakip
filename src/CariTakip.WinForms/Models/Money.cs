namespace CariTakip.Models;

internal static class Money
{
    internal static long ToKurus(decimal value)
    {
        return (long)Math.Round(value * 100m, 0, MidpointRounding.AwayFromZero);
    }

    internal static decimal FromKurus(long kurus)
    {
        return kurus / 100m;
    }
}
