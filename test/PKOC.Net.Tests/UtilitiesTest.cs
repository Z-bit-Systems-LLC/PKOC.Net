namespace PKOC.Net.Tests;

[TestFixture]
[TestOf(typeof(Net.Utilities))]
public class UtilitiesTest
{

    [Test]
    public void ValidateSignature()
    {
        // Arrange
        byte[] publicKey = Convert.FromHexString("040ec5d87dc39d14a2c5480686da860c82b16be0b6903b525f84848b79fd463e32bbda1f0252c33503c5287035e6eac55d138d0650dcfb5281d59a9cf4124d2831");
        byte[] signature = Convert.FromHexString("b98613070c78010b04ed306d143f94ee6dc4eca2585b621405731fb3a53cd877a21685de18435da7cbcc38f1d926300a454efee3594cec5effe28c7feac03d7d");
        byte[] data = Convert.FromHexString("6fcf5012b224043b09350a4fc5e56a8f");

        // Act
        bool isSignatureValid = Net.Utilities.ValidateSignature(data, publicKey, signature);

        // Assert
        Assert.That(isSignatureValid, Is.True);
    }
}