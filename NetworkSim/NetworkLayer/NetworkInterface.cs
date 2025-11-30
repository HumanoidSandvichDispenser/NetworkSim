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

    /// <summary>
    /// Event triggered when a datagram is received on this network interface.
    /// </summary>
    public event Action<NetworkInterface, Datagram?>? DatagramReceived;

    /// <summary>
    /// Event triggered when an ARP payload is received on this network interface.
    /// </summary>
    public event Action<ArpPayload, LinkLayer.Frame, NetworkInterface>? ArpPayloadReceived;

    /// <summary>
    /// The IPv4 address assigned to this network interface.
    /// </summary>
    public IpAddress IpAddress { get; set; } = new("0.0.0.0");

    /// <summary>
    /// The subnet mask assigned to this network interface.
    /// </summary>
    public IpAddress SubnetMask { get; set; } = 0xFF_FF_FF_00;

    public bool IsInSameSubnet(IpAddress otherIp)
    {
        return otherIp.Matches(IpAddress, SubnetMask);
    }

    private void OnFrameReceived(LinkLayer.Frame frame)
    {
        DatagramReceived?.Invoke(this, frame.Datagram);

        if (frame.Datagram is ArpPayload arpPayload)
        {
            ArpPayloadReceived?.Invoke(arpPayload, frame, this);
        }
    }
}
