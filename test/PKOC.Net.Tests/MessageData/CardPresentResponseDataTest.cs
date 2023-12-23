using PKOC.Net.MessageData;

namespace PKOC.Net.Tests.MessageData;

[TestFixture]
[TestOf(typeof(CardPresentResponseData))]
public class CardPresentResponseDataTest
{
    [Test]
    public void ParseDataTest()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E0 FC18 FB02 5C020100 4C10000102030405060708090A0B0C0D0E0F".Replace(" ", string.Empty));

        // Act
        var actual = CardPresentResponseData.ParseData(messageData);

        // Assert
        Assert.That(actual.ProtocolVersions, Is.EqualTo(new byte[] { 0x01, 0x00 }));
        Assert.That(actual.Error, Is.EqualTo(new [] { ErrorCode.TimeoutAccessingCard }));
        Assert.That(actual.TransactionIdentifier,
            Is.EqualTo(Enumerable.Range(0x00, 16).Select(i => (byte)i).ToArray()));
    }
    
    [Test]
    public void ParseDataISOStatus()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E0 FC1A FB019000 5C020100 4C10000102030405060708090A0B0C0D0E0F".Replace(" ", string.Empty));

        // Act
        var actual = CardPresentResponseData.ParseData(messageData);

        // Assert
        Assert.That(actual.ProtocolVersions, Is.EqualTo(new byte[] { 0x01, 0x00 }));
        Assert.That(actual.Error, Is.EqualTo(new byte[] { ErrorCode.ISO7816Status, 0x90, 0x00 }));
        Assert.That(actual.TransactionIdentifier,
            Is.EqualTo(Enumerable.Range(0x00, 16).Select(i => (byte)i).ToArray()));
    }

    [Test]
    public void ParseDataWrongMessageIdentifier()
    {
        // Arrange
        var messageData = Convert.FromHexString("E0 FF04 5C020100".Replace(" ", string.Empty));

        // Act/Assert
        Assert.Catch(() => { CardPresentResponseData.ParseData(messageData); });
    }
    
    [Test]
    public void ParseDataMissingTLVLength()
    {
        // Arrange
        var messageData = Convert.FromHexString("E0 FC".Replace(" ", string.Empty));

        // Act/Assert
        Assert.Catch(() => { CardPresentResponseData.ParseData(messageData); });
    }

    [Test]
    public void BuildDataTest()
    {
        // Arrange
        var cardPresentData = new CardPresentResponseData(new byte[] { 0x00, 0x01 },
            new [] { ErrorCode.TimeoutAccessingCard },
            Enumerable.Range(0x00, 16).Select(i => (byte)i).ToArray());

        // Act
        var actual = cardPresentData.BuildData();

        Assert.That(actual.ToArray(), Is.EqualTo(Convert.FromHexString(
            "E0 FC18 5C020100 FB02 4C10000102030405060708090A0B0C0D0E0F".Replace(" ", string.Empty))));
    }
}