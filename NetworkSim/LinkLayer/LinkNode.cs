namespace NetworkSim.LinkLayer;

/// <summary>
/// Represents a node in the data link layer that can connect to other nodes
/// via links.
/// </summary>
public abstract class LinkNode : Entity, IDrawable
{
    public Vector2 Position { get; set; }

    public bool Visible { get; set; } = true;

    /// <summary>
    /// The MAC address of this link node.
    /// </summary>
    public string MacAddress { get; set; } = "00:00:00:00:00:00";

    protected Dictionary<LinkNode, Link> _links = new();

    /// <summary>
    /// A read-only mapping of connected nodes to their links from this node.
    /// </summary>
    public IReadOnlyDictionary<LinkNode, Link> LinkMap => _links;

    /// <summary>
    /// A set of links from this node to connected nodes.
    /// </summary>
    public HashSet<Link> Links => new(_links.Values);

    /// <summary>
    /// A set of nodes connected to this node.
    /// </summary>
    public HashSet<LinkNode> Nodes => new(_links.Keys);

    /// <summary>
    /// Event invoked when a frame is received by this node.
    /// </summary>
    public event Action<Frame>? FrameReceived;

    /// <summary>
    /// If true, this endpoint will receive all frames transmitted on its
    /// links, regardless of the destination MAC address.
    /// </summary>
    public bool IsPromiscuous { get; set; } = false;

    /// <summary>
    /// Creates a bidirectional link between this endpoint and the specified endpoint.
    /// </summary>
    /// <param name="node">The endpoint to link with.</param>
    /// <returns>The created Link object.</returns>
    /// <remarks>
    /// This method is idempotent.
    /// </remarks>
    public virtual Link LinkWith(LinkNode node, Link? existingLink = null)
    {
        if (_links.ContainsKey(node))
        {
            return _links[node];
        }

        var link = existingLink ?? new Link(this, node);
        _links[node] = link;

        node.LinkWith(this, link);

        CurrentWorld?.AddEntity(link);

        return link;
    }

    public virtual bool Unlink(LinkNode node)
    {
        var link = _links.GetValueOrDefault(node);
        if (link is not null && _links.Remove(node))
        {
            node.Unlink(this);
            CurrentWorld?.RemoveEntity(link);
            return true;
        }
        return false;
    }

    public abstract void SendFrame(Frame frame, Link? forwardFromLink = null);

    public abstract void ReceiveFrame(Frame frame, Link fromLink);

    public override void Update(float delta)
    {

    }

    public virtual void Draw()
    {

    }

    protected void InvokeFrameReceived(Frame frame)
    {
        FrameReceived?.Invoke(frame);
    }

    public static string GenerateRandomMacAddress()
    {
        Random rand = new();
        byte[] buffer = new byte[6];
        rand.NextBytes(buffer);

        // set the locally administered address bit
        buffer[0] |= 0x02;
        // clear the multicast bit
        buffer[0] &= 0xFE;

        return string.Join(":", buffer.Select(b => b.ToString("X2")));
    }
}
