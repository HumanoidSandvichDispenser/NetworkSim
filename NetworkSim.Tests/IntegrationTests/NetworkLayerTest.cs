namespace NetworkSim.Tests.IntegrationTests;

public class NetworkLayerTest
{
    [Fact]
    public void Frame_ShouldTransmitAcrossRouter()
    {
        var world = new World();

        var router = new NetworkLayer.IpTablesRouter();

        var datagram = new NetworkLayer.Datagram();
        datagram.DestinationIp = "8.8.8.8";

        var frame = new LinkLayer.Frame();
        frame.Datagram = datagram;

        router.Interfaces[0].LinkNode = new LinkLayer.Switch("1");
        router.Interfaces[1].LinkNode = new LinkLayer.Switch("2");

        router.IpTable.Entries.Add(new(0, 0, "8.8.8.8"));
        router.InterfaceTable.Entries.Add(new("8.8.8.8", 0, 1));

        var switchA = new LinkLayer.Switch("a");
        var switchB = new LinkLayer.Switch("b");

        frame.SourceMac = switchA.MacAddress;
        frame.DestinationMac = router.Interfaces[0].LinkNode!.MacAddress;

        router.ArpTable["8.8.8.8"] = switchB.MacAddress;

        world.AddEntity(router);
        world.AddEntity(switchA);
        world.AddEntity(switchB);

        switchA.LinkWith(router.Interfaces[0].LinkNode!);
        switchB.LinkWith(router.Interfaces[1].LinkNode!);

        LinkLayer.Frame? receivedFrame = null;
        switchB.FrameReceived += (f) => receivedFrame = f;

        switchA.SendFrame(frame);
        world.Update(1.0f, 20);

        receivedFrame.ShouldNotBeNull();
        receivedFrame.Datagram.ShouldBe(datagram);
    }

    [Theory]
    [InlineData("00:FE:AB:00:02:01", "00:FE:AB:82:FF:FE")]
    [InlineData("AA:BB:CC:DD:EE:FF", "11:22:33:44:55:66")]
    public void Routers_ShouldRespondToArp(string macA, string macB)
    {
        var world = new World();

        var routerA = new NetworkLayer.IpTablesRouter(1);
        routerA.Interfaces[0].LinkNode = new LinkLayer.LinkEndpoint(macA);
        routerA.Interfaces[0].IpAddress = "192.168.1.1";
        routerA.IpTable.Entries.Add(new(0, 0, "192.168.1.2"));
        routerA.InterfaceTable.Entries.Add(new(0, 0, 0));

        var routerB = new NetworkLayer.IpTablesRouter(1);
        routerB.Interfaces[0].LinkNode = new LinkLayer.LinkEndpoint(macB);
        routerB.Interfaces[0].IpAddress = "192.168.1.2";
        routerB.IpTable.Entries.Add(new(0, 0, "192.168.1.1"));
        routerB.InterfaceTable.Entries.Add(new(0, 0, 0));

        world.AddEntity(routerA);
        world.AddEntity(routerB);

        routerA.Interfaces[0].LinkNode!.LinkWith(routerB.Interfaces[0].LinkNode!);

        var datagram = new NetworkLayer.Datagram
        {
            SourceIp = "192.168.1.1",
            DestinationIp = "192.168.1.2",
        };

        routerA.SendDatagram(datagram);

        world.Update(1.0f, 20);

        routerB.ArpTable.ShouldNotBeEmpty();
        routerB.ArpTable.ShouldContainKeyAndValue("192.168.1.1", macA);
        routerA.ArpTable.ShouldContainKeyAndValue("192.168.1.2", macB);
    }

    [Fact]
    public void ThreeRouters_ShouldRouteAndRespondToArp()
    {
        var world = new World();

        var routerA = new NetworkLayer.IpTablesRouter(1);
        routerA.Interfaces[0].LinkNode = new LinkLayer.LinkEndpoint(":0A");
        routerA.Interfaces[0].IpAddress = "192.168.1.34";
        routerA.IpTable.Entries.Add(new(0, 0, "192.168.1.1"));
        routerA.InterfaceTable.Entries.Add(new(0, 0, 0));

        var routerB = new NetworkLayer.IpTablesRouter(2);
        routerB.Interfaces[0].LinkNode = new LinkLayer.LinkEndpoint(":0B");
        routerB.Interfaces[0].IpAddress = "192.168.1.1";
        routerB.Interfaces[1].LinkNode = new LinkLayer.LinkEndpoint(":1B");
        routerB.Interfaces[1].IpAddress = "10.100.100.1";
        routerB.IpTable.Entries.Add(new("10.0.0.0", "255.0.0.0", "10.4.35.2"));
        routerB.IpTable.Entries.Add(new(0, 0, 0));
        routerB.InterfaceTable.Entries.Add(new("10.0.0.0", "255.0.0.0", 1));
        routerB.InterfaceTable.Entries.Add(new(0, 0, 0));

        var routerC = new NetworkLayer.IpTablesRouter(1);
        routerC.Interfaces[0].LinkNode = new LinkLayer.LinkEndpoint(":0C");
        routerC.Interfaces[0].IpAddress = "10.4.35.2";
        routerC.IpTable.Entries.Add(new(0, 0, 0));
        routerC.InterfaceTable.Entries.Add(new(0, 0, 0));

        world.AddEntity(routerA);
        world.AddEntity(routerB);
        world.AddEntity(routerC);

        routerA.Interfaces[0].LinkNode!.LinkWith(routerB.Interfaces[0].LinkNode!);
        routerB.Interfaces[1].LinkNode!.LinkWith(routerC.Interfaces[0].LinkNode!);

        var datagram = new NetworkLayer.Datagram
        {
            SourceIp = routerA.Interfaces[0].IpAddress,
            DestinationIp = routerC.Interfaces[0].IpAddress,
        };

        routerA.SendDatagram(datagram);

        world.Update(1.0f, 20);

        routerA.ArpTable.ShouldContainKeyAndValue("192.168.1.1", ":0B");
        routerB.ArpTable.ShouldContainKeyAndValue("192.168.1.34", ":0A");
        routerB.ArpTable.ShouldContainKeyAndValue("10.4.35.2", ":0C");
        routerC.ArpTable.ShouldContainKeyAndValue("10.100.100.1", ":1B");
    }

    [Fact]
    public void Hosts_ShouldTransmitDatagrams()
    {
        var world = new World();

        var router = new NetworkLayer.IpTablesRouter(1);
        router.Interfaces[0].LinkNode = new LinkLayer.Switch(":00");
        router.Interfaces[0].IpAddress = "192.168.5.1";

        var hostA = new NetworkLayer.NetworkHost("192.168.5.3");
        var hostB = new NetworkLayer.NetworkHost("192.168.5.8");

        world.AddEntity(router);
        world.AddEntity(hostA);
        world.AddEntity(hostB);

        hostA.Interface.LinkNode!.MacAddress = ":A0";
        hostA.Interface.LinkNode!.LinkWith(router.Interfaces[0].LinkNode!);

        hostB.Interface.LinkNode!.MacAddress = ":B0";
        hostB.Interface.LinkNode!.LinkWith(router.Interfaces[0].LinkNode!);

        var datagram = new NetworkLayer.Datagram
        {
            SourceIp = hostA.Interface.IpAddress,
            DestinationIp = hostB.Interface.IpAddress,
        };

        bool received = false;
        hostB.SegmentReceived = (segment) => received = true;

        hostA.SendDatagram(datagram);

        world.Update(1.0f, 20);

        hostA.ArpQueue.ShouldNotContainKey(hostB.Interface.IpAddress);
        received.ShouldBeTrue();
    }

    [Fact]
    public void DefaultGatewayRouter_ShouldRouteToExternalNetwork()
    {
        var world = new World();
        world.TimeScale = 0.002f;

        var router = new NetworkLayer.IpTablesRouter(2);
        router.Interfaces[0].LinkNode = new LinkLayer.Switch(":00");
        router.Interfaces[0].IpAddress = "192.168.5.1";
        router.Interfaces[1].LinkNode = new LinkLayer.LinkEndpoint(":08");
        router.Interfaces[1].IpAddress = "10.100.1.1";
        router.IpTable.Entries.Add(new("10.0.0.0", "255.0.0.0", "10.100.2.1"));
        router.InterfaceTable.Entries.Add(new("10.0.0.0", "255.0.0.0", 1));

        var router2 = new NetworkLayer.IpTablesRouter(1);
        router2.Interfaces[0].LinkNode = new LinkLayer.Switch(":FF");
        router2.Interfaces[0].IpAddress = "10.100.2.1";
        router2.IpTable.Entries.Add(new(0, 0, "10.100.1.1"));
        router2.InterfaceTable.Entries.Add(new(0, 0, 0));

        var hostA = new NetworkLayer.NetworkHost("192.168.5.3");
        var hostB = new NetworkLayer.NetworkHost("192.168.5.8");

        world.AddEntity(router);
        world.AddEntity(router2);
        world.AddEntity(hostA);
        world.AddEntity(hostB);

        hostA.Interface.LinkNode!.MacAddress = ":A1";
        hostA.Interface.LinkNode!.LinkWith(router.Interfaces[0].LinkNode!);
        hostA.DefaultGateway = router.Interfaces[0].IpAddress;

        hostB.Interface.LinkNode!.MacAddress = ":B2";
        hostB.Interface.LinkNode!.LinkWith(router.Interfaces[0].LinkNode!);
        hostB.DefaultGateway = router.Interfaces[0].IpAddress;

        router.Interfaces[1].LinkNode!.LinkWith(router2.Interfaces[0].LinkNode!);

        var datagram = new NetworkLayer.Datagram
        {
            SourceIp = hostA.Interface.IpAddress,
            DestinationIp = "10.100.2.1",
        };

        hostB.SendDatagram(datagram);

        bool received = false;
        router2.Interfaces[0].DatagramReceived += (ni, d) =>
        {
            if (d == datagram)
            {
                received = true;
            }
        };

        world.Update(1.0f, 20);

        received.ShouldBeTrue();
    }
}
