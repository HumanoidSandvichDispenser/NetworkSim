namespace NetworkSim.NetworkLayer;

public sealed class OspfHelloDatagram : Datagram
{
    public override uint Size => 48;
}
