namespace NetworkSim.NetworkLayer;

public class Datagram : IPacket
{
    public uint SourceIp { get; set; }

    public uint DestinationIp { get; set; }

    public TransportLayer.Segment? Segment { get; set; }

    public const uint HeaderSize = 20;

    public virtual uint Size => HeaderSize;
}
