using NetworkSim.LinkLayer;

namespace NetworkSim.Tests;

public class SwitchTests
{
    [Fact]
    public void Constructor_InitializesWithNoLinks()
    {
        var endpoint = new Switch();
        endpoint.Links.ShouldBeEmpty();
    }

    [Fact]
    public void LinkWith_CreatesBidirectionalLink()
    {
        var endpoint1 = new Switch();
        var endpoint2 = new Switch();

        endpoint1.LinkWith(endpoint2);

        endpoint1.Nodes.ShouldContain(endpoint2);
        endpoint2.Nodes.ShouldContain(endpoint1);
    }

    [Fact]
    public void LinkWith_IsIdempotent()
    {
        var endpoint1 = new Switch();
        var endpoint2 = new Switch();

        var link1 = endpoint1.LinkWith(endpoint2);
        var link2 = endpoint1.LinkWith(endpoint2);

        link1.ShouldBe(link2);
        endpoint1.Links.Count.ShouldBe(1);
        endpoint2.Links.First().ShouldBe(link1);
    }

    [Fact]
    public void Unlink_RemovesLinkBetweenEndpoints()
    {
        var endpoint1 = new Switch();
        var endpoint2 = new Switch();

        endpoint1.LinkWith(endpoint2);

        var result = endpoint1.Unlink(endpoint2);

        result.ShouldBeTrue();
        endpoint1.Nodes.ShouldNotContain(endpoint2);
        endpoint2.Nodes.ShouldNotContain(endpoint1);
    }

    [Fact]
    public void Unlink_NonExistentLink_ReturnsFalse()
    {
        var endpoint1 = new Switch();
        var endpoint2 = new Switch();

        var result = endpoint1.Unlink(endpoint2);

        result.ShouldBeFalse();
    }
}
