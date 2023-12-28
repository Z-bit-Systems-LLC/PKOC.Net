using System;
using System.Security.Cryptography;

namespace PKOC.Net
{
    internal static class Utilities
    {
        internal static bool ValidateSignature(ReadOnlySpan<byte> data, ReadOnlySpan<byte> publicKey, ReadOnlySpan<byte> signature)
        {
            using (var algorithm = ECDsa.Create(new ECParameters
                   {
                       Curve = ECCurve.NamedCurves.nistP256,
                       Q =
                       {
                           X = publicKey.Slice(1, 32).ToArray(),
                           Y = publicKey.Slice(33).ToArray()
                       }
                   }))
            {
#if NETSTANDARD2_0
                return algorithm.VerifyData(data.ToArray(), signature.ToArray(), HashAlgorithmName.SHA256);
#else
                return algorithm.VerifyData(data, signature, HashAlgorithmName.SHA256);
#endif
            }
        }
    }
}