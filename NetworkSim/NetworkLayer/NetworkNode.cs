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

    public Dictionary<uint, string> ArpTable { get; } = new();

    public void SendDatagram(Datagram datagram, NetworkInterface to)
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

        Console.WriteLine($"Sending datagram to IP {datagram.DestinationIp:X8}");

        var frame = new LinkLayer.Frame();
        frame.SourceMac = to.LinkNode.MacAddress;

        if (ArpTable.TryGetValue(datagram.DestinationIp, out string? destMac))
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
                SourceIp = datagram.SourceIp,
                DestinationIp = datagram.DestinationIp,
            };

            // queue the datagram for sending after arp reply
            if (!ArpQueue.ContainsKey(datagram.DestinationIp))
            {
                ArpQueue[datagram.DestinationIp] = new Queue<Datagram>();
            }

            Console.WriteLine($"Queuing datagram for ARP resolution of IP {datagram.DestinationIp:X8}");

            ArpQueue[datagram.DestinationIp].Enqueue(datagram);
        }

        to.LinkNode?.SendFrame(frame);
    }

    /// <summary>
    /// Handles an incoming ARP payload. Subclasses should listen to
    /// the NetworkInterface.ArpPayloadReceived event and call this method.
    /// </summary>
    public void OnArpPayloadReceived(ArpPayload arp, LinkLayer.Frame frame, NetworkInterface ni)
    {
        if (arp.Operation == ArpPayload.OperationType.Request)
        {
            Console.WriteLine($"ARP request received for IP {arp.DestinationIp:X8}");

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
            Console.WriteLine($"ARP reply received for IP {arp.DestinationIp:X8}");

            // add arp entry
            ArpTable[arp.SourceIp] = frame.SourceMac;

            // send queued datagrams
            if (ArpQueue.TryGetValue(arp.SourceIp, out Queue<Datagram>? queue))
            {
                while (queue.Count > 0)
                {
                    var datagram = queue.Dequeue();
                    SendDatagram(datagram, ni);
                }

                ArpQueue.Remove(arp.SourceIp);
            }
        }
    }
}
