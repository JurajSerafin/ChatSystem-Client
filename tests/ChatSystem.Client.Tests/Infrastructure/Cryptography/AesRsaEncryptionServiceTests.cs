using ChatSystem.Client.Infrastructure.Cryptography;

namespace ChatSystem.Client.Tests.Infrastructure.Cryptography;

public class AesRsaEncryptionServiceTests {
    private readonly AesRsaEncryptionService _service = new();

    [Fact]
    public void GenerateKeyPair_ReturnsPemFormattedKeys() {
        // Arrange & Act
        var keyPair = _service.GenerateKeyPair();

        // Assert
        Assert.Contains("-----BEGIN RSA PRIVATE KEY-----", keyPair.PrivateKey);
        Assert.Contains("-----BEGIN RSA PUBLIC KEY-----", keyPair.PublicKey);
    }

    [Fact]
    public void GenerateSymmetricKey_ReturnsUniqueKeys() {
        // Arrange & Act
        const int requiredKeyLen = 32;

        var key1 = _service.GenerateSymmetricKey();
        var key2 = _service.GenerateSymmetricKey();

        // Assert
        Assert.Equal(requiredKeyLen, key1.Length);
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void EncryptDecryptSymmetric_RoundTrip_ReturnsOriginalPlaintext() {
        // Arrange
        var key = _service.GenerateSymmetricKey();
        const string original = "❌ secret message ❌";

        // Act
        var ciphertext = _service.EncryptSymmetric(original, key);
        var decrypted = _service.DecryptSymmetric(ciphertext, key);

        // Assert
        Assert.Equal(original, decrypted);
    }

    [Fact]
    public void EncryptSymmetric_SameInput_ProducesDifferentCiphertext() {
        // Arrange
        var key = _service.GenerateSymmetricKey();
        const string plaintext = "same message";

        // Act
        var ciphertext1 = _service.EncryptSymmetric(plaintext, key);
        var ciphertext2 = _service.EncryptSymmetric(plaintext, key);

        // Assert
        Assert.NotEqual(ciphertext1, ciphertext2);
    }

    [Fact]
    public void WrapUnwrapKey_ReturnsOriginalSymmetricKey() {
        // Arrange
        var keyPair = _service.GenerateKeyPair();
        var symmetricKey = _service.GenerateSymmetricKey();

        // Act
        var wrapped = _service.WrapKey(symmetricKey, keyPair.PublicKey);
        var unwrapped = _service.UnwrapKey(wrapped, keyPair.PrivateKey);

        // Assert
        Assert.Equal(symmetricKey, unwrapped);
    }


    [Fact]
    public void SignVerify_ReturnsTrue() {
        // Arrange
        var keyPair = _service.GenerateKeyPair();
        var data = "SIGN_ME"u8.ToArray();

        // Act
        var signature = _service.Sign(data, keyPair.PrivateKey);
        var isValid = _service.Verify(data, signature, keyPair.PublicKey);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void Verify_ModifiedData_ReturnsFalse() {
        // Arrange
        var keyPair = _service.GenerateKeyPair();
        var data = "original"u8.ToArray();
        var signature = _service.Sign(data, keyPair.PrivateKey);

        // Act
        var modified = "modidfied"u8.ToArray();
        var isValid = _service.Verify(modified, signature, keyPair.PublicKey);

        Assert.False(isValid);
    }
}