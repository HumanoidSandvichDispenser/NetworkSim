using Raylib_cs;

namespace NetworkSim.NetworkLayer;

/// <summary>
/// Represents a L3 router with multiple IPv4 network interfaces.
/// </summary>
public abstract class Router : NetworkNode, IDrawable
{
    private Vector2 _position;

    public Vector2 Position
    {
        get => _position;
        set
        {
            _position = value;

            foreach (NetworkInterface? net in Interfaces)
            {
                if (net?.LinkNode is not null)
                {
                    net.LinkNode.Position = value;
                }
            }
        }
    }

    public bool Visible { get; set; } = true;

    public NetworkInterface[] Interfaces { get; }

    public Router(int numInterfaces = 2)
    {
        Interfaces = new NetworkInterface[numInterfaces];

        for (int i = 0; i < numInterfaces; i++)
        {
            Interfaces[i] = new NetworkInterface();
            Interfaces[i].DatagramReceived += OnDatagramReceived;
            Interfaces[i].ArpPayloadReceived += OnArpPayloadReceived;
        }
    }

    public override void AddToWorld(World world)
    {
        base.AddToWorld(world);

        foreach (NetworkInterface? net in Interfaces)
        {
            if (net?.LinkNode is not null)
            {
                world.AddEntity(net.LinkNode);
            }
        }
    }

    /// <summary>
    /// Routes a datagram arriving on a specific interface to the next hop
    /// interface. Returns null (drop) if no route is found.
    /// </summary>
    public abstract NetworkInterface? Route(Datagram datagram, NetworkInterface from);

    public void OnDatagramReceived(NetworkInterface from, Datagram? datagram)
    {
        if (datagram is null)
        {
            return;
        }

        if (datagram.DestinationIp == from.IpAddress)
        {
            return;
        }

        NetworkInterface? nextHop = Route(datagram, from);

        if (nextHop is not null && nextHop != from)
        {
            SendDatagram(datagram, nextHop);
        }
    }

    public void Draw()
    {
        Raylib.DrawCircleV(Position, 15, Color.SkyBlue);
    }
}
