using System;
using ChatSystem.Client.Infrastructure.Cryptography;

namespace ChatSystem.Client.Tests.Infrastructure.Cryptography;

public class Argon2KeyDerivationServiceTests {
    private readonly Argon2KeyDerivationService _service = new();

    [Fact]
    public void DeriveKey_ReturnsExpectedLength() {
        // Arrange
        const int requiredLen = 32;
        var salt = _service.GenerateSalt();
        var parameters = _service.GetDefaultParams();

        // Act
        var key = _service.DeriveKey("password", salt, parameters);

        // Assert
        Assert.Equal(requiredLen, key.Length);
    }

    [Fact]
    public void DeriveKey_DifferentPassword_ProducesDifferentKey() {
        // Arrange
        var salt = _service.GenerateSalt();
        var parameters = _service.GetDefaultParams();

        // Act
        var key1 = _service.DeriveKey("passwor1", salt, parameters);
        var key2 = _service.DeriveKey("password2", salt, parameters);

        // Assert
        Assert.NotEqual(key1, key2);
    }

    [Fact]
    public void GetDefaultParams_ReturnsRecommendedValues() {
        // Arrange & Act
        var parameters = _service.GetDefaultParams();

        // Assert
        Assert.Equal(19456, parameters.MemorySizeKib);
        Assert.Equal(2, parameters.Iterations);
        Assert.Equal(1, parameters.DegreeOfParallelism);
    }

    [Fact]
    public void ParseAlgorithmId_ValidId_ReturnsCorrectParams() {
        // Arrange & Act
        var parsed = _service.ParseAlgorithmKdfParams("argon2id-19456-2-1");

        // Assert
        Assert.Equal(19456, parsed.MemorySizeKib);
        Assert.Equal(2, parsed.Iterations);
        Assert.Equal(1, parsed.DegreeOfParallelism);
    }

    [Fact]
    public void ParseAlgorithmId_MalformedId_ThrowsFormatException() {
        // Arrange & Act & Assert
        Assert.Throws<FormatException>(
            () => _service.ParseAlgorithmKdfParams("not-a-valid-id"));
    }

    [Fact]
    public void NeedsRehash_WeakParams_ReturnsTrue() {
        // Arrange
        var weakerAlgorithmId = "argon2id-8192-1-1";

        // Act & Assert
        Assert.True(_service.NeedsRehash(weakerAlgorithmId));
    }

    [Fact]
    public void NeedsRehash_CurrentDefaultParams_ReturnsFalse() {
        // Arrange
        var currentAlgorithmId = _service.GetDefaultParams();

        // Act & Assert
        Assert.False(_service.NeedsRehash(currentAlgorithmId.ToAlgorithmId()));
    }
}