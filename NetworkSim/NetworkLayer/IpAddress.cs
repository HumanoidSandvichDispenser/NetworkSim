namespace NetworkSim.NetworkLayer;

/// <summary>
/// Represents an IPv4 address.
/// </summary>
public struct IpAddress
{
    public uint Address { get; set; }

    public override string ToString()
    {
        return string.Join(".",
            (Address >> 24) & 0xFF,
            (Address >> 16) & 0xFF,
            (Address >> 8) & 0xFF,
            Address & 0xFF);
    }

    public IpAddress(uint address)
    {
        Address = address;
    }

    public IpAddress(string address)
    {
        foreach (var octet in address.Split('.').Select(byte.Parse))
        {
            Address = (Address << 8) | octet;
        }
    }

    public static implicit operator IpAddress(string address) => new(address);

    public static implicit operator uint(IpAddress ip) => ip.Address;

    public static implicit operator IpAddress(uint address) => new(address);

    public bool Matches(uint otherIp, uint mask)
    {
        return (Address & mask) == (otherIp & mask);
    }

    public static readonly IpAddress Any = new(0x00000000);

    public static readonly IpAddress Broadcast = new(0xFFFFFFFF);
}
