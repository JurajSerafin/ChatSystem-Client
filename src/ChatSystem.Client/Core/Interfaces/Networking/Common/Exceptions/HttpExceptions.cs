using System;

namespace ChatSystem.Client.Core.Interfaces.Networking.Common.Exceptions;

/// <summary>
/// Defines an exception type associated with a specific HTTP status code.
///
/// Implementing exceptions expose the HTTP status code they represent through
/// a static factory-like member, enabling generic handling based on the exception type.
/// </summary>
/// <typeparam name="TSelf"> The implementing exception type.</typeparam>
internal interface IHttpException<TSelf>
    where TSelf : Exception, IHttpException<TSelf> {
    /// <summary>
    /// Gets the HTTP status code represented by this exception type.
    /// </summary>
    /// <returns>The corresponding HTTP status code.</returns>
    static abstract int StatusCode();
}

/// <summary>
/// Exception thrown when the server returns an HTTP 400 Bad Request status.
/// Indicates that the client sent a malformed or otherwise invalid request.
/// </summary>
internal class BadRequestException : Exception, IHttpException<BadRequestException> {
    public BadRequestException() { }

    public BadRequestException(string message) : base(message) { }

    public BadRequestException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <inheritdoc />
    public static int StatusCode() => 400;
}

/// <summary>
/// Exception thrown when the server returns an HTTP 401 Unauthorized status.
/// Indicates missing, invalid, or expired authentication credentials.
/// </summary>
internal class UnauthorizedException : Exception, IHttpException<UnauthorizedException> {
    public UnauthorizedException() { }

    public UnauthorizedException(string message) : base(message) { }

    public UnauthorizedException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <inheritdoc />
    public static int StatusCode() => 401;
}

/// <summary>
/// Exception thrown when the server returns an HTTP 404 Not Found status.
/// Indicates the requested resource does not exist.
/// </summary>
internal class NotFoundException : Exception, IHttpException<NotFoundException> {
    public NotFoundException() { }

    public NotFoundException(string message) : base(message) { }

    public NotFoundException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <inheritdoc />
    public static int StatusCode() => 404;
}

/// <summary>
/// Exception thrown when the server returns an HTTP 422 Validation Error.
/// Indicates the client sent invalid request data that failed validation.
/// </summary>
internal class ValidationException : Exception, IHttpException<ValidationException> {
    public ValidationException() { }

    public ValidationException(string message) : base(message) { }

    public ValidationException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <inheritdoc />
    public static int StatusCode() => 422;
}

/// <summary>
/// Exception thrown when the server returns an HTTP 500 Internal Server Error
/// status. Indicates an unexpected failure on the server.
/// </summary>
internal class ServerException : Exception, IHttpException<ServerException> {
    public ServerException() { }

    public ServerException(string message) : base(message) { }

    public ServerException(string message, Exception innerException)
        : base(message, innerException) { }

    /// <inheritdoc />
    public static int StatusCode() => 500;
}