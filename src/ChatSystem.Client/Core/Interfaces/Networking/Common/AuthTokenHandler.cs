using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using ChatSystem.Client.Core.Interfaces.Session;

namespace ChatSystem.Client.Infrastructure.Networking;

/// <summary>
/// HTTP delegating handler that automatically attaches the current session
/// token as a Bearer authorization header on every outgoing request.
/// 
/// </summary>
internal sealed class AuthTokenHandler : DelegatingHandler {
    private readonly ISessionContext _session;

    private const string authHeader = "Bearer";
    public AuthTokenHandler(ISessionContext session) {
        _session = session;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken
    ) {
        if (!string.IsNullOrEmpty(_session.SessionToken)) {
            request.Headers.Authorization = new AuthenticationHeaderValue(
                authHeader,
                _session.SessionToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}