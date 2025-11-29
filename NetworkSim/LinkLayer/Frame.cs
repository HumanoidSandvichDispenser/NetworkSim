using System.Numerics;
using Raylib_cs;

namespace NetworkSim.LinkLayer;

/// <summary>
/// Represents a data link layer frame similar to an Ethernet frame. For
/// simplicity, the project does not simulate the physical layer, so this class
/// also represents the physical layer transmission.
/// </summary>
public class Frame : Packet, IDrawable
{
    public Vector2 Position { get; set; }

    public bool Visible { get; set; } = true;

    public NetworkLayer.Datagram? Datagram { get; set; }

    public string SourceMac { get; set; } = "00:00:00:00:00:00";

    public string DestinationMac { get; set; } = "00:00:00:00:00:00";

    private const uint HeaderSize = 18;

    public override uint Size => HeaderSize + (Datagram?.Size ?? 0);

    private Link? _currentLink;

    public Link? CurrentLink
    {
        get => _currentLink;
        set
        {
            _currentLink = value;
            Visible = value is not null;
        }
    }

    public float TransmissionTime { get; set; }

    public float TimeLeft { get; set; }

    public event Action<Frame>? TransmissionComplete;

    public override void Update(float delta)
    {
        if (CurrentLink is null)
        {
            return;
        }

        TimeLeft -= delta;

        float t = 1 - (TimeLeft / TransmissionTime);

        int index = CurrentLink.GetTransmissionIndex(this);

        LinkNode fromEndpoint = CurrentLink.Endpoints[1 - index];
        LinkNode toEndpoint = CurrentLink.Endpoints[index];
        Position = Vector2.Lerp(fromEndpoint.Position, toEndpoint.Position, t);

        if (TimeLeft <= 0)
        {
            TransmissionComplete?.Invoke(this);
            CurrentLink = null;
        }
    }

    public bool IsBroadcast()
    {
        return DestinationMac.ToUpper() == "FF:FF:FF:FF:FF:FF";
    }

    public void Draw()
    {
        if (CurrentLink is null)
        {
            return;
        }

        Raylib.DrawPoly(Position, 4, 8, 0, Color.DarkPurple);

        Vector2 textPos = Position + new Vector2(0, -16);
        string sizeInfo = $"{Size}B";
        Raylib.DrawText(sizeInfo, (int)textPos.X, (int)textPos.Y, 20, Color.Black);
    }

    public override object Clone()
    {
        var clone = (Frame)MemberwiseClone();
        var clonedDatagram = Datagram?.CloneEntity() as NetworkLayer.Datagram;
        clone.Datagram = clonedDatagram;
            
        return clone;
    }
}
