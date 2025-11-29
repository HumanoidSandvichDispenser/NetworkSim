using NSubstitute;
using NetworkSim.LinkLayer;

namespace NetworkSim.Tests.IntegrationTests;

public class LinkLayerTests
{
    [Fact]
    public void Linking_ShouldCreateLinkEntity()
    {
        var world = new World();
        var endpoint1 = new Switch();
        var endpoint2 = new Switch();

        world.AddEntity(endpoint1);
        world.AddEntity(endpoint2);

        var link = endpoint1.LinkWith(endpoint2);
        world.Entities.ShouldContain(link);
    }

    [Fact]
    public void Unlinking_ShouldRemoveLinkEntity()
    {
        var world = new World();
        var endpoint1 = new Switch();
        var endpoint2 = new Switch();

        world.AddEntity(endpoint1);
        world.AddEntity(endpoint2);

        var link = endpoint1.LinkWith(endpoint2);
        endpoint1.Unlink(endpoint2);

        world.Entities.ShouldNotContain(link);
    }

    [Fact]
    public void SendingFrame_ShouldEnqueueFrameOnLink()
    {
        var world = new World();
        var endpoint1 = new Switch("a");
        var endpoint2 = new Switch("b");

        world.AddEntity(endpoint1);
        world.AddEntity(endpoint2);

        var link = endpoint1.LinkWith(endpoint2);
        var frame = new Frame
        {
            SourceMac = "00:00:00:00:00:01",
            DestinationMac = "00:00:00:00:00:02",
        };

        world.AddEntity(frame);

        endpoint1.SendFrame(frame);

        endpoint1.TxQueue.ShouldNotBeEmpty();
        endpoint1.TxQueue[link].ShouldNotBeNull();

        var dequeuedFrame = endpoint1.TxQueue[link].Dequeue();
        dequeuedFrame.SourceMac.ShouldBe(frame.SourceMac);
        dequeuedFrame.CurrentLink.ShouldBeNull();
    }

    [Fact]
    public void SendingFrameAndUpdate_ShouldMoveFrame()
    {
        var world = new World();
        var endpoint1 = new Switch("a");
        var endpoint2 = Substitute.ForPartsOf<Switch>("b");

        world.AddEntity(endpoint1);
        world.AddEntity(endpoint2);

        var link = endpoint1.LinkWith(endpoint2);

        var frame = new Frame
        {
            SourceMac = "a",
            DestinationMac = "b",
        };
        world.AddEntity(frame);

        Frame? receivedFrame = null;
        endpoint2.FrameReceived += f => receivedFrame = f;

        endpoint1.SendFrame(frame);
        endpoint1.TxQueue[link].Count.ShouldBe(1);

        world.Update(1, 2);
        receivedFrame.ShouldNotBeNull();
    }
}
