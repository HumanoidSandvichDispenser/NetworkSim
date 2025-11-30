namespace NetworkSim.NetworkLayer;

/// <summary>
/// A node in a network, such as a host or router. This includes basic
/// functionality common to all network nodes such as sending and receiving
/// datagrams.
/// </summary>
public abstract class NetworkNode : Entity
{
    /// <summary>
    /// Datagrams waiting for ARP resolution.
    /// </summary>
    public Dictionary<uint, Queue<Datagram>> ArpQueue { get; } = new();

    public Dictionary<IpAddress, string> ArpTable { get; } = new();

    /// <summary>
    /// Queue of datagrams to be processed, along with the interface they
    /// arrived on. This is used to simulate processing delay.
    /// </summary>
    public Queue<(Datagram, NetworkInterface)> ProcessingQueue { get; set; } = new();

    /// <summary>
    /// Processing delay in seconds for incoming datagrams.
    /// </summary>
    public float ProcessingDelay { get; set; } = 0;

    /// <summary>
    /// Routes a datagram arriving on a specific interface to the next hop
    /// interface. Returns null (drop) if no route is found.
    /// </summary>
    public abstract IpAddress? Route(Datagram datagram, NetworkInterface? from);

    /// <summary>
    /// Sends a datagram, routing it to the appropriate next hop.
    /// </summary>
    public void SendDatagram(Datagram datagram)
    {
        IpAddress? nextHop = Route(datagram, null);
        if (nextHop.HasValue)
        {
            SendDatagram(datagram, nextHop.Value);
        }
        else
        {
            // drop datagram if no route
        }
    }

    /// <summary>
    /// Sends a datagram to the specified next hop. This is marked abstract
    /// because the implementation depends on how the network node is set up
    /// (e.g., which interfaces it has, and how many).
    /// </summary>
    public abstract void SendDatagram(Datagram datagram, IpAddress nextHop);

    /// <summary>
    /// Sends a datagram on the specified network interface to the specified
    /// next hop.
    /// </summary>
    public void SendDatagram(Datagram datagram, IpAddress nextHop, NetworkInterface to)
    {
        if (to.LinkNode is null)
        {
            // drop datagram if no link node
            return;
        }

        if (datagram.DestinationIp == to.IpAddress)
        {
            // don't send datagram to self
            return;
        }

        Console.WriteLine($"Sending datagram to IP {datagram.DestinationIp} via next hop {nextHop} on interface with MAC {to.LinkNode.MacAddress}");

        var frame = new LinkLayer.Frame();
        frame.SourceMac = to.LinkNode.MacAddress;

        // try to resolve arp
        if (ArpTable.TryGetValue(nextHop, out string? destMac))
        {
            frame.DestinationMac = destMac;
            frame.Datagram = datagram;
        }
        else
        {
            // if no arp entry, send broadcast frame and wait for reply
            frame.DestinationMac = "FF:FF:FF:FF:FF:FF";
            frame.Datagram = new ArpPayload
            {
                SourceIp = to.IpAddress,
                DestinationIp = nextHop,
            };

            // queue the datagram for sending after arp reply
            if (!ArpQueue.ContainsKey(nextHop))
            {
                ArpQueue[nextHop] = new Queue<Datagram>();
            }

            Console.WriteLine($"[{to.LinkNode.MacAddress}] Unable to locate {nextHop}. Sending ARP request first.");

            ArpQueue[nextHop].Enqueue(datagram);
        }

        to.LinkNode?.SendFrame(frame);
    }

    /// <summary>
    /// Handles an incoming ARP payload. Subclasses should listen to
    /// the NetworkInterface.ArpPayloadReceived event and call this method.
    /// </summary>
    protected void OnArpPayloadReceived(ArpPayload arp, LinkLayer.Frame frame, NetworkInterface ni)
    {
        if (arp.Operation == ArpPayload.OperationType.Request)
        {
            // add arp entry
            ArpTable[arp.SourceIp] = frame.SourceMac;

            // if the request wasn't for us, ignore it
            if (arp.DestinationIp != ni.IpAddress)
            {
                return;
            }

            // if we don't have a link node, we can't reply
            // should not happen in practice, as we wouldn't receive the
            // request in the first place
            if (ni.LinkNode is null)
            {
                return;
            }

            var reply = new ArpPayload
            {
                SourceIp = arp.DestinationIp,
                DestinationIp = arp.SourceIp,
                Operation = ArpPayload.OperationType.Reply,
            };

            var replyFrame = new LinkLayer.Frame
            {
                SourceMac = ni.LinkNode.MacAddress,
                DestinationMac = frame.SourceMac,
                Datagram = reply,
            };

            ni.LinkNode.SendFrame(replyFrame);
        }
        else if (arp.Operation == ArpPayload.OperationType.Reply)
        {
            // add arp entry
            ArpTable[arp.SourceIp] = frame.SourceMac;

            // send queued datagrams
            if (ArpQueue.TryGetValue(arp.SourceIp, out Queue<Datagram>? queue))
            {
                while (queue.Count > 0)
                {
                    var datagram = queue.Dequeue();
                    SendDatagram(datagram);
                }

                ArpQueue.Remove(arp.SourceIp);
            }
        }
    }
}
