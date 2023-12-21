using PKOC.Net.MessageData;

namespace PKOC.Net.Tests.MessageData;

[TestFixture]
public class CardPresentDataTest
{
    [Test]
    public void ParseDataTest()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E0 FC18 FB02 5C020100 4C10000102030405060708090A0B0C0D0E0F".Replace(" ", string.Empty));

        // Act
        var actual = CardPresentData.ParseData(messageData);

        // Assert
        Assert.That(actual.ProtocolVersions, Is.EqualTo(new byte[] { 0x01, 0x00 }));
        Assert.That(actual.Error, Is.EqualTo(new byte[] { 0x02 }));
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
        var actual = CardPresentData.ParseData(messageData);

        // Assert
        Assert.That(actual.ProtocolVersions, Is.EqualTo(new byte[] { 0x01, 0x00 }));
        Assert.That(actual.Error, Is.EqualTo(new byte[] { 0x01, 0x90, 0x00 }));
        Assert.That(actual.TransactionIdentifier,
            Is.EqualTo(Enumerable.Range(0x00, 16).Select(i => (byte)i).ToArray()));
    }

    [Test]
    public void ParseDataWrongMessageIdentifier()
    {
        // Arrange
        var messageData = Convert.FromHexString("E0 FF04 5C020100".Replace(" ", string.Empty));

        // Act/Assert
        Assert.Catch(() => { CardPresentData.ParseData(messageData); });
    }
    
    [Test]
    public void ParseDataMissingTLVLength()
    {
        // Arrange
        var messageData = Convert.FromHexString("E0 FC".Replace(" ", string.Empty));

        // Act/Assert
        Assert.Catch(() => { CardPresentData.ParseData(messageData); });
    }

    [Test]
    public void BuildDataTest()
    {
        // Arrange
        var cardPresentData = new CardPresentData(new byte[] { 0x00, 0x01 }, new byte[] { 0x02 },
            Enumerable.Range(0x00, 16).Select(i => (byte)i).ToArray());

        // Act
        var actual = cardPresentData.BuildData();

        Assert.That(actual.ToArray(), Is.EqualTo(Convert.FromHexString(
            "E0 FC18 5C020100 FB02 4C10000102030405060708090A0B0C0D0E0F".Replace(" ", string.Empty))));
    }
}