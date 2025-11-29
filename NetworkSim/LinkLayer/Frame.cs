using System.Numerics;
using Raylib_cs;

namespace NetworkSim.LinkLayer;

/// <summary>
/// Represents a data link layer frame similar to an Ethernet frame. For
/// simplicity, the project does not simulate the physical layer, so this class
/// also represents the physical layer transmission.
/// </summary>
public class Frame : IPacket
{
    public NetworkLayer.Datagram? Datagram { get; set; }

    public string SourceMac { get; set; } = "00:00:00:00:00:00";

    public string DestinationMac { get; set; } = "00:00:00:00:00:00";

    public const uint HeaderSize = 18;

    public uint Size => HeaderSize + (Datagram?.Size ?? 0);

    public FrameType Type { get; set; } = FrameType.Ip;

    public bool IsBroadcast()
    {
        return DestinationMac.ToUpper() == "FF:FF:FF:FF:FF:FF";
    }
}

public enum FrameType
{
    Ip,
    Arp
}

