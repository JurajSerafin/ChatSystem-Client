namespace ChatSystem.Client.Core.Interfaces.Cryptography;

/// <summary>
/// Argon2id parameters used to derive an encryption key from a password.
/// </summary>
/// <param name="MemorySizeKib">RAM allocated per hashing operation, in KiB.</param>
/// <param name="Iterations">Number of internal iterations (time cost).</param>
/// <param name="DegreeOfParallelism">Number of parallel lanes to use.</param>
/// <param name="AlgorithmId">
/// Wire identifier stored alongside the encrypted key, e.g. "argon2id-19456-2-1".
/// Enables detecting outdated parameters for rehashing.
/// </param>
public sealed record KdfParams(
    int MemorySizeKib,
    int Iterations,
    int DegreeOfParallelism,
    string AlgorithmId
);
