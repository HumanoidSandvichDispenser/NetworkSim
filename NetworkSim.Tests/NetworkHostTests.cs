namespace NetworkSim.NetworkLayer;

public class NetworkHostTests
{
    [Fact]
    public void NetworkHost_ShouldNotRouteToSelf()
    {
        var host = new NetworkHost("192.168.1.8");
        host.DefaultGateway = "192.168.1.1";
        host.Interface.SubnetMask = "255.255.255.0";

        var datagram = new Datagram();
        datagram.DestinationIp = host.Interface.IpAddress;

        host.Route(datagram, null).ShouldBeNull();
    }

    [Fact]
    public void NetworkHost_ShouldRouteToDefaultGateway()
    {
        var host = new NetworkHost("192.168.1.8");
        host.DefaultGateway = "192.168.1.1";
        host.Interface.SubnetMask = "255.255.255.0";

        var datagram = new Datagram();
        datagram.DestinationIp = "1.1.1.1";

        host.Route(datagram, null).ShouldBe(host.DefaultGateway);
    }

    [Fact]
    public void NetworkHost_ShouldRouteToLan()
    {
        var host = new NetworkHost("192.168.1.8");
        host.DefaultGateway = "192.168.1.1";
        host.Interface.SubnetMask = "255.255.255.0";

        var datagram = new Datagram();
        datagram.DestinationIp = "192.168.1.25";

        host.Route(datagram, null).ShouldBe(datagram.DestinationIp);
    }
}
