using PKOC.Net.MessageData;

namespace PKOC.Net.Tests.MessageData;

[TestFixture]
[TestOf(typeof(ReaderErrorResponseData))]
public class ReaderErrorResponseDataTest
{
    [Test]
    public void ParseDataTest()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "FE 02".Replace(" ", string.Empty));

        // Act
        var actual = ReaderErrorResponseData.ParseData(messageData);

        // Assert
        Assert.That(actual.Error, Is.EqualTo(new [] { ErrorCode.TimeoutAccessingCard }));
    }
    
    [Test]
    public void ParseDataISOStatus()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "FE 019000".Replace(" ", string.Empty));

        // Act
        var actual = ReaderErrorResponseData.ParseData(messageData);

        // Assert
        Assert.That(actual.Error, Is.EqualTo(new byte[] { ErrorCode.ISO7816Status, 0x90, 0x00 }));
    }

    [Test]
    public void BuildDataTest()
    {
        // Arrange
        var readerErrorResponseData = new ReaderErrorResponseData(new[] { ErrorCode.TimeoutAccessingCard });

        // Act
        var actual = readerErrorResponseData.BuildData();

        Assert.That(actual.ToArray(), Is.EqualTo(Convert.FromHexString("FE 02".Replace(" ", string.Empty))));
    }
    
    [Test]
    public void BuildDataISOStatus()
    {
        // Arrange
        var readerErrorResponseData = new ReaderErrorResponseData(new byte[] { ErrorCode.ISO7816Status,  0x90, 0x00 });

        // Act
        var actual = readerErrorResponseData.BuildData();

        Assert.That(actual.ToArray(), Is.EqualTo(Convert.FromHexString("FE 019000".Replace(" ", string.Empty))));
    }
}