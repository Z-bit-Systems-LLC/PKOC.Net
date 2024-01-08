using PKOC.Net.MessageData;

namespace PKOC.Net.Tests.MessageData;

[TestFixture]
[TestOf(typeof(AuthenticationRequestData))]
public class AuthenticationRequestDataTest
{
    [Test]
    public void ParseDataTest()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E1 0001 5C020100 4D20000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F 4C10000102030405060708090A0B0C0D0E0F FD03".RemoveWhiteSpaceFromHexadecimalString());

        // Act
        var actual = AuthenticationRequestData.ParseData(messageData);

        // Assert
        Assert.That(actual.ProtocolVersion, Is.EqualTo(new byte[] {0x01, 0x00}));
        Assert.That(actual.ReaderIdentifier, Is.EqualTo(Enumerable.Range(0x00, 32).Select(i => (byte)i)));
        Assert.That(actual.TransactionIdentifier, Is.EqualTo(Enumerable.Range(0x00, 16).Select(i => (byte)i)));
        Assert.That(actual.TransactionSequence, Is.EqualTo(0x03));
    }
    
    [Test]
    public void ParseDataMissingReaderIdentifierTest()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E1 0001 5C020100 4C10000102030405060708090A0B0C0D0E0F".RemoveWhiteSpaceFromHexadecimalString());

        // Act/Assert
        Assert.Catch(() => { AuthenticationRequestData.ParseData(messageData); });
    }

    [Test]
    public void BuildData()
    {
        // Arrange
        var authenticationRequestData =
            new AuthenticationRequestData([0x01, 0x00], Enumerable.Range(0x00, 32).Select(i => (byte)i).ToArray(),
                Enumerable.Range(0x00, 16).Select(i => (byte)i).ToArray(), 0x03);

        // Act
        var actual = authenticationRequestData.BuildData();

        Assert.That(actual.ToArray(), Is.EqualTo(Convert.FromHexString(
            "E1 0001 5C020100 4D20000102030405060708090A0B0C0D0E0F101112131415161718191A1B1C1D1E1F 4C10000102030405060708090A0B0C0D0E0F FD03".RemoveWhiteSpaceFromHexadecimalString())));
    }
}