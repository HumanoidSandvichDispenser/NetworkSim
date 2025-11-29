namespace NetworkSim.Tests.IntegrationTests;

public class NetworkLayerTest
{
    public void Frame_ShouldTransmitAcrossRouter()
    {
        var world = new World();

        //var router = Substitute.ForPartsOf<NetworkLayer.IpTablesRouter>(2);
        var router = new NetworkLayer.IpTablesRouter();

        var datagram = new NetworkLayer.Datagram();
        datagram.DestinationIp = 0xFF000000;

        var frame = new LinkLayer.Frame();
        frame.Datagram = datagram;

        router.Interfaces[0].LinkNode = new LinkLayer.Switch("1");
        router.Interfaces[1].LinkNode = new LinkLayer.Switch("2");

        router.RoutingTable.Entries.Add(new(0xFF000000, 0xFFFFFFFF, 1));

        var switchA = new LinkLayer.Switch("a");
        var switchB = new LinkLayer.Switch("b");

        frame.SourceMac = switchA.MacAddress;
        frame.DestinationMac = router.Interfaces[0].LinkNode!.MacAddress;

        router.ArpTable[0xFF000000] = switchB.MacAddress;

        world.AddEntity(router);
        world.AddEntity(router.Interfaces[0].LinkNode!);
        world.AddEntity(router.Interfaces[1].LinkNode!);
        world.AddEntity(switchA);
        world.AddEntity(switchB);
        world.AddEntity(frame);

        switchA.LinkWith(router.Interfaces[0].LinkNode!);
        switchB.LinkWith(router.Interfaces[1].LinkNode!);

        router.Route(datagram, router.Interfaces[0]).ShouldBe(router.Interfaces[1]);

        LinkLayer.Frame? receivedFrame = null;
        switchB.FrameReceived += (f) => receivedFrame = f;

        switchA.SendFrame(frame);
        world.Update(1.0f, 20);

        receivedFrame.ShouldNotBeNull();
        receivedFrame.Datagram.ShouldBe(datagram);
    }

    [Fact]
    public void Routers_ShouldRespondToArp()
    {
        var world = new World();

        var routerA = new NetworkLayer.IpTablesRouter();
        routerA.Interfaces[0].LinkNode = new LinkLayer.LinkEndpoint("A");
        routerA.Interfaces[0].IpAddress = 1;
        routerA.RoutingTable.Entries.Add(new(0, 0, 0));

        var routerB = new NetworkLayer.IpTablesRouter();
        routerB.Interfaces[0].LinkNode = new LinkLayer.LinkEndpoint("B");
        routerB.Interfaces[0].IpAddress = 2;
        routerB.RoutingTable.Entries.Add(new(0, 0, 0));

        world.AddEntity(routerA);
        world.AddEntity(routerB);

        routerA.Interfaces[0].LinkNode!.LinkWith(routerB.Interfaces[0].LinkNode!);

        var datagram = new NetworkLayer.Datagram
        {
            SourceIp = 1,
            DestinationIp = 2,
        };

        routerA.SendDatagram(datagram, routerA.Interfaces[0]);

        world.Update(1.0f, 20);

        routerB.ArpTable.ShouldContainKeyAndValue((uint)1, "A");
        routerA.ArpTable.ShouldContainKeyAndValue((uint)2, "B");
    }
}
