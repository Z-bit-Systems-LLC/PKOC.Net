using PKOC.Net.MessageData;

namespace PKOC.Net.Tests.MessageData;

[TestFixture]
[TestOf(typeof(ReaderErrorResponseData))]
public class TransactionRefreshResponseDataTest
{
    [Test]
    public void ParseDataTest()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E4");

        // Act
        var actual = TransactionRefreshResponseData.ParseData(messageData);

        // Assert
        Assert.That(actual, Is.Not.Null);
    }
    
    [Test]
    public void ParseDataExtraBytes()
    {
        // Arrange
        var messageData = Convert.FromHexString("E4 00".RemoveWhiteSpaceFromHexadecimalString());

        // Act/Assert
        Assert.Catch(() => { TransactionRefreshResponseData.ParseData(messageData); });
    }
    
    [Test]
    public void ParseDataInvalidDataType()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E5");

        // Act/Assert
        Assert.Catch(() => { TransactionRefreshResponseData.ParseData(messageData); });
    }
    
    [Test]
    public void BuildDataTest()
    {
        // Arrange
        var transactionRefreshResponseData = new TransactionRefreshResponseData();

        // Act
        var actual = transactionRefreshResponseData.BuildData();

        Assert.That(actual.ToArray(),
            Is.EqualTo(Convert.FromHexString("E4".RemoveWhiteSpaceFromHexadecimalString())));
    }
}