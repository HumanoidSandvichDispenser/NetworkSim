namespace NetworkSim.NetworkLayer;

/// <summary>
/// Represents a L3 IPv4 network interface.
/// </summary>
public class NetworkInterface
{
    private LinkLayer.LinkNode? _linkNode;

    /// <summary>
    /// The link endpoint associated with this network interface.
    /// </summary>
    public LinkLayer.LinkNode? LinkNode
    {
        get => _linkNode;
        set
        {
            if (_linkNode is not null)
            {
                // Unsubscribe from previous link node events
                _linkNode.FrameReceived -= OnFrameReceived;
            }

            _linkNode = value;

            if (_linkNode is not null)
            {
                _linkNode.FrameReceived += OnFrameReceived;
            }
        }
    }

    public event Action<NetworkInterface, Datagram?>? DatagramReceived;

    public event Action<ArpPayload, LinkLayer.Frame, NetworkInterface>? ArpPayloadReceived;

    /// <summary>
    /// The IPv4 address assigned to this network interface.
    /// </summary>
    public uint IpAddress { get; set; } = 0;

    /// <summary>
    /// The subnet mask assigned to this network interface.
    /// </summary>
    public uint SubnetMask { get; set; } = 0xFF_FF_FF_00;

    /// <summary>
    /// Returns the uint representation of a dotted-decimal IPv4 address.
    /// </summary>
    public static uint AddressFromString(string ipAddress)
    {
        uint addr = 0;
        foreach (byte octet in ipAddress.Split('.').Select(byte.Parse))
        {
            addr = (addr << 8) | octet;
        }
        return addr;
    }

    public bool IsInSameSubnet(uint otherIp)
    {
        return (IpAddress & SubnetMask) == (otherIp & SubnetMask);
    }

    private void OnFrameReceived(LinkLayer.Frame frame)
    {
        Console.WriteLine($"NetworkInterface received frame with datagram from {frame.SourceMac} to {frame.DestinationMac}");

        DatagramReceived?.Invoke(this, frame.Datagram);

        if (frame.Datagram is ArpPayload arpPayload)
        {
            Console.WriteLine($"Frame contains ARP payload: {arpPayload}");
            ArpPayloadReceived?.Invoke(arpPayload, frame, this);
        }
    }
}
