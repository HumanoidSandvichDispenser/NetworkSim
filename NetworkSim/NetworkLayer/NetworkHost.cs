namespace NetworkSim.NetworkLayer;

/// <summary>
/// Represents a L3 network host with a single IPv4 network interface,
/// capable of sending and receiving datagrams and sending L4 payloads
/// to higher-level handlers.
/// </summary>
public class NetworkHost : NetworkNode, IDrawable
{
    public Vector2 Position
    {
        get => Interface.LinkNode?.Position ?? Vector2.Zero;
        set
        {
            if (Interface.LinkNode is not null)
            {
                Interface.LinkNode.Position = value;
            }
        }
    }

    public bool Visible { get; set; } = true;

    /// <summary>
    /// The default gateway IP address for this host.
    /// </summary>
    public IpAddress DefaultGateway { get; set; }

    public NetworkHost(IpAddress ip) : base()
    {
        Interface = new NetworkInterface();
        Interface.IpAddress = ip;
        Interface.LinkNode = new LinkLayer.LinkEndpoint();
        Interface.DatagramReceived += OnDatagramReceived;
        Interface.ArpPayloadReceived += OnArpPayloadReceived;
    }

    public NetworkInterface Interface { get; }

    public Action<TransportLayer.Segment?>? SegmentReceived;

    public override void SendDatagram(Datagram datagram, IpAddress nextHop)
    {
        SendDatagram(datagram, nextHop, Interface);
    }

    private void OnDatagramReceived(NetworkInterface ni, Datagram? datagram)
    {
        if (datagram is null)
        {
            return;
        }

        SegmentReceived?.Invoke(datagram.Segment);
    }

    /// <summary>
    /// Routes a datagram to the next hop IP address, using local subnet
    /// information and the default gateway.
    /// </summary>
    public override IpAddress? Route(Datagram datagram, NetworkInterface? from)
    {
        // if the datagram is for us, drop
        if (datagram.DestinationIp == Interface.IpAddress)
        {
            return null;
        }

        // if the destination is in the same subnet, send directly
        if (Interface.IsInSameSubnet(datagram.DestinationIp))
        {
            return datagram.DestinationIp;
        }

        // otherwise, send to default gateway
        return DefaultGateway;
    }

    public override void Update(float delta)
    {
        
    }

    public override void AddToWorld(World world)
    {
        base.AddToWorld(world);

        if (Interface.LinkNode is not null)
        {
            world.AddEntity(Interface.LinkNode);
        }
    }

    public void Draw()
    {
        Raylib_cs.Raylib.DrawCircleV(Position, 20, Raylib_cs.Color.LightGray);
    }
}
