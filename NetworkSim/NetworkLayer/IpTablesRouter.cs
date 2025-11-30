namespace NetworkSim.NetworkLayer;

/// <summary>
/// A router that uses static tables for routing decisions, either first-match
/// or longest-prefix-match.
/// </summary>
public class IpTablesRouter : Router
{
    public RoutingTable<IpAddress> IpTable { get; } = new();

    /// <summary>
    /// If true, uses longest-prefix-match routing. If false, uses first-match
    /// routing.
    /// </summary>
    public bool UseLongestPrefixMatch { get; set; } = true;

    public IpTablesRouter(int numInterfaces = 2) : base(numInterfaces)
    {

    }

    public override IpAddress? Route(Datagram datagram, NetworkInterface? from)
    {
        if (UseLongestPrefixMatch)
        {
            return IpTable.GetLongestPrefixMatch(datagram.DestinationIp);
        }
        else
        {
            return IpTable.GetFirstMatch(datagram.DestinationIp);
        }
    }
}
