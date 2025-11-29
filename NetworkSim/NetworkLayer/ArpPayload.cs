namespace NetworkSim.NetworkLayer;

public class ArpPayload : Datagram
{
    public const uint ArpPayloadSize = 28;

    public enum OperationType : ushort
    {
        Request = 1,
        Reply = 2
    }

    public OperationType Operation { get; set; } = OperationType.Request;

    public override uint Size => ArpPayloadSize;
}
