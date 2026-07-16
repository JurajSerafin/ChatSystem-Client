using System.Collections.Generic;

namespace ChatSystem.Client.Core.Interfaces.Cryptography;

/// <summary>
/// Parameters defining the work-factor for a Key Derivation Function.
/// </summary>
internal interface IAbstractKdfParams {
    static abstract string AlgorithmId { get; }
}

/// <summary>
/// Interface for a service for deriving strong cryptographic keys from user passwords.
/// </summary>
internal interface IKeyDerivationService  {

    /// <summary>
    /// Generates a cryptographically secure random salt.
    /// </summary>
    /// <returns>A cryptographic salt - vector of random bytes.</returns>
    public IList<byte> GenerateSalt();

    /// <summary>
    /// Derives a fixed-length cryptographic key from a plaintext password and salt.
    /// </summary>
    /// <param name="password">The plaintext password provided by the user.</param>
    /// <param name="salt">The cryptographic salt.</param>
    /// <param name="kdfParams">The cost factors and parameters for the specific KDF.</param>
    /// <returns>The derived cryptographic key.</returns>
    public string DeriveKey(string password, IList<byte> salt, IAbstractKdfParams kdfParams);

    /// <summary>
    /// Returns the current recommended default parameters for key derivation.
    /// </summary>
    /// <returns>The default KdfParams instance.</returns>
    public KdfParams GetDefaultParams();

    /// <summary>
    /// Parses an algorithm identifier string into a structured KdfParams object.
    /// </summary>
    /// <param name="algId">The raw algorithm identification string.</param>
    /// <returns>The parsed KdfParams.</returns>
    public KdfParams ParseAlgorithmId(string algId);

    /// <summary>
    /// Checks if the provided algorithm identifier uses outdated parameters requiring an upgrade.
    /// </summary>
    /// <param name="algId">The algorithm identifier string to evaluate.</param>
    /// <returns>True if a rehash is recommended, false otherwise.</returns>
    public bool NeedsRehash(string algId);
}