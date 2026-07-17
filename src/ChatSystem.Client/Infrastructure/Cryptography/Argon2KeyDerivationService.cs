using System.Security.Cryptography;
using System.Text;
using ChatSystem.Client.Core.Interfaces.Cryptography;
using Konscious.Security.Cryptography;

namespace ChatSystem.Client.Infrastructure.Cryptography;

/// <summary>
/// Concrete implementation of the Key Derivation Service using System.Security.Cryptography and
/// Konscious.Security.Cryptography package.
///
/// Utilizes strong, memory-hard algorithms Argon2 to securely derive keys from user passwords,
/// protecting against brute-force attacks.
/// </summary>
internal class Argon2KeyDerivationService : IKeyDerivationService {

    private const int SaltSize = 16;

    private const int DerivedKeySize = 32;


    private const int DefaultMemorySizeKib = 19456;

    private const int DefaultIterations = 2;

    private const int DefaultParallelism = 1;


    public byte[] GenerateSalt() {
        return RandomNumberGenerator.GetBytes(SaltSize);
    }
    public byte[] DeriveKey(string password, byte[] salt, KdfParams kdfParams) {
        var passBytes = Encoding.UTF8.GetBytes(password);

        using var argon2 = new Argon2id(passBytes) {
            Salt = salt,
            MemorySize = kdfParams.MemorySizeKib,
            Iterations = kdfParams.Iterations,
            DegreeOfParallelism = kdfParams.DegreeOfParallelism
        };

        return argon2.GetBytes(DerivedKeySize);
    }


    public KdfParams GetDefaultParams() {
        return new KdfParams(
            DegreeOfParallelism: DefaultParallelism,
            Iterations: DefaultIterations,
            MemorySizeKib: DefaultMemorySizeKib
        );
    }

    public KdfParams ParseAlgorithmKdfParams(string algoritmParamsToken) {
        return KdfParams.FromAlgorithmId(algoritmParamsToken);
    }

    public bool NeedsRehash(string algId) {
        try {
            var curr = ParseAlgorithmKdfParams(algId);

            return curr.MemorySizeKib < DefaultMemorySizeKib
                   || curr.Iterations < DefaultIterations
                   || curr.DegreeOfParallelism < DefaultParallelism;
        } catch {
            return true;
        }
    }
}