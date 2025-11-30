namespace NetworkSim.NetworkLayer;

public class Datagram : IPacket
{
    public IpAddress SourceIp { get; set; }

    public IpAddress DestinationIp { get; set; }

    public TransportLayer.Segment? Segment { get; set; }

    public const uint HeaderSize = 20;

    public virtual uint Size => HeaderSize;
}
