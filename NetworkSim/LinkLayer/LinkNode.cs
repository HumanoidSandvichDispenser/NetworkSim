namespace NetworkSim.LinkLayer;

public abstract class LinkNode : Entity, IDrawable
{
    public Vector2 Position { get; set; }

    public string MacAddress { get; set; } = "00:00:00:00:00:00";

    protected Dictionary<LinkNode, Link> _links = new();

    public HashSet<Link> Links => new(_links.Values);

    public HashSet<LinkNode> Nodes => new(_links.Keys);

    public bool Visible { get; set; } = true;

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
    public Link LinkWith(LinkNode node)
    {
        if (_links.ContainsKey(node))
        {
            return _links[node];
        }

        var link = new Link(this, node);
        _links[node] = link;
        node._links[this] = link;

        CurrentWorld?.AddEntity(link);

        return link;
    }

    public bool Unlink(LinkNode node)
    {
        var link = _links.GetValueOrDefault(node);
        if (link is not null && _links.Remove(node))
        {
            node._links.Remove(this);
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
}
