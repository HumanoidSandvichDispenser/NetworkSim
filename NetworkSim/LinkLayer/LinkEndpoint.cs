namespace NetworkSim.LinkLayer;

public abstract class LinkEndpoint : Entity, IDrawable
{
    public Vector2 Position { get; set; }

    public string MacAddress { get; set; } = "00:00:00:00:00:00";

    protected Dictionary<LinkEndpoint, Link> _endpoints = new();

    public HashSet<Link> Links => new(_endpoints.Values);

    public HashSet<LinkEndpoint> Endpoints => new(_endpoints.Keys);

    public bool Visible { get; set; } = true;

    /// <summary>
    /// If true, this endpoint will receive all frames transmitted on its
    /// links, regardless of the destination MAC address.
    /// </summary>
    public bool IsPromiscuous { get; set; } = false;

    /// <summary>
    /// Creates a bidirectional link between this endpoint and the specified endpoint.
    /// </summary>
    /// <param name="endpoint">The endpoint to link with.</param>
    /// <returns>The created Link object.</returns>
    /// <remarks>
    /// This method is idempotent.
    /// </remarks>
    public Link LinkWith(LinkEndpoint endpoint)
    {
        if (_endpoints.ContainsKey(endpoint))
        {
            return _endpoints[endpoint];
        }

        var link = new Link(this, endpoint);
        _endpoints[endpoint] = link;
        endpoint._endpoints[this] = link;

        CurrentWorld?.AddEntity(link);

        return link;
    }

    public bool Unlink(LinkEndpoint endpoint)
    {
        var link = _endpoints.GetValueOrDefault(endpoint);
        if (link is not null && _endpoints.Remove(endpoint))
        {
            endpoint._endpoints.Remove(this);
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
