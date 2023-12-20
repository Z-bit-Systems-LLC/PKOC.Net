using PKOC.Net.MessageData;

namespace PKOC.Net.Tests.MessageData;

[TestFixture]
public class CardPresentDataTest
{
    [Test]
    public void ParseDataTest()
    {
        // Arrange
        var messageData = Convert.FromHexString("E0 FC04 5C020100".Replace(" ", string.Empty));

        // Act
        var actual = CardPresentData.ParseData(messageData);

        // Assert
        Assert.That(actual.ProtocolVersions, Is.EqualTo(new byte[]{0x01, 0x00}));
        Assert.That(actual.ErrorCode, Is.EqualTo(0x00));
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
}