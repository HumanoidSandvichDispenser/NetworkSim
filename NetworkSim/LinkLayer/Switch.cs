using Raylib_cs;

namespace NetworkSim.LinkLayer;

public class Switch : LinkNode
{
    private readonly Dictionary<string, LinkNode> _macTable = new();

    public IReadOnlyDictionary<string, LinkNode> MacTable => _macTable;

    private readonly Dictionary<Link, FrameQueue> _txQueue = new();

    public IReadOnlyDictionary<Link, FrameQueue> TxQueue => _txQueue;

    public uint QueueSize { get; set; } = 4096;

    public Switch(string macAddress = "00:00:00:00:00:00")
    {
        MacAddress = macAddress;
    }

    public override void SendFrame(Frame frame, Link? forwardFromLink = null)
    {
        if (frame.CurrentWorld != CurrentWorld)
        {
            frame.CurrentWorld?.RemoveEntity(frame);
            CurrentWorld?.AddEntity(frame);
        }

        if (_macTable.ContainsKey(frame.DestinationMac))
        {
            // known destination, forward to that link
            var endpoint = _macTable[frame.DestinationMac];
            var link = _links[endpoint];

            if (!_txQueue.ContainsKey(link))
            {
                _txQueue[link] = new FrameQueue(QueueSize);
            }

            _txQueue[link].TryEnqueue(frame);
        }
        else
        {
            // unknown destination, flood to all links except the source

            // create a copy of the frame for each link
            // and remove the original frame
            foreach ((var endpoint, var link) in _links)
            {
                if (endpoint.MacAddress != frame.SourceMac)
                {
                    if (!_txQueue.ContainsKey(link))
                    {
                        _txQueue[link] = new FrameQueue(QueueSize);
                    }

                    if (link == forwardFromLink)
                    {
                        continue;
                    }

                    var clone = (Frame)frame.Clone();
                    CurrentWorld?.AddEntity(clone);

                    _txQueue[link].TryEnqueue(clone);
                }
            }

            CurrentWorld?.RemoveEntity(frame);
        }
    }

    public override void ReceiveFrame(Frame frame, Link fromLink)
    {
        // learn the source MAC address
        _macTable[frame.SourceMac] = fromLink.GetOtherEndpoint(this);

        if (frame.IsBroadcast() || frame.DestinationMac != MacAddress)
        {
            SendFrame(frame, fromLink);
        }

        if (frame.DestinationMac == MacAddress)
        {
            // deliver to upper layer through event
            InvokeFrameReceived(frame);

            // typically, the upper layers will just another frame, so we
            // can free the current one
            CurrentWorld?.RemoveEntity(frame);
        }
    }

    public override void Update(float delta)
    {
        // each frame must wait for its turn to be transmitted on the link
        foreach (var (link, queue) in _txQueue)
        {
            var endpoint = link.GetOtherEndpoint(this);
            if (link.GetTransmission(endpoint) is null)
            {
                if (queue.Count > 0)
                {
                    var frame = queue.Dequeue();
                    link.Transmit(frame, endpoint);
                }
            }
        }
        base.Update(delta);
    }

    public override void Draw()
    {
        Raylib.DrawCircleV(Position, 8, Color.Blue);
    }
}
