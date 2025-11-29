using NetworkSim.NetworkLayer;

namespace NetworkSim.Tests;

public class IpTablesRouterTests
{
    [Fact]
    public void Empty_ShouldDropAll()
    {
        var router = new IpTablesRouter();
        var datagram = new Datagram();

        datagram.DestinationIp = 0x12345678;

        router.Interfaces[0] = new NetworkInterface();

        var nextHop = router.Route(datagram, router.Interfaces[0]);
        nextHop.ShouldBeNull();
    }

    [Fact]
    public void Router_ShouldMatchLongestMatch()
    {
        var router = new IpTablesRouter();
        var datagram = new Datagram();
        datagram.DestinationIp = 0xAB00;

        router.RoutingTable.Entries.Add(new(0xAB00, 0xFFFFFF00, 0));
        router.RoutingTable.Entries.Add(new(0xAB00, 0xFFFFFFFF, 1));

        var nextHop = router.Route(datagram, router.Interfaces[0]);
        nextHop.ShouldBe(router.Interfaces[1]);
    }

    [Fact]
    public void Router_ShouldMatchFirstMatch()
    {
        var router = new IpTablesRouter();
        router.UseLongestPrefixMatch = false;

        var datagram = new Datagram();
        datagram.DestinationIp = 0xAB00;

        router.RoutingTable.Entries.Add(new(0xAB00, 0xFFFFFF00, 0));
        router.RoutingTable.Entries.Add(new(0xAB00, 0xFFFFFFFF, 1));

        var nextHop = router.Route(datagram, router.Interfaces[0]);
        nextHop.ShouldBe(router.Interfaces[0]);
    }
}
