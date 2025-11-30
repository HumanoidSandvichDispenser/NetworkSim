using Raylib_cs;

namespace NetworkSim.NetworkLayer;

/// <summary>
/// Represents a L3 router with multiple IPv4 network interfaces. This class
/// also has optional simulated processing delay for incoming datagrams.
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

    public RoutingTable<int> InterfaceTable { get; } = new();

    private float _processingTimer = 0;

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

    public override void SendDatagram(Datagram datagram, IpAddress nextHop)
    {
        int? index = InterfaceTable.GetFirstMatch(nextHop);

        if (index.HasValue && index.Value >= 0 && index.Value < Interfaces.Length)
        {
            SendDatagram(datagram, nextHop, Interfaces[index.Value]);
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

    private void OnDatagramReceived(NetworkInterface from, Datagram? datagram)
    {
        if (datagram is null)
        {
            return;
        }

        if (datagram.DestinationIp == from.IpAddress)
        {
            return;
        }

        // ignore ARP packets for routing
        if (datagram is ArpPayload)
        {
            return;
        }

        if (ProcessingDelay <= 0)
        {
            HandleDatagram(from, datagram);
        }
        else
        {
            ProcessingQueue.Enqueue((datagram, from));
        }
    }

    private void HandleDatagram(NetworkInterface from, Datagram datagram)
    {
        IpAddress? nextHop = Route(datagram, from);

        if (nextHop is not null && nextHop.Value != from.IpAddress)
        {
            SendDatagram(datagram, nextHop.Value);
        }
    }

    public override void Update(float delta)
    {
        _processingTimer -= delta;
        if (_processingTimer <= 0 && ProcessingDelay > 0)
        {
            Console.WriteLine($"Router processing queue length: {ProcessingQueue.Count}");
            if (ProcessingQueue.Count > 0)
            {
                (Datagram datagram, NetworkInterface from) = ProcessingQueue.Dequeue();
                HandleDatagram(from, datagram);
            }
            _processingTimer += ProcessingDelay;
        }
    }

    public void Draw()
    {
        Raylib.DrawCircleV(Position, 15, Color.SkyBlue);
    }
}
