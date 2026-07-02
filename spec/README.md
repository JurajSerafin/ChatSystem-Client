# Specification of the final project for relevant C# courses

## C# Courses selection
- [x] NPRG035 (Programming in C# language | Programování v jazyce C#)
- [x] NPRG038 (Advanced C# Programming | Pokročilé programování v jazyce C#)
- [x] NPRG057 (Advanced .NET Programming II | Pokročilé programování pro .NET II)
- [x] NPRG064 (Programming user interfaces in .NET | Programování uživatelských rozhraní v .NET)

## Specification

### ChatSystem Chat Application Frontend

Frontend for a chatting application with end-to-end encryption (E2EE)


#### Motivation
---
The goal is to develop a desktop chatting application used for general-purpose secure communication. The intention is to make the application simple to understand and use, while also versatile in terms of usage (types of users, messages, etc.) and code (abstraction, maintainability, extensibility, etc.).


#### Main Features
---
Besides already mentioned E2EE message encryption, the application will also perform local caching of recent/frequently-used data such as encryption keys or newest messages.

The application's server is already written in C++ and ready to use. The frontend will communicate with it using a REST API.

From a high-level point of view, the application will handle:
	- creating user accounts
	- sending direct text messages

That being said, the system should be extensible by supporting new message types, implementing group chats, and defining new user roles and allowed actions within the application.


#### UI/UX:
---
The application will use a GUI implemented by MVVM pattern. The user's initial interaction will be with a login/registration window. After a successful logging in, the user will enter a dashboard-like window, where they can browse through already existing chats, or search for other users using a dedicated search bar.


#### Persistence
---
The application will utilize the **SQLite** engine for local cache purposes, using cache-aside pattern. The cache will store encryption keys an recent messages, including their associated chat and sender information.


---
#### Networking
---
The application will communicate with the server using a REST API.


#### Libraries
---
| Libraries | Purpose |
|-----------|----------|
|**Avalonia**, **ReactiveUI**| GUI |
| **SQLite**, **Entity Framework** | Local Cache |
|**Refit**	| Networking (API contracts, REST calls, serialization, etc.) |
|**System.Security.Cryptography**| AES and RSA cryptographic algorithms |
| **Konscious.Security.Cryptography.Argon2**| Password-based **MEK** key derivation |


#### Testing
---
The application will utilize unit testing of all important components and integration testing for individual use cases (logging in, sending a message, etc.)
using the **xUnit** framework.