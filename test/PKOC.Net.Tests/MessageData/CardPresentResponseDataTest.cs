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
                "E0 FC08 FB02 5C020100 FD03".RemoveWhiteSpaceFromHexadecimalString());

        // Act
        var actual = CardPresentResponseData.ParseData(messageData);

        // Assert
        Assert.That(actual.ProtocolVersions, Is.EqualTo(new byte[] { 0x01, 0x00 }));
        Assert.That(actual.Error, Is.EqualTo(new [] { ErrorCode.TimeoutAccessingCard }));
        Assert.That(actual.TransactionSequence, Is.EqualTo(0x03));
    }

    [Test]
    public void ParseDataISOStatus()
    {
        // Arrange
        var messageData =
            Convert.FromHexString("E0 FC0A FB019000 5C020100 FD03".RemoveWhiteSpaceFromHexadecimalString());

        // Act
        var actual = CardPresentResponseData.ParseData(messageData);

        // Assert
        Assert.That(actual.ProtocolVersions, Is.EqualTo(new byte[] { 0x01, 0x00 }));
        Assert.That(actual.Error, Is.EqualTo(new byte[] { ErrorCode.ISO7816Status, 0x90, 0x00 }));
        Assert.That(actual.TransactionSequence, Is.EqualTo(0x03));
    }

    [Test]
    public void ParseDataWrongMessageIdentifier()
    {
        // Arrange
        var messageData = Convert.FromHexString("E0 FF04 5C020100".RemoveWhiteSpaceFromHexadecimalString());

        // Act/Assert
        Assert.Catch(() => { CardPresentResponseData.ParseData(messageData); });
    }
    
    [Test]
    public void ParseDataMissingTLVLength()
    {
        // Arrange
        var messageData = Convert.FromHexString("E0 FC".RemoveWhiteSpaceFromHexadecimalString());

        // Act/Assert
        Assert.Catch(() => { CardPresentResponseData.ParseData(messageData); });
    }

    [Test]
    public void BuildDataTest()
    {
        // Arrange
        var cardPresentData = new CardPresentResponseData([0x00, 0x01], [ErrorCode.TimeoutAccessingCard], 0x03);

        // Act
        var actual = cardPresentData.BuildData();

        Assert.That(actual.ToArray(), Is.EqualTo(Convert.FromHexString(
            "E0 FC08 5C020100 FB02 FD03".RemoveWhiteSpaceFromHexadecimalString())));
    }
}