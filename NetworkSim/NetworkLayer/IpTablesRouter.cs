namespace NetworkSim.NetworkLayer;

/// <summary>
/// A router that uses static tables for routing decisions, either first-match
/// or longest-prefix-match.
/// </summary>
public class IpTablesRouter : Router
{
    public class StaticRoutingTable
    {
        public record Entry(uint Destination, uint Mask, int Index);

        public readonly List<Entry> Entries = new();

        public int? GetFirstMatch(uint destinationIp)
        {
            foreach (var entry in Entries)
            {
                if ((destinationIp & entry.Mask) == (entry.Destination & entry.Mask))
                {
                    return entry.Index;
                }
            }

            return null;
        }

        public int? GetLongestPrefixMatch(uint destinationIp)
        {
            Entry? bestMatch = null;
            uint bestMask = 0;

            foreach (var entry in Entries)
            {
                if ((destinationIp & entry.Mask) == (entry.Destination & entry.Mask))
                {
                    if (entry.Mask > bestMask)
                    {
                        bestMask = entry.Mask;
                        bestMatch = entry;
                    }
                }
            }

            return bestMatch?.Index;
        }
    }

    public StaticRoutingTable RoutingTable { get; } = new();

    /// <summary>
    /// If true, uses longest-prefix-match routing. If false, uses first-match
    /// routing.
    /// </summary>
    public bool UseLongestPrefixMatch { get; set; } = true;

    public IpTablesRouter(int numInterfaces = 2) : base(numInterfaces)
    {

    }

    public override NetworkInterface? Route(Datagram datagram, NetworkInterface from)
    {
        int? index;
        if (UseLongestPrefixMatch)
        {
            index = RoutingTable.GetLongestPrefixMatch(datagram.DestinationIp);
        }
        else
        {
            index = RoutingTable.GetFirstMatch(datagram.DestinationIp);
        }

        if (index.HasValue && index.Value >= 0 && index.Value < Interfaces.Length)
        {
            return Interfaces[index.Value];
        }

        return null;
    }

    public override void Update(float delta)
    {

    }
}
