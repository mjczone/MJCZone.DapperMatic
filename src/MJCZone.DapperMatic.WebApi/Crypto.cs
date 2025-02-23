using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Modes;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Security;

namespace MJCZone.DapperMatic.WebApi;

/// <summary>
/// Provides cryptographic functionality.
/// </summary>
/// <remarks>
/// This class is used to encrypt and decrypt data.
/// </remarks>
/// <example>
/// <code>
/// var encrypted = Crypto.Encrypt("Hello World", "MySecretKey");
/// var decrypted = Crypto.Decrypt(encrypted, "MySecretKey");
/// </code>
/// </example>
/// <seealso cref="System.Security.Cryptography.Aes"/>
/// <seealso cref="System.Security.Cryptography.Rfc2898DeriveBytes"/>
/// <seealso cref="System.Security.Cryptography.CryptoStream"/>
internal static class Crypto
{
    private const int SaltBitSize = 128;
    private const int Iterations = 100000;
    private const int NonceBitSize = 128;
    private const int KeyBitSize = 256;
    private const int MacBitSize = 128;

    private static readonly SecureRandom Random = new();

    /// <summary>
    /// Encrypts the specified plain text using the provided key and IV (Initialization Vector).
    /// </summary>
    /// <param name="clearText">The plain text to encrypt.</param>
    /// <param name="cryptoKey">The encryption key.</param>
    /// <returns>A byte array containing the encrypted data.</returns>
    public static string Encrypt(string clearText, string cryptoKey)
    {
        var plainText = Encoding.UTF8.GetBytes(clearText);
        var generator = new Pkcs5S2ParametersGenerator();
        var salt = new byte[SaltBitSize / 8];
        Random.NextBytes(salt);

        generator.Init(
            PbeParametersGenerator.Pkcs5PasswordToBytes(cryptoKey.ToCharArray()),
            salt,
            Iterations
        );

        var key = (KeyParameter)generator.GenerateDerivedMacParameters(KeyBitSize);

        var nonSecretPayload = Array.Empty<byte>();
        var payload = new byte[salt.Length];
        Array.Copy(nonSecretPayload, payload, nonSecretPayload.Length);
        Array.Copy(salt, 0, payload, nonSecretPayload.Length, salt.Length);

        var nonce = new byte[NonceBitSize / 8];
        Random.NextBytes(nonce, 0, nonce.Length);

        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(
            new KeyParameter(key.GetKey()),
            MacBitSize,
            nonce,
            payload
        );
        cipher.Init(true, parameters);

        var cipherText = new byte[cipher.GetOutputSize(plainText.Length)];
        var len = cipher.ProcessBytes(plainText, 0, plainText.Length, cipherText, 0);
        cipher.DoFinal(cipherText, len);

        using var combinedStream = new MemoryStream();
        using var binaryWriter = new BinaryWriter(combinedStream);
        binaryWriter.Write(payload);
        binaryWriter.Write(nonce);
        binaryWriter.Write(cipherText);

        return Convert.ToBase64String(combinedStream.ToArray());
    }

    /// <summary>
    /// Decrypts the specified encrypted text using the provided key.
    /// </summary>
    /// <param name="encryptedText">The encrypted text to decrypt.</param>
    /// <param name="cryptoKey">The decryption key.</param>
    /// <returns>The decrypted plain text.</returns>
    public static string Decrypt(string encryptedText, string cryptoKey)
    {
        var cipherText = Convert.FromBase64String(encryptedText);
        var generator = new Pkcs5S2ParametersGenerator();
        var salt = new byte[SaltBitSize / 8];
        Array.Copy(cipherText, 0, salt, 0, salt.Length);

        generator.Init(
            PbeParametersGenerator.Pkcs5PasswordToBytes(cryptoKey.ToCharArray()),
            salt,
            Iterations
        );

        var key = (KeyParameter)generator.GenerateDerivedMacParameters(KeyBitSize);

        using var cipherStream = new MemoryStream(cipherText);
        using var cipherReader = new BinaryReader(cipherStream);

        var payload = cipherReader.ReadBytes(salt.Length);
        var nonce = cipherReader.ReadBytes(NonceBitSize / 8);

        var cipher = new GcmBlockCipher(new AesEngine());
        var parameters = new AeadParameters(
            new KeyParameter(key.GetKey()),
            MacBitSize,
            nonce,
            payload
        );
        cipher.Init(false, parameters);

        var readBytes = cipherReader.ReadBytes(encryptedText.Length - salt.Length - nonce.Length);
        var plainTextBytes = new byte[cipher.GetOutputSize(readBytes.Length)];

        var len = cipher.ProcessBytes(readBytes, 0, readBytes.Length, plainTextBytes, 0);
        cipher.DoFinal(plainTextBytes, len);

        return Encoding.UTF8.GetString(plainTextBytes);
    }
}
