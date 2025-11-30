using NetworkSim.NetworkLayer;

namespace NetworkSim.Tests;

public class IpTablesRouterTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(uint.MaxValue)]
    public void Empty_ShouldDropAll(uint destination)
    {
        var router = new IpTablesRouter();
        var datagram = new Datagram();
        datagram.DestinationIp = destination;
        router.Interfaces[0] = new NetworkInterface();

        var nextHop = router.Route(datagram, null);

        nextHop.ShouldBeNull();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(uint.MaxValue)]
    public void Router_ShouldMatchAll(uint destination)
    {
        var router = new IpTablesRouter();
        var datagram = new Datagram();
        datagram.DestinationIp = destination;
        router.Interfaces[0] = new NetworkInterface();

        router.IpTable.Entries.Add(new(0, 0, "2.3.4.5"));

        var nextHop = router.Route(datagram, null);

        nextHop.ShouldNotBeNull();
        nextHop.Value.Address.ShouldBe(new IpAddress("2.3.4.5").Address);
    }

    [Fact]
    public void Router_ShouldMatchLongestMatch()
    {
        var router = new IpTablesRouter();
        var datagram = new Datagram();

        datagram.DestinationIp = "192.168.0.34";
        router.IpTable.Entries.Add(new("192.168.0.0", "255.255.0.0", "192.168.0.1"));
        router.IpTable.Entries.Add(new("192.168.0.0", "255.255.255.0", "192.168.0.2"));

        var nextHop = router.Route(datagram, null);

        nextHop.ShouldNotBeNull();
        nextHop.Value.Address.ShouldBe(new IpAddress("192.168.0.2").Address);
    }

    [Fact]
    public void Router_ShouldMatchFirstMatch()
    {
        var router = new IpTablesRouter();
        router.UseLongestPrefixMatch = false;
        var datagram = new Datagram();

        datagram.DestinationIp = "192.168.0.34";
        router.IpTable.Entries.Add(new("192.168.0.0", "255.255.0.0", "192.168.0.1"));
        router.IpTable.Entries.Add(new("192.168.0.0", "255.255.255.0", "192.168.0.2"));

        var nextHop = router.Route(datagram, null);

        nextHop.ShouldNotBeNull();
        nextHop.Value.Address.ShouldBe(new IpAddress("192.168.0.1").Address);
    }
}
