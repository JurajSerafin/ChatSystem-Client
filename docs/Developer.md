# ChatSystem Client Developer Documentation

## Architecture overview

The code is split into **4** main parts:

 - **Config** (Server setup, DI Registration)

 - **Core** (Interfaces, Database business logic)

 - **Infrastructure** (Implementation)

 - **Presentation**

## Used Libraries

| Libraries | Purpose |
|-----------|----------|
|**Avalonia**| GUI |
| **SQLite**, **Entity Framework** | Local Cache |
|**Refit**	| Networking (API contracts, REST calls, serialization, etc.) |
|**System.Security.Cryptography**| AES and RSA cryptographic algorithms |
| **Konscious.Security.Cryptography.Argon2**| Password-based **MEK** key derivation |

## Config

Contains a setup of external settings and internal dependency injection container.

The DI container manages creation, lifetime and injection of application's dependencies. In `ServiceCollectionExtensions.cs`,
individual interfaces are mapped to their concrete implementations. All instantiation is handled by .NET based on selected
service lifetime (Transient, Scoped, Singleton). Read [more:](https://learn.microsoft.com/en-us/dotnet/core/extensions/dependency-injection/overview)

## Core

Defines the architectureal rules, contracts, and domain models:

 - **Domain Models:** business entities for chats, messages, users and their strongly typed identifiers
 
 - **Interfaces and Abstractions:** contracts for all business logic repositories and services

 - **Networking Contracts:** definitions of REST endpoints, DTOs for commutication with the server

 - **Cryptography Specifications:** Abstractions for E2EE, symmetric key generation, secure key management

 - **Session and Presentation:** interfaces for tracking user session state and view-to-view navigation logic

### Infrastructure

Provides the concrete implementations for the contracts proposed in **Core**.

**Breakdown:**

 - **Cryptography:** Security mechanisms including AES symmetric encryption, RSA wrappers, Argon2 key derivation
 and secure key storage.

 - **Database:** Helpers for database setup; resolving file paths for user-specific SQLite storage.
    - Local database stores recently viewed chats(including their participants) and locally wrapped cryptographic keys
    - Utilizes Cache-Aside pattern

 - **Networking:** Response mappers translating raw API DTOs into the domain classes, JSON serialization converters.

 - **Presentation:** UI-related utilities - XAML converters for message bubbles, view-to-view navigation

 - **Repositories:** Cache and query chats, messages, users, and identity.
 data utilizing EF Core/SQLite.

 - **Services:** Business logic implementations executing whole pipelines for authentication, E2EE operations, server
 communication, etc..

 - **Session:** Session tracking for the authenticated users and object lifetime scope management tied to it.

### Execution Flow Example (Session Unlock and Message Transmission)

### 1. Session Unlock

- User logs in using password credentials
- `ClientKeyManager` forwards password to `Argon2KeyDerivationService`
- The service computes the Master Encryption Key (MEK)

### 2. Vault Decryption

- `ClientKeyManager` retrieves encrypted private key from `EFKeyStore`
- The MEK decrypts the private key into volatile memory
- The key remains accessible only during the active session

### 3. Symmetric Streaming

- Upon sends a message
- `AesRsaEncryptionService` generates single-use AES session key
- Plaintext payload is symmetrically encrypted

### 4. Key Wrapping

- AES session key is asymmetrically wrapped multiple times
- One encrypted session key is generated for all participants
- Wrapping uses participant public keys via:
  - `_crypto.WrapKey(sessionKey, publicKey)`

### 5. Payload Packing

The client constructs a final payload containing:

- A single universal ciphertext
- A mapping:
  - `participant_id → encrypted_session_key`

This armored payload is then passed to the Refit Neetworking layer for network transmission.

### Presentation

Implements the UI layer of the application, utilizing MVVM pattern.

#### Breakdown

**App Shell and Placeholders**

 - `MainWindow`: Root application window that hosts the primary view and manages the main window lifecycle.

 - `Empty`: A placeholder for windows when no active content is displayed.

**Authentication**

 - `Registration`: Handles new account creation, user validation and transitions upon successful registration.

 - `Login`: Manages credential input, authentication against the server and related session lifecycle.
 
**Main Chat Experience**

 - `ChatShell`: Dashboard container coordinating content in individual panes, sidebar navigation, header state and
 transitioning actions like searching a user and logging out.

 - `ChatList and DirectMessageChatListItem`: Manage the sidebar list of active direct message chats and chat selection.

 - `Chat`: Manages the conversation window(message history fetching and rendering, sending new messages).
 
 - `UserSearch`: Handles searching of other users on the network and eventual opening of direct message chats with them.

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
