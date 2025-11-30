using System.Numerics;
using Raylib_cs;

namespace NetworkSim;

/// <summary>
/// Represents a physical packet.
/// </summary>
public class PhysicalPacket : Entity, IPacket, IDrawable
{
    public Vector2 Position { get; private set; }

    public bool Visible { get; set; } = true;

    public LinkLayer.Frame Frame { get; set; } = null!;

    public uint Size => Frame.Size;

    public float TransmissionTime { get; set; }

    public float TimeLeft { get; set; }

    public event Action<PhysicalPacket>? TransmissionComplete;

    private LinkLayer.Link? _currentLink;

    public LinkLayer.Link? CurrentLink
    {
        get => _currentLink;
        set
        {
            _currentLink = value;
            Visible = value is not null;
        }
    }

    public override void Update(float delta)
    {
        if (CurrentLink is null)
        {
            return;
        }

        TimeLeft -= delta;

        float t = 1 - (TimeLeft / TransmissionTime);

        int index = CurrentLink.GetTransmissionIndex(this);
        LinkLayer.LinkNode fromEndpoint = CurrentLink.Endpoints[1 - index];
        LinkLayer.LinkNode toEndpoint = CurrentLink.Endpoints[index];

        Position = Vector2.Lerp(fromEndpoint.Position, toEndpoint.Position, t);

        if (TimeLeft <= 0)
        {
            TransmissionComplete?.Invoke(this);
            CurrentLink = null;
        }
    }

    public void Draw()
    {
        if (CurrentLink is null)
        {
            return;
        }

        Raylib.DrawPoly(Position, 4, 12, 0, Color.Black);

        if (Frame is not null)
        {
            Raylib.DrawPoly(Position, 4, 8, 0, Color.Purple);
            if (Frame.Datagram is not null)
            {
                Raylib.DrawPoly(Position, 4, 4, 0, Color.Magenta);
            }
        }

        Vector2 textPos = Position + new Vector2(0, -24);
        string sizeInfo = $"{Size} bytes";
        Raylib.DrawText(sizeInfo, (int)textPos.X, (int)textPos.Y, 24, Color.Black);
    }
}
