namespace NetworkSim;

/// <summary>
/// Factory methods for creating network components.
/// </summary>
public static class NetworkFactory
{
    /// <summary>
    /// Creates a network host and connects it to the specified gateway interface.
    /// </summary>
    public static NetworkLayer.NetworkHost CreateHost(
        World world,
        NetworkLayer.IpAddress ipAddr,
        string macAddr,
        NetworkLayer.NetworkInterface gatewayInterface)
    {
        var host = new NetworkLayer.NetworkHost(ipAddr);
        host.DefaultGateway = gatewayInterface.IpAddress;
        host.Interface.SubnetMask = gatewayInterface.SubnetMask;
        host.Interface.LinkNode!.MacAddress = macAddr;

        world.AddEntity(host);
        host.Interface.LinkNode!.LinkWith(gatewayInterface.LinkNode!);

        return host;
    }

    /// <summary>
    /// Creates a 2-interface gateway router. The first interface is for the
    /// LAN side, and the second is for the WAN side. This only creates the
    /// router and local LAN interface; the WAN interface must be connected
    /// separately.
    /// </summary>
    public static NetworkLayer.IpTablesRouter CreateGateway(
        World world,
        NetworkLayer.IpAddress localIpAddr,
        NetworkLayer.IpAddress localSubnetMask)
    {
        var router = new NetworkLayer.IpTablesRouter(2);
        router.Interfaces[0].IpAddress = localIpAddr;
        router.Interfaces[0].SubnetMask = localSubnetMask;
        router.Interfaces[0].LinkNode = new LinkLayer.Switch();
        router.Interfaces[0].ArpPayloadReceived += (arp, _, _) =>
        {
             Console.WriteLine($"[Gateway LAN] Arp received: {arp.SourceIp}");
        };
        router.Interfaces[0].LinkNode!.MacAddress = ":01";
        router.Interfaces[1].LinkNode = new LinkLayer.LinkEndpoint();
        router.ProcessingDelay = 0.005f;

        world.AddEntity(router);

        return router;
    }

    public static IEnumerable<Entity> CreateLocalNetwork(
        World world,
        NetworkLayer.IpAddress ipAddr,
        NetworkLayer.IpAddress subnetMask,
        int numberOfHosts)
    {
        var gateway = CreateGateway(world, ipAddr, subnetMask);

        gateway.Position = new Vector2(400, 240);

        for (int i = 0; i < numberOfHosts; i++)
        {
            Vector2 offset = new Vector2((i % 5 + 1) * 80, (i / 5 + 1) * 80);
            offset -= new Vector2(240, 160);

            // generate random IP in subnet
            NetworkLayer.IpAddress net = ipAddr & subnetMask;
            NetworkLayer.IpAddress address = net + (uint)(i + 2);

            var host = CreateHost(
                world,
                address,
                $":{i + 2:X2}",
                gateway.Interfaces[0]);

            host.Position = gateway.Position + offset;
            yield return host;
        }

        yield return gateway;
    }
}
