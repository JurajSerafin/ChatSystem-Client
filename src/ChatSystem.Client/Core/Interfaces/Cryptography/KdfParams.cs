using System;

namespace ChatSystem.Client.Core.Interfaces.Cryptography;

/// <summary>
/// Argon2id parameters used to derive an encryption key from a password.
/// </summary>
/// <param name="MemorySizeKib">RAM allocated per hashing operation, in KiB.</param>
/// <param name="Iterations">Number of internal iterations (time cost).</param>
/// <param name="DegreeOfParallelism">Number of parallel lanes to use.</param>
/// Wire identifier stored alongside the encrypted key, e.g. "argon2id-19456-2-1".
/// Enables detecting outdated parameters for rehashing.
public sealed record KdfParams(
    int MemorySizeKib,
    int Iterations,
    int DegreeOfParallelism
) {

    private const string Algorithm = "argon2id";

    private const char AlgIdSep = '-';

    private const int AlgIdPartsNum = 4;

    /// <summary>
    /// Serializes the parameters into a persistent algorithm identifier.
    /// Format: "argon2id-{memory}-{iterations}-{parallelism}"
    /// </summary>
    public string ToAlgorithmId() =>
        $"{Algorithm}{AlgIdSep}{MemorySizeKib}{AlgIdSep}{Iterations}{AlgIdSep}{DegreeOfParallelism}";

    /// <summary>
    /// Reconstructs parameters from a stored algorithm identifier.
    /// </summary>
    public static KdfParams FromAlgorithmId(string algorithmId) {
        var parts = algorithmId.Split('-');

        if (parts.Length != 4
            || parts[0] != "argon2id"
            || !int.TryParse(parts[1], out var memory)
            || !int.TryParse(parts[2], out var iterations)
            || !int.TryParse(parts[3], out var parallelism)) {
            throw new FormatException($"Invalid Argon2 algorithm identifier: '{algorithmId}'.");
        }

        return new KdfParams(memory, iterations, parallelism);
    }

    public override string ToString() => ToAlgorithmId();
}
