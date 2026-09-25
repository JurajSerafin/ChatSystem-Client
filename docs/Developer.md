# ChatSystem Client developer documentation

## Architecture overview

The code is split into **4** main parts:

 - **Config** (Server setup, DI Registration)

 - **Core** (Interfaces, Database business logic)

 - **Infrastructure** (Implementation)

 - **Presentation**

## Used libraries

| Libraries | Purpose |
|-----------|----------|
|**Avalonia**| GUI |
| **SQLite**, **Entity Framework** | Local Cache |
|**Refit**	| Networking (API contracts, REST calls, serialization, etc.) |
|**System.Security.Cryptography**| AES and RSA cryptographic algorithms |
| **Konscious.Security.Cryptography.Argon2**| Password-based **MEK** key derivation |

## Config

Contains a setup of external settings and internal dependency injection container.


### ServiceCollectionExtensions

Manages Dependency Injection registrations into extension methods on
`IServiceCollection`, which is .NET's registry container used to configure and register dependencies before an application runs.

The items registration connects contracts from **Core** with their implementations from either **Infrastructure** and **Presentation**

The one method called on startup, which wires up the entire dependency graph is `AddChatSystemServices()`.

Each registration of a dependency also strictly sets its lifetime to one of:

 - **Singleton:** Single instance during the whole application lifetime. Used for stateless services: `IClientEncryptionService`, `IKeyDerivationService`, `INavigationService` and `ISessionScopeService`

 - **Scoped:** Single instance per a `CreateScope()` call. It is instantiated the first time it is aked for, reused forall the other requests within the same scope, and destroyed when the scope ends. Used for session-bound dependencies tied to a logged-in user such as EF Core's `ChatSystemLocalDbContext`, local repositories,
 services and ViewModels

 - **Transient:** Single instance per request. Used for UI context or HTTP message handlers.


Read [more:](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/overview)


### ServerOptions

Manages backend server connection strings. This class is bound directly to `appsettings.json`:

```json
{
  "Server": {
    "BaseUrl": "http://localhost:8080",
    "TimeoutSeconds": 30
  }
}
```
It loads its server connection settings and provides fallback defaults if the file or any of its keys is missing.


## Core

Defines the architectureal rules, contracts, and domain models.

### Domain models

Business entities for chats, messages, users and their strongly typed identifiers. It enforces strict compile-time safety, offline-first local caching rules and smooth serialization. Let us present this on a Chat entity as an illustration. This is analogous for the other entities.

#### Strongly typed IDs

Using primitive types to uniquely represent entities introduces risks for accidental swaps (passing `UserId` into a method expecting `ChatId`). Therefore, we use `readonly record struct`s to prevent this,
and wrap the primitive ID value itself in an object with clear API,
demonstrating its purpose.

```csharp
[JsonConverter(typeof(ChatIdJsonConverter))]
public readonly record struct ChatId(Guid Value) : IId<ChatId> {
    public static ChatId Create(Guid guid) => new(guid);
    public override string ToString() => Value.ToString();
}

internal sealed class ChatIdJsonConverter : DefaultIdJsonConverter<ChatId>;
```

With this `ChatId` cannot be mistakenly assigned to a variable expecting other ID type. Implementing it as a `readonly record struct`
saves heap allocation and provides value equality semantics.

`IId<T>` contract, which all entity ID classes implement, provides a static factory method `Create(Guid)`, enabling the static `IdFactory` to construct IDs dynamically during parsing.

```csharp

internal static class IdFactory {

    public static TId Generate<TId>() where TId : struct, IId<TId> {
        return TId.Generate();
    }

    public static TId Parse<TId>(string idString) where TId : struct, IId<TId> {
        return TId.Parse(idString);
    }

    public static TId Create<TId>(Guid guid) where TId : struct, IId<TId> {
        return TId.Create(guid);
    }
}
```


`ChatIdJsonConverter` inheriting from `DefaultIdJsonConverter<T>`
ensures `ChatId` serialization to and from a plain JSON string/GUID.

#### Domain entity class

`CachedChat` models a chat conversation within the local cache storage

```csharp

public class CachedChat {

    public required ChatId Id { get; set; }

    public MessageId? LastMessageId { get; set; }

    [MaxLength(128)]
    public string? Name { get; set; }

    public required DateTimeOffset CreatedAt { get; set; }

    public required DateTimeOffset LastActivityAt { get; set; }

    public required DateTimeOffset CachedAt { get; set; }

    public required bool IsDeleted { get; set; } = false;

    public List<CachedChatParticipant> Participants { get; set; } = [];

}
```

 - strongly typed IDs serve as primary and foreign keys

 - `DateTimeOffset` properties allow the services to determine whether the local cache is stale or needs revalidation against the server, or may
 provide interesting information to the user about their used chats

 - `IsDeleted` serves for soft deletion, enabling smooth local deletion workflow

#### Junction entity (`ChatParticipant`)

```csharp

public class CachedChatParticipant {

    public UserId UserId { get; set; }

    public ChatId ChatId { get; set; }

    [MaxLength(32)]
    public string Role { get; set; } = null!;


    //--- Entity navigation

    public CachedUser User { get; set; } = null!;

    public CachedChat Chat { get; set; } = null!;
}

```

To handle many-to-many relationships between users and chats in a local cache database, `ChatParticipant` acts as an junction entity that carries context about such relationship.

 - strongly typed IDs ensure that a user belongs to a specific chat room

 - `Role` property stores details about specific user's connection and capabilities to that chat

 - navigation properties enable object-graph traversal without leaking inner database information into higher-level services 


### Interfaces and abstractions

Propose contracts for all business logic repositories and services.

#### Cryptography services and related utilities

This layout layer organizes conctracts for low-level cryptographic engines, higher-level domain facades, storage repository interfaces and their utility structures.

```csharp

internal interface IClientEncryptionService {

    public KeyPair GenerateKeyPair();

    public byte[] GenerateSymmetricKey();

    public byte[] EncryptSymmetric(string plaintext, byte[] key);

    public string DecryptSymmetric(byte[] ciphertext, byte[] key);

    public byte[] WrapKey(byte[] symmetricKey, string publicKeyPem);

    public byte[] UnwrapKey(byte[] wrappedKey, string privateKeyPem);

    public byte[] Sign(byte[] data, string privateKey);

    public bool Verify(byte[] data, byte[] signature, string publicKeyPem);

    public string DerivePublicKey(string privateKeyPem);
}

```

Defines primitive operations for E2EE pipelines. This includes asymmetric key pair generation, AES symmetric payload encryption/decryption, RSA key wrapping and digital signatures.

```csharp

internal interface IKeyDerivationService  {

    public byte[] GenerateSalt();

    public byte[] DeriveKey(string password, byte[] salt, KdfParams kdfParams);

    public KdfParams GetDefaultParams();

    public KdfParams ParseAlgorithmKdfParams(string algoritmParamsToken);

    public bool NeedsRehash(string algId);
}

```

Defines the contract for password-based key derivation functions (KDF). This includes generating cryptographically secure salt, deriving Master Encryption Keys (MEKs) from user password, parsing parameter tokens and checking if KDF parameters require upgrading.

```csharp

internal interface IClientKeyManager {

    public Task<KeyPair> GenerateAndProtectKeyPairAsync(string password, CancellationToken cancellationToken = default);

    public Task UnlockPrivateKeyAsync(string password, CancellationToken cancellationToken = default);

    public void LockPrivateKey();

    public string GetPrivateKey();

    public string GetPublicKey();

    public Task DeleteProtectedKeysAsync(CancellationToken cancellationToken = default);
}

```

Combines multiple low-level primitives and repositories into fully organized workflows. This includes coordinating key generation, MEK derivation, key protection, memory lifecycle management and database persistance.

```csharp

internal interface IKeyStore {

    public Task StoreAsync(EncryptedKeyMaterial encryptedKeyMaterial, CancellationToken cancellationToken = default);

    public Task<EncryptedKeyMaterial?> LoadAsync(CancellationToken cancellationToken = default);

    public Task ClearAsync(CancellationToken cancellationToken = default);

    public int KeyId { get; }
}

```

Serves as a cryptographic repository; defines contracts for the persistance layer without expososing the used database technology. It enforces a single-row constraint to isolate the active user's encrypted identity key material.

All these proposed contracts rely on explicit domain objects, wrapping the primitive parameters.

```csharp

internal readonly record struct KeyPair(string PublicKey, string PrivateKey) {

    public bool Equals(KeyPair other) {
        return PublicKey == other.PublicKey;
    }

    public override int GetHashCode() {
        return string.GetHashCode(PublicKey);
    }
}
```

```csharp

public sealed record KdfParams(
    int MemorySizeKib,
    int Iterations,
    int DegreeOfParallelism
) {

    private const string Algorithm = "argon2id";

    private const char AlgIdSep = '-';

    private const int AlgIdPartsNum = 4;

    public string ToAlgorithmId() => ...;

    public static KdfParams FromAlgorithmId(string algorithmId) {...}

    public override string ToString() => ToAlgorithmId();
}

```

Argon2id is the KDF used within ChatSystem. It is a memory-hard algorithm that forces a specific RAM allocation and iteration count per guess, making guessing the hash with GPUs cost-prohibitive.

The KDF string (e.g., "`argon2id-19456-2-1`") serializes the exact algorithm parameters —Memory Size, Iterations, and Parallelism—alongside the encrypted key material. Storing this string in `EncryptedKeyMaterial` is crucial for:

 - **Deterministic key reconstruction:** Argon2id is deterministic, but only if its input parameters are identical. Without storing the exact parameters used during creation, `UnlockPrivateKeyAsync()` would fail to re-derive the correct MEK, making the encrypted key permanently unreadable.

 - **Smooth security upgrades:** Harware becomes faster over time which increases cryptographic work-factors. By storing the KDF parameter string with the record, the system is able to inspect old keys during unlock using the `NeedsRehash(string algId)` function:

```csharp

public bool NeedsRehash(string algId) {
    var curr = ParseAlgorithmKdfParams(algId);
    return curr.MemorySizeKib < DefaultMemorySizeKib
        || curr.Iterations < DefaultIterations
        || curr.DegreeOfParallelism < DefaultParallelism;
}

```

If a user created their account on a weaker KDF settings, the application can detect this on a successful login, re-derive the new MEK with updated parameters, re-encrypt their private key, and update the stored KDF string automatically

`KdfParams` record captures the work-factors and wires the KDF string serialization.

```csharp

public class EncryptedKeyMaterial {

    public int Id { get; set; }

    [MaxLength(32)]
    public required string Algorithm { get; set; }

    public required byte[] EncryptedKey { get; set; }

    public required byte[] Salt { get; set; }

    public DateTimeOffset StoredAt { get; set; }
}

```

Groups the encrypted private key ciphertext alongside its crypthographic salt, KDF string and storage timestamp.

#### Networking Contracts

Defines REST endpoints and DTOs necessary with the server communications. It separates the network protocol and the client's internal logic.

By utilizing Refit alongside ``System.Text.Json` DTOs, this layer defines what endpoints exist and what JSON data shapes are exchanged without tying the client to a specific HTTP client implementation.

This layer is modularized based on the business feature, each component containing conctracts (`IAuthApi`, `IChatApi`, etc.) and DTOs wrapping network requests and responses.

We will illustrate this on the `Message/` component.

**Declarative REST contract**

```csharp

internal interface IMessageApi {

    [Post("/chats/{id}/messages")]
    Task<SingleMessageResponse> SendMessageAsync(ChatId id, [Body] SendMessageRequest request, CancellationToken cancellationToken = default);

    [Get("/chats/{id}/messages")]
    Task<IReadOnlyList<SingleMessageResponse>> GetHistoryAsync(
        ChatId id,
        [Query] int limit,
        [Query] int offset,
        CancellationToken cancellationToken = default
    );

    [Put("/messages/{id}/read")]
    Task MarkAsReadAsync(MessageId id, CancellationToken cancellationToken = default);

    [Get("/messages/undelivered")]
    Task<IReadOnlyList<SingleMessageResponse>> GetUndeliveredAsync(
        [Query] int limit,
        [Query] MessageId? afterId,
        CancellationToken cancellationToken = default
    );

    [Get("/messages/{id}/key")]
    Task<GetEncryptedKeyResponse> GetEncryptedKeyAsync(
        MessageId id,
        CancellationToken cancellationToken = default);
}

```

The API interfaces use Refit attributes to describe the HTTP contract declaratively:

 - **Route Parameter Mapping:** `[Post("/chats/{id}/messages")]` automatically maps the method's `ChatId id` parameter into the URL path.

 - **Query String Formatting:** Attributes like `[Query] int limit` serialize parameters into standard URL query strings (e.g., ?limit=100&offset=0).

 - **Request Body Binding:** `[Body] SendMessageRequest request` automatically serializes the C# record into a JSON request payload.

 - **Asynchronous Execution:** Every method returns a `Task` and accepts an optional `CancellationToken` for request abortion.

Wrapping E2EE request payload:

```csharp

internal record SendMessageRequest(
    [property: JsonPropertyName("ciphertext")]
    string CipherText,

    [property: JsonPropertyName("encrypted_keys")]
    IReadOnlyDictionary<UserId, string> EncryptedKeys,

    [property: JsonPropertyName("type")]
    string MessageType
);

```

 - **Ciphertext:** A single payload encrypted symmetrically with a temporary AES session key.

 - **Participant Key Mapping:** `IReadOnlyDictionary<UserId, string>` maps participants (identified by `UserId`) with the AES session key wrapped specifically in that user's RSA public key.

Wrapping server response payloads:

```csharp

internal record SingleMessageResponse(
    [property: JsonPropertyName("id")]
    MessageId Id,

    [property: JsonPropertyName("sender_id")]
    UserId SenderId,

    [property: JsonPropertyName("chat_id")]
    ChatId ChatId,

    [property: JsonPropertyName("ciphertext")]
    string Ciphertext,

    [property: JsonPropertyName("type")]
    string Type,

    [property: JsonPropertyName("created_at")]
    DateTimeOffset CreatedAt
);

  internal record GetEncryptedKeyResponse(
      [property: JsonPropertyName("encrypted_key")] string EncryptedKey
  );
```

These `SingleMessageResponse` DTOs integrate the domain classes' strongly typed IDs directly into the contract rather than raw `Guid` primitives. Custom JSON converters automatically handle conversion during network deserialization.

`GetEncryptedKeyResponse` provides a payload specifically for requesting the wrapped AES key corresponding to the current logged-in user when decrypting message history.

All DTOs use `[property: JsonPropertyName("...")]`. This maps standard C# `PascalCase` record properties to the server's expected `snake_case` JSON fields (e.g., sender_id, created_at), keeping the network naming conventions decoupled from the C# ones:

```csharp

private async Task<CachedMessage?> DecryptAndPersistAsync(
    SingleMessageResponse dto,
    CancellationToken cancellationToken
) { 
  ...

  var keyResponse = await _messageApi.GetEncryptedKeyAsync(dto.Id, cancellationToken);

  var wrappedKey = Convert.FromBase64String(keyResponse.EncryptedKey);
  var privateKey = _keyManager.GetPrivateKey();

  var sessionKey = _crypto.UnwrapKey(wrappedKey, privateKey);

  var ciphertext = Convert.FromBase64String(dto.Ciphertext);
  var plaintext = _crypto.DecryptSymmetric(ciphertext, sessionKey);

  var cached = new CachedMessage {
      Id = dto.Id,
      ChatId = dto.ChatId,
      SenderId = dto.SenderId,
      PlainText = plaintext,
      Type = dto.Type,
      CreatedAt = dto.CreatedAt,
      IsRead = false,
      IsDelivered = true,
      IsDeleted = false
  };

  ...
}
```

#### Session contracts

Session tracking is split in **2** responsibilities:

**Identity state management**:

```csharp
 internal interface ISessionContext {
    UserId? CurrentUserId { get; }

    string? SessionToken { get; }

    bool IsAuthenticated { get; }

    void SetSession(UserId userId, string sessionToken);

    void Clear();
}

```

Serves as a state container storing:

 - Who is currently logged in

 - What is their authentication token

This is injected into `AuthTokenHandler` to automatically append Bearer tokens to outgoing network requests and also used in services to verify authentication before executing operations.

**DI-related related memory management**

```csharp

internal interface ISessionScopeService {

    IServiceProvider? CurrScope { get; }

    IServiceProvider BeginSession();

    void EndSession();
}

```

`ISessionScopeService` manages the lifetime of all scoped dependencies in the application. It isolates and cleans up user resources in RAM when logging in and logging out.

`BeginSession()` opens a new `IServiceScope` from the DI container. Any pre-existing session scope is firstly disposed, to prevent resource leakage.

`CurrScope` Exposes the `IServiceProvider` tied to the active user`s session. Parametried ViewModels us this provider to resolve session-scoped repositories and services.

`EndSession()` Terminates the active `IServiceScope`, disposing all `Scoped` services created within it - closing database connections, destroying session ViewModels and cleaning out decrypted cryptographic keys in `ClientKeyManager`.

These 2 interfaces cooperate:

 - On login in `AuthService`:
    - `ISessionScopeService.BeginSession()` is called to create a fresh DI scope boundary

    - `ClientManager` resolved from the new scope unlocks the private key into memory

    - `ISessionContext.SetSession(userId, token)` is called to store the active session ccredentials globally

 - During active session:
    - Services check `ISessionContext.IsAuthenticated` to ensure requests are valid

    - `AuthTokenHandler` reads ``ISessionContext.SessionToken` to authenticate Refit API calls

    - UI components resolve session-bound ViewModels through `ISessionScopeService.CurrScope`

 - On logout in `AuthService`:
    - `ISessionContext.Clear()` wipes the global token and user ID
    
    - `ISessionScopeService.EndSession()` disposes the underlying `IServiceScope` which wipes the user data and unlocked private keys from RAM

#### Database

Provides the local persistance schema and database profile management abstractions. It connects the domain model with EF Core and SQLite storage.

**ChatSystemLocalDbContext**

This class represents a context for an object-relational mapping (ORM). It acts as the client-side storage angine and defines all local tables and configures relational rules inside `OnModelCreating()`:

 - Entity storage manages cached tables (`DbSet<T>`) for user identities, chat rooms, participant links, message history and private key material.

 - SQLite does not natively understand our strongly typed ID objects or .NET's `DateTimeOffset`. This class handles their conversions (Ids to Guid, `DateTimeOffset` to `UtcTicks`). Converting the `DateTimeOffset` this way enables chronological sorting.

 - Enforces primary keys and sets up foreign key relationship cascades so deleting a chat room automatically cleans up local participant links and message logs.

 - Configures specific database indices to make some query patters faster and more effective. This concerns sorting of chat lists and messages and lookups of other users.

**ProfilePathProvider**

This class abstracts the local database file from its resolution on disk.

 - `SetActiveProfile(string login)` allows dynamically switching the active profile path

 - `GetDatabasePath()` passes the active profile path directly into the EF Core `DbContext` options, ensuring that different users logging in on the same device keep their local caches separated.

#### Repositories

Repository interfaces present business contracts which abstract SQLite/EF Core persistance. These contracts are asynchronous and are used to manage offline-first entities cached in the local database

 - `ILocalChatRepository`: Manages persistance for chat rooms and their participation links including room updated, participant role assignment and deletions.

 - `ILocalIdentityRepository`: Manages the persistent active user session identity, handling the storage, updates, and clearing of the logged-in user profile and session token.

 - `ILocalMessageRepository`: Handles persistance for decrypted local messages, providing methods for maginated history fetching, read-state updates and client-side text searching.

 - `ILocalUserRepository`: Caches public user profiles and their RSA public keys locally, allowing fast participant lookups without repeated network requests.

#### Services

Service interfaces present contracts for orchestrating business logic across network APIs, local storage and cryptographic frameworks.

 - **IAuthService**: Orchestrates user authentication flows. it coordinates registering new accounts, deriving and unlocking identity keys on login, initializing local database profiles and disposing active session states on logout

 - **IChatService**: Handles fetching active user conversation lists, retrieving participants, and creating new chats.

 - **IMessageService**: Handles the E2EE messaging pipeline. It generates session keys, encrypts and wraps outgoing messages, unwraps and decrypts incoming messages and executes local plaintext searches.

 - **IUserService**: Handles global user discovery and public key resolution.

#### Presentation contracts

```csharp

internal interface INavigationService {
    public TViewModel NavigateTo<TViewModel>() where TViewModel : ViewModelBase;
}

```

`INavigationService` manages the ViewModel navigation while keeping the presentation logic decoupled from specific Avalonia UI views.

**Key Mechanisms**

 - navigation is driven entirely through ViewModels. No need to reference Avalonia controls

 - using the `where TViewModel : ViewModelBase` clause prevents from accidental attempt to navigate to an invalid type or raw domain object

 - with correct integration with the DI container, individual ViewModels are resolved directly from the DI container. That guarenteed that all their required services, repositories and all  scoped dependencies are automatically injected upon navigation

## Infrastructure

Provides the concrete implementations for the contracts proposed in **Core**.

### Cryptography

We will present the key implementation details worth mentioning for this layer:

**AesRsaEncryptionService**

 - For authenticated encryption, it uses .NET's `AesGcm` class with a 256-bit key, 12-byte nonce and 16-byte authentication tag. The service 

 - For custom binary packing, the server packs the nonce, tag and ciphertext into a single unified byte array - `[Nonce (12B)] + [Tag (16B)] + [Ciphertext]`. On decryption, `DecryptSymmetric` unpacks these offsets via `Buffer.BlockCopy`

**Argon2KeyDerivationService**

 - Wraps the `Konscious.Security.Cryptography.Argon2id` to provide memory-hard key derivation

 - Uses `RandomNumberGenerator.GetBytes(16)` to generate cryptographically secure salts

 - Hardcodes balanced defaults (19,456 KiB RAM, 2 iterations, 1 thread) to balance GPU brute-force resistance with desktop login responsiveness

**ClientKeyManager**

 - Uses `CryptographicOperations.ZeroMemory` to prevent the key material from persisting in RAM. MEKs are explicitly zeroed inside `finally` blocks immediately after their use.

 - Unlocked private keys are stored as raw byte arrays. When calling `LockPrivateKey()`, upon session end, the memory is zeroed before dropping the reference.


**EFKeyStore**

 - Implements the `IKeyStore` interface using EF core.

 - Enforcing `KeyId = 1`, a single key is stored stored on a device for each user

 - When saving new key material, `ExecuteDeleteAsync()` clears any old entries before inserting the new record, guaranteeing that the local database stores in a single identity storage. 

#### Execution Flow Example (Session Unlock and Message Transmission)

#### 1. Session Unlock

- User logs in using password credentials
- `ClientKeyManager` forwards password to `Argon2KeyDerivationService`
- The service computes the Master Encryption Key (MEK)

#### 2. Vault Decryption

- `ClientKeyManager` retrieves encrypted private key from `EFKeyStore`
- The MEK decrypts the private key into volatile memory
- The key remains accessible only during the active session

#### 3. Symmetric Streaming

- Upon sends a message
- `AesRsaEncryptionService` generates single-use AES session key
- Plaintext payload is symmetrically encrypted

#### 4. Key Wrapping

- AES session key is asymmetrically wrapped multiple times
- One encrypted session key is generated for all participants
- Wrapping uses participant public keys via:
  - `_crypto.WrapKey(sessionKey, publicKey)`

#### 5. Payload Packing

The client constructs a final payload containing:

- A single universal ciphertext
- A mapping:
  - `participant_id → encrypted_session_key`

This armored payload is then passed to the Refit Neetworking layer for network transmission.

### Database

Implements `IProfilePathProvider`, which manages physical SQLite database on disk per user.

#### Key details:

 - Uses `Environment.SpecialFolder.LocalApplicationData` to build cross-platform paths according to OS standards (e.g., %LOCALAPPDATA% on Windows, ~/.local/share on Linux)

 - Enforces multi-profile disk isolation using the following directory structure:

```bash
Path = LocalAppData/ChatSystem/Profiles/$_activLogin/chatsystem.db
```

 - `GetBuiltProfileDirPath()` throws an `InvalidOperationException` if an it is attempted to acces the database before a profile is bound via `SetActiveProfile()`, preventing accidental acces to unassigned default paths.

 - Automatically creates the physical folder structure on a disk, when EF Core requests the path in `GetDatabasePath()`, so it is not necessary to manually setup the file-system.

 - Provides `ClearProfile()` method to set `_activeLogin` back to `null` upon logout which ensures that the path provider cannot leak user contexts across logins


### Networking

This layer serves the translation and serialization for domain entities which adapts to the server behavior. It also ensures that te network serialization mechanics do not leak into business service or local storage logic.

**UnixSecondsDateTimeOffsetConverter**

The C++ server serializes timestamps as raw 64-bit Unix epoch epoch seconds (`std::chrono::seconds`). This class intercepts JSON reads and writes during Refit's or `System.Text.Json`'s operations. It converts epoch integer numbers into .NET's `DateTimeOffset` instances, eliminating manual timestamp parsing across the application.

**ResponseMappers/**

Network DTOs represent wire-contract shapes, whereas `CachedChat` and `CachedUser` represent domain/database entities. Static mapper objects like `ChatMapper` and `UserMapper` transfrom network payloads into corresponding domain objects. While doing so, they also enrich the records with `CachedAt` timestamp and `IsDeleted = false` soft deletion flag.

### Presentation

This layer connects domain/session logic with Avalonia's UI rendering mechanics and ViewModel-first navigation.

**NavigationService**

- Resolves targeted ViewModels based on the current scope. When a user is logged in, ViewModels are resolved from active session scope (injecting session-bound repositories and decrypted state). Before login, it falls back to the root container for public/global ViewModels like `LoginViewModel`.

- Resolves `MainWindowViewModel` from the root container and updates its `CurrentViewModel` property. This manages Avalonia's top-level `ContentControl` view-swapping mechanism without referencing UI controls.

**MessageBubbleAlignmentConverter**

Implements Avalonia's `IMultiValueConverter` to convert raw domain IDs into UI layout instructions during binding evaluation. C#'s pattern matching feature is used here:

```csharp
values is [UserId senderId, UserId currentUserId]
```

to smoothly extracting and resolving IDs of both chat participants. After that, it horizontally aligns message bubbles; outgoing messages to the right, incoming to the left.


### Repositories

Repositories realize the propose contracts by managing EF Core SQLite operations. All repositories basically wrap `ChatSystemLocalDbContext` based on their specific assigned role.

Methods like `DeleteAsync())`, `MarkAsReadAsync()` or `ClearAsync()` utilize `ExecuteDeleteAsync()` and `ExecuteUpdateAsync()`. That execute direct SQL queries against SQLite.

Methods like `SaveForChatAsync()` and `UpsertAsync()` use `FindAsync()`. When `DbUpdateException` occurs, `_dbContext.ChangeTracker.Clear()` is invoked inside the catch block to prevent tracked entity state corruption.

`LocalIdentityRepository` enforces single-row assertion by explicitly checking a count of affected entities, throwing an exception if multiple or zero rows were altered.

`LocalMessageRepository` performs `PlainText.ToLower().Contains(...)` queries directly against the local SQLite database. Because messages are stored decrypted locally, this enables full-text message search without compromisign E2EE on the server.

### Services

Services exucute business workflows across network APIs, local repositories, cryptographic engines and session scopes.

**AuthService**

 - Performs profile bootstrapping; in `RegisterAsync` and `LoginAsync`, it binds the user profile path - `_profilePathProvider.SetActiveProfile(login)` - opens a new DI session scope - `_sessionScopeService.BeginSession()` - and calls `EnsureCreatedAsync()` to provision user's isolated SQLite database file.

 - Protects cryptographic identity using `IClientKeyManager` to derive master keys, encrypting raw private keys on disk on registration or unlocking them into RAM on login.

 - Manages session destruction on logout by notifying the backend, locking and zeroing out the private key memory, clearing the local identity database table, wiping global `ISessionContext` and disposing the DI container scope tied to the session.

**MessageService**

Executes E2EE pipelines.

Outgoing pipeline:

1. Generates a fresh, single-use 256-bit AES symmetric key
2. Encrypts plaintext message payload into base64 ciphertext via AES-GCM.
3. Queries participant public keys via `IUserService` and wraps/encrypts the AES key using RSA-OAEP for every chat recipient.
4. Posts the encrypted payload to `IMessageAPI` and saves the decrypted plaintext locally in SQLite.

Incoming pipeline:

1. Fetches the wrapped AES session key for the message via `IMessageApi.getEncryptedKeyAsync()`.
2. Unwraps the AES key using the active user's private key retrieved from `IClientKeyManager`.
3. Symmetrically decrypts ciphertext into `CachedMessage.PlainText`property and persists the record to the local repository.

`GetUndeliveredAsync()` performs the background catch-up by pulling unread server messages delivered while offline and passes them through the `DecryptAndPersistAsync()` pipeline

**ChatService and UserService**

These services serve the cache-aside strategy. First, the local cache is read via repositories API, and then, on cache miss, Refit APIs are queried. Incoming DTOs comming from the server are converted into domain entity classes using the response mappers and automatically upsert into local repositories for a subsequent offline use.

### Session

This layer implements the session management workflow mechanisms, splitting identity state storage from the technical container scope lifecycle.

**SessionContext**

This class serves as a global identity state container holding the active `CurrentUserId` and `SessionToken` with private setters to prevent unauthorized external state mutation

The `IsAuthenticated` handles the guard logic by dynamically validating instantiation of the current user and a session token.

`Clear()` sets both backing properties back to `null` on logout, which ensures that the identity token do not leak across user logins.

**SessionScopeService**

Integrates the root service container; accepts `IServiceProvider` which intends to be a singleton dependency, using it as a factory for session-based sub-scopes.

In `BeginSession()` and `EndSession()`, calling `_sessionScope?.Dispose()` triggers cascade disposal of all `Scoped` dependencies within the active user session. This deterministically closes open EF Core SQLite database handles, destroys session-specific ViewModels, calls memory-zeroing cleanups on services like `ClientKeyManager`

`EndSession()` explicitly sets `_sessionScope` to `null`, guaranteeing that any subsequent hecks on `CurrScope` also return `null` when no user is logged in.

### Presentation

Implements the UI layer of the application, utilizing MVVM pattern.

This layer heavily leverages .NET's `CommunityToolkit.Mvvm` source generators:

 - `[ObservableProperty]` automatically generates boilerplate `INotifyPropertyChanged` code, its backing fields and `On<property>Changed` partial methods.

 - `[RelayCommand]` generates `ICommand` instances for UI bindings, automatically wiring up validation guards via `CanExecute` methods (e.g. preventing message submission when `MessageText` is whitespace or `IsBusy` is `true`)

#### ViewModels

ViewModels serve as a mediator between Avalonia XAML views and underlying core business services. That includes isolating UI presentation logic, managing asynchronous loading states, enorcing user input validation rules or controling master-detail view swapping without introducing dependencies on specific Avalonia UI controls.

**MainWindowViewModel**

The single top-level container holding `CurrentViewModel`, enabling scree switches between authentication views and active messaging views.

**ChatShellViewModel**

Orchestrates the post-login two-pane layout. It keeps `ChatListViewModel` static in the left sidebar, while dynamically swapping `CurrDetailPane` on the right between `ChatViewModel`, `UserSearchViewModel` or `EmptyViewModel`.

ViewModels like `ChatViewModel` and `UserSearchViewModel` cannot be safely instantiated from root DI container. Thus, they are resolved from `_sessionScopeService.CurrScope` for **3** reasons:

1. **Binding to the active user's isolated SQLite database**

Some services injected into ViewModels depend on `ChatSustemLocalDbContext`. `IMessageService` or `ILocalMessageRepository` are an examples of this. Because `ChatSystemLocalDbContext` is registered with `Scoped` lifetime, resolving a ViewModel from `CurrProvider` guarantees that all its underlying repositories query the **active user's isolated SQLite database file** on disk.

2. **Access to in-memory cryptographic state**

To send or display decrypted messages, `ChatViewModel` relies on `IMessageService`, which relies on `IClientKeyManager`, which holds the user's unlocked RSA private key in RAM. This state only exists within the active `IServiceScope` created upon login. Resolving ViewModels outside this scope would attempt cryptographic operations without an access to the unlocked key.

3. **Deterministic memory and lifecycle cleanup**

When a user logs out, `AuthService` calls `_sessionScopeService.EndSession()`. Disposing the `IServiceScope` automatically destroys all ViewModels and session-scoped services within it. (e.g. closing database conncetions, zeroing out private keys in RAM and preventing memory leaks across user sessions)

**How are chat rooms tied to the logged-in user?**

1. **Authentication**

When `IChatApi.GetChatsAsync()` is invoked, `AuthTokenHandler` attaches the user's active bearer token (`SessionTOken` from `ISessionContext`). The server decodes this token to identify the user and strictly returns only the chat records where that user is an authorize participant.

2. **Disk storage boundary**

Upon login, `ProfilePathProvider.SetActiveProfile(login)` routes all EF Core queries to a dedicated SQLite database file. A user's local chats are physically separated on disk from any other user profiles on the same device.

3. **Database schema boundary**

Inside `ChatSystemLocalDbContext`, chats and users are joined via `CachedParticipant` entity by:
 - composite primary key `{ UserId, ChatId }`
 - a chat room is relationally tied to a user by maintaining an explicit participant record linking `CurrentUserId` to `ChatId`

4. **Cryptographic Boundary**

Even if an unauthorized user obtained access to the raw SQLite database file, message payloads remain protected by E2EE:
 - every message is encrypted with a unique single-use AES key, which is wrapped using the public keys of individual chat participants
 
 - a chat room's history is unreadable to anyone except the user who holds the matching RSA private key unlocked in their session memory.

This ViewModel uses events to manage child pane state without hard coupling:

 - Wires up communication bridges between child ViewModels by attaching event handlers:
    - `SidebarPane.ChatSelected` triggers when a user clicks a chat room item in the sidebar. It disposes the currently active detail pane, instantiates a new `ChatViewModel` for the selected `ChachedChat` using session DI, wires up its "Close" action and assigns it to the `CurrDetailPane` property. This decouples `ChatListViewModel` from detail view creation. The sidebar simply reports which chat was clicked, delegating the responsibility of view resolution, session context passing and pane swapping entirely to `ChatShellViewModel`

    - `searchVm.SearchCancelled` triggers when the user clicks the "Cancel" button inside the search view. It disposes the `UserSearchViewModel` instance and resets `CurrDetailPane` property back to `EmptyViewModel`. This allows `UserSearchViewModel` to signal that the user exited the search without needing a direct reference to the parent shell or navigation container. Calling `.Dispose()` on event firing ensures memory and reactive bindings are freed immediately.

    - `searchVm.ChatStarted` triggers when a user initiates a conversation with a search result. It disposes `UserSearchViewModel`, reloads `SidebarPane` so the new conversation appears in the list, resolves a new `ChatViewModel` for that `ChatId` and displays it in `CurrDetailPane`. This keeps the search execution separated from navigation and sidebar state. `UserSearchViewModel` only needs to know how to create the chat on the server and emit its `ChatId`. `ChatShellViewModel` then manages the multi-pane UI transition and the sidebar re-synchronization.

    - `chatVm.CloseRequested` triggers when the user clicks the "Close" button inside an active chat header. It disposes the active `ChatViewModel` instance and replaces `CurrDetailPane` with `EmptyViewModel`. This satisifies the Single Responsibility Principle by allowing `ChatViewModel` to expose a close action without requiring a reference to `ChatShellViewModel`. The parent shell listens for the request, destroys the chat instance and returns the detail pane to `EmptyViewModel`

 - Uses `ActivatorUtilities.CreateInstance<TViewModel>(CurrProvider, ...)` to resolve detail ViewModels using the active `IServiceProvider` session scope (from `ISessionScopeService`), which ensures that the injected dependencies are tied strictly to the active logged-in user.

Because ViewModelBase implements the `IDisposable` interface, henever `ChatShellViewModel` swaps a detail pane, it explicitly calls `.Dispose()` on the outgoing ViewModel, clearing bound collections (like `ChatViewModel.Messages`), unsubscribing from the event handlers, and freeing memory.

**LoginViewModel and RegistrationViewModel**

Manage credential forms. real-time input validation (for successful password repeating), loading spinners, and error/success flash messages.

**ChatListViewModel and DirectMessageChatListItemViewModel**

Manage left sidebar items. Individual item ViewModels dynamically derive display names for direct messages by loading participant profiles.

`DirectMessageChatListItemViewModel` instantiates with a temporary "Loading..." state. `InitializeAsync()` then runs asynchronously to resolve participant user profiles and populate actual user logins without stalling the UI rendering.

**ChatViewModel**

Controls an active conversation panel. It loads message history in chronological order, binds message entry inputs, and dispatches encrypted "Send Message" commands.

This viewmodel loads updates message history asynchronously in the background and subsequently updates the UI thread using the Avalonia's dispather:

```csharp
EnqueActionToUIThread(() => Messages.Add(sentMessage));
```

This prevents race conditions on `ObservableCollection<T>` instances when a network background threads complete E2EE decryption tasks.

**UserSearchViewModel**

Allows searching global user directories and initiating new E2EE conversations.

#### Views

In Avalonia, Views and ViewModels are connected through **Data Binding** and **DataTemplates (View Locators)**.

**How the connection works?**

 - `DataContext` and `ContentControl`: When a ViewModel instance is assigned to a container's content property (such as <ContentControl Content="{Binding CurrentViewModel}"/>), Avalonia searches its template rules for a View mapped to that specific ViewModel type. It automatically instantiates the matching View and sets its `DataContext` to the ViewModel instance

 - `x:DataType`: Each `.axaml` file specifies its expected ViewModel using `x:DataType="vm:TargetViewModel"` and `x:CompileBindings="True"`. XAML compiler than validates property names at compile time, providing type safety, IDE autocomplete and biding execution without runtime reflection overhead

**.axaml files**

The **.axaml** (Avalonia XAML) files contain declarative layout markup. It defines visual structures, component layouts, colors, fonts, etc. They do not contain any application logic.

**.axaml.cs files**

These files represent individual partial classes linked to the XAML file. In MVVM, these files are intentionally kep minimal, usually containing only `InitializeComponent()` to load the XAML code at runtime - delegating all state, command execution and presentation logic to the ViewModels.

**MainWindow**

 - Primary desktop window shell hosting the entire UI.

 - Responsibilities:
    - Configures top-level desktop window parameters
    - Contains a single `<ContentControl Content="{Binding CurrentViewModel}"/>`
    - When a `CurrentViewModel` in `MainWindowViewModel` switches, Avalonia automatically swaps the top-level view inside the main window.

**LoginView**

 - The user authentication and sign-in screen.
 - Responsibilities:
    - Uses a 3-row `Grid` to keep the login card centered on screen regardless of window resizing
    - Uses `PasswordChar="•"` to hide sensitive password input
    - Binds `IsEnabled="{Binding !IsBusy}"` across input fields to prevent double submissions while authentication network requests are being executed.
    - Displays conditional succes text or error messages bound to `IsVisible="{Binding HasSuccess}" / IsVisible="{Binding HasError}"`
    - Provides a `<HyperlinkButton>` bound to `RequestRegistrationNavigationCommand` to switch to account creation.

**RegistrationView**

 - The user account registration screen.

 - Responsibilities:
    - Defines visual class styles in `<UserControl.Styles>` for gren borders of the textbox when the passwords match, red borders when they don't. Avalonia's dynamic class bindings(`Classes.match="{Binding IsPasswordsMatch}"` and `Classes.mismatch="{Binding IsPasswordsMismatch}"`) to orchestrate thi feature.
    - The confirm-password field's enabled state is driven by `IsEnabled="{Binding IsPassword2Enabled}"`, preventing confirmation text entry until the initial password is provided.

**ChatShellView**

 - The overarching post-login master-detail layout.

 - Responsibilities:
    - Uses a 5-Column Split Grid to divide the UI into a fixed sidebar, separator lines, a flexible detail area and an action strip.

    - Displays user profile information in the header (`SidebarPane.HeaderTitle`) and hosts the conversation list via <ContentControl Content="{Binding SidebarPane}"/>

    - A right-hand vertical bar occupied by buttons for user search and account logout buttons bound to `OpenSearchCommand` and `LogoutCommand` respectively.

**ChatListView**

 - The sidebar panel rendering active direct message conversations.

 - Responsibilities:
    - Uses a `<ListBox>` bound to `Chats`(`ObservableCollection<DirectMessageChatListItemViewModel>`).
    
    - Binding `SelectedItem="{Binding SelectedChat}"` notifies the ViewModel when a user clicks a conversation item.

    - The `DataTemplate` displays `DisplayName` with `TextTrimming="CharacterEllipsis"` to prevent long contact names from breaking sidebar layout bounds.

**ChatView**

 - The active E2EE chat room panel

 - Responsibilities:
    - Employs the `MessageBubbleAlignmentConverter` within a `<MultiBinding>`, dynamucally aligning outgoing messages to the right and incoming messages to the left:

    ```xml
    <MultiBinding Converter="{StaticResource MessageBubbleAlignmentConverter}">
        <Binding Path="SenderId" />
        <Binding Path="$parent[UserControl].((vm:ChatViewModel)DataContext).CurrentUserId" />
    </MultiBinding>
    ```

    - Overrides default `ListBoxItem` templates to make background transparent on hover/selection, which creates a smooth chat stream effect.

    - Wraps decrypted message text inside rounded borders (`CornerRadius="12"`) with right-aligned timestamp strings (`StringFormat='HH:mm'`)

    - Contains a multi-line capable `TextBox` (`MaxLines="4"`), a "Send" button bound to `SendMessageCommand` and line error reporting.

**UserSearchView**

 - The user discovery view for finding contacts and creating the new chat rooms.

 - Responsibilities:
    - Top control row combining a query `TextBox`, a "Search" submit button and "Cancel" button cound to `CancelCommand`.

    - Configures `<Style Selector="ListBoxItem:pointerover ...">` to highlight result rows and display a hand cursor `Cursor="Hand"`.

    - Renders search results inside rounded, semi-transparent card borders.

    - Selecting a user row via `SelectedItem="{Binding SelectedUser}"` immediately triggers chat room creation on the backend.

**EmptyView**

 - The default placeholder panel when no chat or search view is active.

 - Roles:
    - Minimalist view containing a single centered `TextBlock` containing (""Select a chat to start messaging") to fill empty master-detail panes cleanly.


## Tests

Test layout mirrors the application's architecture, which makes the tests easily discoverable and aligned.

 - `Core/`: Contains pure unit tests focused on validating domain logic and basic utilities, focused on strongly-typed DTOs (ChatId, UserId, MessageId)
 and `IdFactory` to serialize and validate them correctly.

 - `Infrastructure/`: Contains a mix of unit and integration tests for the concrete implementations:
    - `Cryptography/`: Validates the encryption mechanisms, key derivation behavior and state management within the `ClientKeyManager`.
    - `Repositories/`: Verifies EF Core interactions with local SQLite databases
    - `RepositoryTestBase.cs`: Provides a clean in-memory SQLite database for repository each test, with automatic schema creation and cleanup.

## Extensibility and Future Work

While the current state of the application provides a fully operational comunication, there still is a plenty of space for extending
its functionalities.

### UI/UX

The current UI serves as a bare minimum, covering only the necessary features. The main areas of improvement to enhance UX include increasing the
color palette, including more windows for application settings or key binding setup for common actipons (e.g. enter - ending a message).

### Websocket Integration

Current synchronization primarily relies on HTTP request polling. The next architectural milestone is completing the real-time event pipeline. This
requires dedicated work on the server as well.

### Group chats

The current database schema is already ready for this.

### Sending other media types

### Expanding the user role palette

Also ready within the current database schema.

### Extending the tests

Although the application is functional, a large portion of services and presentation layers remains untested, possibly hiding some serious bugs.
