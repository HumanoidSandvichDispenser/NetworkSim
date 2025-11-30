namespace NetworkSim.NetworkLayer;

public class RoutingTable<TEntryValue> where TEntryValue : struct
{
    public record Entry(IpAddress Destination, IpAddress Mask, TEntryValue Value);

    public readonly List<Entry> Entries = new();

    public TEntryValue? GetFirstMatch(IpAddress destinationIp)
    {
        foreach (var entry in Entries)
        {
            if (destinationIp.Matches(entry.Destination, entry.Mask))
            {
                return entry.Value;
            }
        }

        return null;
    }

    public TEntryValue? GetLongestPrefixMatch(IpAddress destinationIp)
    {
        Entry? bestMatch = null;
        uint bestMask = 0;

        foreach (var entry in Entries)
        {
            if (destinationIp.Matches(entry.Destination, entry.Mask))
            {
                if (entry.Mask >= bestMask)
                {
                    bestMask = entry.Mask;
                    bestMatch = entry;
                }
            }
        }

        return bestMatch?.Value;
    }
}

