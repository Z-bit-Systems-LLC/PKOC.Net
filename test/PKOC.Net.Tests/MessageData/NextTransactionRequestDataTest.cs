using PKOC.Net.MessageData;

namespace PKOC.Net.Tests.MessageData;

[TestFixture]
[TestOf(typeof(NextTransactionRequestData))]
public class NextTransactionRequestDataTest
{
    [Test]
    public void ParseDataTest()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E3 4C10000102030405060708090A0B0C0D0E0F FD03".RemoveWhiteSpaceFromHexadecimalString());

        // Act
        var actual = NextTransactionRequestData.ParseData(messageData);

        // Assert
        Assert.That(actual.TransactionIdentifier, Is.EqualTo(Enumerable.Range(0x00, 16).Select(i => (byte)i)));
        Assert.That(actual.TransactionSequence, Is.EqualTo(0x03));
    }
    
    [Test]
    public void ParseDataMissingTransactionIdentifierTest()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E3".RemoveWhiteSpaceFromHexadecimalString());

        // Act/Assert
        Assert.Catch(() => { NextTransactionRequestData.ParseData(messageData); });
    }
    
    [Test]
    public void BuildData()
    {
        // Arrange
        var nextTransactionRequestData =
            new NextTransactionRequestData(Enumerable.Range(0x00, 16).Select(i => (byte)i).ToArray(), 0x03);

        // Act
        var actual = nextTransactionRequestData.BuildData();

        Assert.That(actual.ToArray(), Is.EqualTo(Convert.FromHexString(
            "E3 4C10000102030405060708090A0B0C0D0E0F FD03".RemoveWhiteSpaceFromHexadecimalString())));
    }
}