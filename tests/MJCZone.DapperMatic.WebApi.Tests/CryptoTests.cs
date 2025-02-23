namespace MJCZone.DapperMatic.WebApi.Tests;

public class CryptoTests
{
    [Fact]
    public void Can_encrypt_and_decrypt_text_using_crypto_class()
    {
        // create a crypto key with 36 alpha-numeric characters and one special character, and one lowercase and uppercase letter at least
        var cryptoKey =
            "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()_+";

        // create a test string
        var testString = "This is a test string";

        // encrypt the test string
        var encryptedString = Crypto.Encrypt(testString, cryptoKey);

        // decrypt the encrypted string
        var decryptedString = Crypto.Decrypt(encryptedString, cryptoKey);

        // assert that the decrypted string is equal to the test string
        Assert.Equal(testString, decryptedString);
    }

    [Fact]
    public void Can_encrypt_and_decrypt_long_text_using_crypto_class()
    {
        // create a crypto key with 36 alpha-numeric characters and one special character, and one lowercase and uppercase letter at least
        var cryptoKey =
            "1234567890abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ!@#$%^&*()_+";

        // create a really long string (english sentence)
        var testString =
            @"This is a really long string that contains a lot of characters. 
            It is used to test the encryption and decryption functionality of the crypto class. 
            It should be able to handle long strings without any issues.";

        // add some special characers
        testString += "!@#$%^&*()_+{}|:\"<>?";

        // add some accents
        testString += "Café";

        // encrypt the test string
        var encryptedString = Crypto.Encrypt(testString, cryptoKey);

        // decrypt the encrypted string
        var decryptedString = Crypto.Decrypt(encryptedString, cryptoKey);

        // assert that the decrypted string is equal to the test string
        Assert.Equal(testString, decryptedString);
    }
}
