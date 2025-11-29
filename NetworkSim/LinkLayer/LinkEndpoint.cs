namespace NetworkSim.LinkLayer;

/// <summary>
/// A LinkNode that represents an endpoint in the L2 network, such as a host or
/// router interface.
/// </summary>
public sealed class LinkEndpoint : LinkNode
{
    public override void Initialize()
    {
        Visible = false;
    }

    private Link? _connectedLink;

    private readonly FrameQueue _txQueue;

    public uint QueueSize { get; set; } = 4096;

    public LinkEndpoint(string macAddress = "00:00:00:00:00:00")
    {
        MacAddress = macAddress;
        _txQueue = new FrameQueue(QueueSize);
    }

    public override Link LinkWith(LinkNode node, Link? existingLink = null)
    {
        if (_connectedLink is not null)
        {
            base.Unlink(_connectedLink.GetOtherEndpoint(this));
        }

        Console.WriteLine($"LinkEndpoint {MacAddress} linking with {node.MacAddress}");

        _connectedLink = base.LinkWith(node);
        return _connectedLink;
    }

    public override bool Unlink(LinkNode node)
    {
        if (_connectedLink is null)
        {
            return false;
        }

        bool result = base.Unlink(node);
        if (result)
        {
            _connectedLink = null;
        }
        return result;
    }

    public override void SendFrame(Frame frame, Link? forwardFromLink = null)
    {
        Console.WriteLine($"LinkEndpoint {MacAddress} queueing frame to {frame.DestinationMac}");
        _txQueue.TryEnqueue(frame);
    }

    public override void ReceiveFrame(Frame frame, Link fromLink)
    {
        // do not forward frames; just pass them up to the network layer using
        // events

        if (frame.DestinationMac == MacAddress || frame.IsBroadcast())
        {
            InvokeFrameReceived(frame);
        }
    }

    public override void Update(float delta)
    {
        if (_connectedLink is null)
        {
            return;
        }

        if (_txQueue.TryDequeue(out Frame? frame))
        {
            if (frame is not null)
            {
                var destination = _connectedLink.GetOtherEndpoint(this);
                _connectedLink.Transmit(frame, destination);
            }
        }

        base.Update(delta);
    }

    public override void Draw()
    {

    }
}
