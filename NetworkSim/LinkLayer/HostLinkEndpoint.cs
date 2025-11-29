namespace NetworkSim.LinkLayer;

/// <summary>
/// A link endpoint used by hosts.
/// </summary>
public class HostLinkEndpoint : LinkEndpoint
{
    public override void Initialize()
    {
        Visible = false;
    }

    public override void SendFrame(Frame frame, Link? forwardFromLink = null)
    {

    }

    public override void ReceiveFrame(Frame frame, Link fromLink)
    {
        // hosts process frames at the network layer
    }
}
