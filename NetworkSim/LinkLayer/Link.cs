using Raylib_cs;

namespace NetworkSim.LinkLayer;

public class Link : Entity, IDrawable
{
    public Vector2 Position { get; set; }

    public bool Visible { get; set; } = true;

    public LinkNode[] Endpoints { get; set; } = new LinkNode[2];

    /// <summary>
    /// The bandwidth of the link in bits per second.
    /// </summary>
    public int Bandwidth { get; set; } = 1 << 12; // 4 Kbps

    public const int NormalizedBandwidth = 1 << 12;

    /// <summary>
    /// The physical packet currently being transmitted on this link for each endpoint.
    /// </summary>
    public PhysicalPacket?[] CurrentTransmission { get; } = new PhysicalPacket?[2];

    public Link(LinkNode end1, LinkNode end2)
    {
        Endpoints[0] = end1;
        Endpoints[1] = end2;
    }

    private int GetIndexOfEndpoint(LinkNode endpoint)
    {
        if (endpoint == Endpoints[0])
        {
            return 0;
        }
        if (endpoint == Endpoints[1])
        {
            return 1;
        }
        throw new ArgumentException("The provided endpoint is not part of this link.");
    }

    /// <summary>
    /// Gets the packet currently being transmitted to the specified endpoint.
    /// </summary>
    public PhysicalPacket? GetTransmission(LinkNode endpoint)
    {
        return CurrentTransmission[GetIndexOfEndpoint(endpoint)]!;
    }

    public LinkNode GetOtherEndpoint(LinkNode thisEndpoint)
    {
        if (thisEndpoint == Endpoints[0])
        {
            return Endpoints[1];
        }

        if (thisEndpoint == Endpoints[1])
        {
            return Endpoints[0];
        }

        throw new ArgumentException("The provided endpoint is not part of this link.");
    }

    /// <summary>
    /// Physically transmits a frame to the specified destination endpoint.
    /// Ignores any queuing or collision detection.
    /// </summary>
    public void Transmit(Frame frame, LinkNode destination)
    {
        int index = GetIndexOfEndpoint(destination);
        float transmissionTime = (float)frame.Size / (Bandwidth * 8);

        PhysicalPacket packet = new PhysicalPacket();
        packet.Frame = frame;
        packet.CurrentLink = this;
        packet.TransmissionTime = transmissionTime;
        packet.TimeLeft = transmissionTime;
        packet.TransmissionComplete += TransmissionComplete;
        CurrentTransmission[index] = packet;

        CurrentWorld?.AddEntity(packet);
    }

    private void TransmissionComplete(PhysicalPacket packet)
    {
        packet.TransmissionComplete -= TransmissionComplete;
        packet.CurrentLink = null;

        int index = Array.IndexOf(CurrentTransmission, packet);
        if (index >= 0)
        {
            CurrentTransmission[index] = null;
            var destinationEndpoint = Endpoints[index];
            destinationEndpoint.ReceiveFrame(packet.Frame, this);
        }

        // remove the packet from the world
        CurrentWorld?.RemoveEntity(packet);
    }

    public override void Update(float delta)
    {

    }

    public int GetTransmissionIndex(PhysicalPacket packet)
    {
        return Array.IndexOf(CurrentTransmission, packet);
    }

    /// <summary>
    /// Drops all ongoing transmissions on this link.
    /// </summary>
    public void DropTransmissions()
    {
        for (int i = 0; i < CurrentTransmission.Length; i++)
        {
            var packet = CurrentTransmission[i];

            if (packet is not null)
            {
                packet.TransmissionComplete -= TransmissionComplete;
                packet.CurrentLink = null;
                CurrentWorld?.RemoveEntity(packet);
                CurrentTransmission[i] = null;
            }
        }
    }

    public void Draw()
    {
        var start = Endpoints[0].Position;
        var end = Endpoints[1].Position;

        Color lineColor = Color.LightGray;

        if (CurrentTransmission[0] is not null || CurrentTransmission[1] is not null)
        {
            lineColor = Color.Black;
        }

        float thickness = Bandwidth / NormalizedBandwidth * 4;
        Raylib.DrawLineEx(start, end, thickness, lineColor);
    }
}
