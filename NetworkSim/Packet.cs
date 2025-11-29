namespace NetworkSim;

public abstract class Packet : Entity
{
    public abstract uint Size { get; }
}
