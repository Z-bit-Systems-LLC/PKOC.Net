using PKOC.Net.MessageData;

namespace PKOC.Net.Tests.MessageData;

[TestFixture]
[TestOf(typeof(AuthenticationResponseDataTest))]
public class AuthenticationResponseDataTest
{
    [Test]
    public void ParseDataTest()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E2 5A41040EC5D87DC39D14A2C5480686DA860C82B16BE0B6903B525F84848B79FD463E32BBDA1F0252C33503C5287035E6EAC55D138D0650DCFB5281D59A9CF4124D2831 9E40B98613070C78010B04ED306D143F94EE6DC4ECA2585B621405731FB3A53CD877A21685DE18435DA7CBCC38F1D926300A454EFEE3594CEC5EFFE28C7FEAC03D7D 4C10000102030405060708090A0B0C0D0E0F FD03 FB02"
                    .RemoveWhiteSpaceFromHexadecimalString());

        // Act
        var actual = AuthenticationResponseData.ParseData(messageData);

        // Assert
        Assert.That(actual.PublicKey,
            Is.EqualTo(Convert.FromHexString(
                "040EC5D87DC39D14A2C5480686DA860C82B16BE0B6903B525F84848B79FD463E32BBDA1F0252C33503C5287035E6EAC55D138D0650DCFB5281D59A9CF4124D2831")));
        Assert.That(actual.DigitalSignature,
            Is.EqualTo(Convert.FromHexString(
                "B98613070C78010B04ED306D143F94EE6DC4ECA2585B621405731FB3A53CD877A21685DE18435DA7CBCC38F1D926300A454EFEE3594CEC5EFFE28C7FEAC03D7D")));
        Assert.That(actual.TransactionIdentifier, Is.EqualTo(Enumerable.Range(0x00, 16).Select(i => (byte)i)));
        Assert.That(actual.TransactionSequence, Is.EqualTo(0x03));
        Assert.That(actual.Error, Is.EqualTo(new[] { 0x02 }));
    }

    [Test]
    public void ParseDataMissingPublicKey()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E2 9E40B98613070C78010B04ED306D143F94EE6DC4ECA2585B621405731FB3A53CD877A21685DE18435DA7CBCC38F1D926300A454EFEE3594CEC5EFFE28C7FEAC03D7D".RemoveWhiteSpaceFromHexadecimalString());

        // Act/Assert
        Assert.Catch(() => { AuthenticationRequestData.ParseData(messageData); });
    }
    
    [Test]
    public void ParseDataMissingDigitalSignature()
    {
        // Arrange
        var messageData =
            Convert.FromHexString(
                "E2 5A41040EC5D87DC39D14A2C5480686DA860C82B16BE0B6903B525F84848B79FD463E32BBDA1F0252C33503C5287035E6EAC55D138D0650DCFB5281D59A9CF4124D2831".RemoveWhiteSpaceFromHexadecimalString());

        // Act/Assert
        Assert.Catch(() => { AuthenticationRequestData.ParseData(messageData); });
    }

    [Test]
    public void BuildData()
    {
        // Arrange
        var nextTransactionRequestData =
            new AuthenticationResponseData(
                Convert.FromHexString(
                    "040EC5D87DC39D14A2C5480686DA860C82B16BE0B6903B525F84848B79FD463E32BBDA1F0252C33503C5287035E6EAC55D138D0650DCFB5281D59A9CF4124D2831"),
                Convert.FromHexString(
                    "B98613070C78010B04ED306D143F94EE6DC4ECA2585B621405731FB3A53CD877A21685DE18435DA7CBCC38F1D926300A454EFEE3594CEC5EFFE28C7FEAC03D7D"),
                Enumerable.Range(0x00, 16).Select(i => (byte)i).ToArray(),
                0x03,
                new byte[] { 0x02 });

        // Act
        var actual = nextTransactionRequestData.BuildData();

        Assert.That(actual.ToArray(), Is.EqualTo(Convert.FromHexString(
            "E2 5A41040EC5D87DC39D14A2C5480686DA860C82B16BE0B6903B525F84848B79FD463E32BBDA1F0252C33503C5287035E6EAC55D138D0650DCFB5281D59A9CF4124D2831 9E40B98613070C78010B04ED306D143F94EE6DC4ECA2585B621405731FB3A53CD877A21685DE18435DA7CBCC38F1D926300A454EFEE3594CEC5EFFE28C7FEAC03D7D 4C10000102030405060708090A0B0C0D0E0F FD03 FB02"
                .RemoveWhiteSpaceFromHexadecimalString())));
    }
}