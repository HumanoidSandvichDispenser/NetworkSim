namespace NetworkSim.Tests;

public class IpAddressTests
{
    [Theory]
    [InlineData("0.0.0.0", 0x00000000)]
    [InlineData("255.255.255.255", 0xFFFFFFFF)]
    [InlineData("192.168.1.1", 0xC0A80101)]
    public void IpAddress_ShouldParseCorrectly(string ipString, uint expectedValue)
    {
        var ipAddress = new NetworkLayer.IpAddress(ipString);
        ipAddress.Address.ShouldBe(expectedValue);
    }

    [Theory]
    [InlineData(0xFFFFFFFF, "255.255.255.255")]
    [InlineData(0x08080808, "8.8.8.8")]
    public void IpAddress_ShouldReturnCorrectString(uint ipValue, string expectedString)
    {
        var ipAddress = new NetworkLayer.IpAddress(ipValue);
        ipAddress.ToString().ShouldBe(expectedString);
    }

    [Theory]
    [InlineData("192.168.1.134", "255.255.255.0", "192.168.1.0")]
    [InlineData("172.16.5.31", "255.255.0.0", "172.16.0.0")]
    [InlineData("192.168.1.1", "0.0.0.0", "0.0.0.0")]
    public void IpAddress_ShouldMask(string ipString, string maskString, string expected)
    {
        var ipAddress = new NetworkLayer.IpAddress(ipString);
        var mask = new NetworkLayer.IpAddress(maskString);
        var expectedNetwork = new NetworkLayer.IpAddress(expected);
        (ipAddress.Address & mask.Address).ShouldBe(expectedNetwork.Address);
    }

    [Theory]
    [InlineData("192.168.1.134", "255.255.255.0", "192.168.1.0")]
    [InlineData("172.16.5.31", "255.255.0.0", "172.16.0.0")]
    [InlineData("172.16.5.31", "0.0.0.0", "0.0.0.0")]
    public void MaskedIp_ShouldMatch(string ipString, string maskString, string networkString)
    {
        var ipAddress = new NetworkLayer.IpAddress(ipString);
        var mask = new NetworkLayer.IpAddress(maskString);
        var network = new NetworkLayer.IpAddress(networkString);
        ipAddress.Matches(network.Address, mask.Address).ShouldBeTrue();
    }
}
