using System.Collections.Generic;

namespace ChatSystem.Client.Core.Interfaces.Cryptography;

internal readonly record struct EncryptedKeyMaterial(
    List<byte> salt,
    List<byte> encryptedKey,
    string algorithm
);