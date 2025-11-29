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

    /// <summary>
    /// The frame currently being transmitted on this link for each endpoint.
    /// </summary>
    public Frame?[] CurrentTransmission { get; } = new Frame?[2];

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
    /// Gets the frame currently being transmitted to the specified endpoint.
    /// </summary>
    public Frame? GetTransmission(LinkNode endpoint)
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

    public void Transmit(Frame frame, LinkNode destination)
    {
        int index = GetIndexOfEndpoint(destination);
        float transmissionTime = (float)frame.Size / (Bandwidth * 8);
        CurrentTransmission[index] = frame;
        frame.CurrentLink = this;
        frame.TransmissionTime = transmissionTime;
        frame.TimeLeft = transmissionTime;
        frame.TransmissionComplete += TransmissionComplete;
        CurrentWorld?.AddEntity(frame);
    }

    private void TransmissionComplete(Frame frame)
    {
        frame.TransmissionComplete -= TransmissionComplete;
        frame.CurrentLink = null;

        int index = Array.IndexOf(CurrentTransmission, frame);
        if (index >= 0)
        {
            CurrentTransmission[index] = null;
            var destinationEndpoint = Endpoints[index];
            destinationEndpoint.ReceiveFrame(frame, this);
        }
    }

    public override void Update(float delta)
    {

    }

    public int GetTransmissionIndex(Frame frame)
    {
        return Array.IndexOf(CurrentTransmission, frame);
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

        Raylib.DrawLineEx(start, end, 2, lineColor);
    }
}
