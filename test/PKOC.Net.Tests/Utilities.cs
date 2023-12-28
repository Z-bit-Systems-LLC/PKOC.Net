using System.Security.Cryptography;

namespace PKOC.Net.Tests;

public static class Utilities
{
    /// <summary>
    /// Removes white space characters from a hexadecimal string.
    /// </summary>
    /// <param name="hexadecimalString">The input hexadecimal string.</param>
    /// <returns>The hexadecimal string with all white space characters removed.</returns>
    public static string RemoveWhiteSpaceFromHexadecimalString(this string hexadecimalString)
    {
        return hexadecimalString.Replace(" ", string.Empty);
    }
    
    /// <summary>
    /// TODO Save for future use
    /// </summary>
    public static void ValidateSignature()
    {
        string publicKeyHex = "040ec5d87dc39d14a2c5480686da860c82b16be0b6903b525f84848b79fd463e32bbda1f0252c33503c5287035e6eac55d138d0650dcfb5281d59a9cf4124d2831";
        byte[] publicKeyBytes = Convert.FromHexString(publicKeyHex);

        using ECDsa publicKey = ECDsa.Create(new ECParameters
        {
            Curve = ECCurve.NamedCurves.nistP256,
            Q =
            {
                X = publicKeyBytes.Skip(1).Take(32).ToArray(),
                Y = publicKeyBytes.Skip(33).ToArray()
            }
        });

        // Load the signature and original data
        byte[] signature = Convert.FromHexString("b98613070c78010b04ed306d143f94ee6dc4eca2585b621405731fb3a53cd877a21685de18435da7cbcc38f1d926300a454efee3594cec5effe28c7feac03d7d");
        byte[] data = Convert.FromHexString("6fcf5012b224043b09350a4fc5e56a8f");

        // Verify the signature
        bool isSignatureValid = publicKey.VerifyData(data, signature, HashAlgorithmName.SHA256);

        Assert.That(isSignatureValid, Is.True);
    }
}