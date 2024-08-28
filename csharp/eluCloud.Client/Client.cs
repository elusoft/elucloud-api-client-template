using elusoft.eluCloud.Client.Model;
using elusoft.eluCloud.Client.ServerPushes;
using ServiceStack;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace elusoft.eluCloud.Client;

public sealed class Client
{
    private IServerPushesHub? signalRHub;
    private readonly JsonServiceClient jsonClient;
    private readonly List<ServerPushHandler> serverPushHandlers = new();
    private string vendor;
    private string apiKey;
    private CancellationToken token;

    public string? SessionId 
    { 
        get => jsonClient.SessionId; 
        private set 
        { 
            jsonClient.SessionId = value;
            if (null == value)
            {
                jsonClient.Headers.Remove("X-ss-id");
            }
            else
            {
                jsonClient.AddHeader("X-ss-id", value);
            }
        } 
    }
    public string? SignalRConnectionId => signalRHub?.ConnectionId;
    public string? ApiVersion { get; private set; }
    public string? ServerVersion { get; private set; }

    public bool IsServerPushAvailable => signalRHub != null;

    public Client(string baseUrl, string user, string apiKey, CancellationToken token = default)
    {
        jsonClient = new JsonServiceClient(baseUrl); ;

        // store this for future if re-authentication is required
        vendor = user;
        this.apiKey = apiKey;
        this.token = token;
    }

    /// <summary>
    /// Authenticates the client on the eluCloud server. 
    /// </summary>
    /// <param name="vendor"></param>
    /// <param name="apiKey"></param>
    /// <param name="sessionId"></param>
    /// <exception cref="System.Net.WebException">Will throw this exception with status <see cref="WebExceptionStatus.ConnectFailure"/> if the eluCloud server is offline</exception>
    /// <exception cref="ServiceStack.WebServiceException">
    /// 401 - Unauthorized StatusCode in case of authentication failures.
    /// 405 - MethodNotAllowed StatusCode in case the REST API modules is not loaded.
    /// </exception>
    public async Task<string?> AuthenticateAsync()
    {
        SessionId = null;
        LoginResponse loginResponse = await jsonClient.GetAsync(new Login(), token);
        var encryptedPwd = computeSha256(apiKey, loginResponse.AuthSalt);
        var authResponse = jsonClient.Post(new Model.Authenticate
        {
            Provider = "api",
            UserName = vendor,
            Password = encryptedPwd,
        });

        return authResponse.SessionId;
    }

    public async Task ConnectAsync(string? sessionId = null)
    {
        if (null == sessionId)
        {
            SessionId = await AuthenticateAsync();
        }
        else
        {
            SessionId = sessionId;
        }

        InfoResponse infoResponse = jsonClient.Get(new Info());
        if (string.IsNullOrEmpty(infoResponse.ApiVersion))
        {
            ApiVersion = infoResponse.Version;
            ServerVersion = infoResponse.Version;
        }
        else
        {
            ApiVersion = infoResponse.ApiVersion;
            ServerVersion = infoResponse.Version;

            if (null != SessionId)
            {
                // in case of re-connections we must dispose the object first
                if (null != signalRHub)
                {
                    signalRHub.OnError -= SignalRHub_OnError;
                    await signalRHub.DisposeAsync();
                }

                signalRHub = new ServerPushesHubNetCore
                {
                    ReconnectTimeout = TimeSpan.FromMinutes(5)
                };

                // signalR error handling
                signalRHub.OnError += SignalRHub_OnError;

                await signalRHub.ConnectAsync(jsonClient.BaseUri, SessionId, token);

                foreach (var actions in serverPushHandlers)
                {
                    actions.RegisterToClient(signalRHub);
                }
            }
        }
    }

    private async void SignalRHub_OnError(Exception? obj)
    {
        await ConnectAsync();
    }

    private string computeSha256(string? apiKey, string? salt)
    {
        byte[] inputBytes = Encoding.ASCII.GetBytes(apiKey + salt);
        byte[] hash = SHA256.Create().ComputeHash(inputBytes);

        StringBuilder sb = new StringBuilder();
        foreach (byte b in hash)
        {
            sb.Append(b.ToString("X2").ToLower());
        }

        return sb.ToString();
    }

    public async Task<TResponse> GetAsync<TResponse>(IReturn<TResponse> request, CancellationToken token) => await jsonClient.GetAsync(request, token);
    public async Task<TResponse> GetAsync<TResponse>(string relativeOrAbsoluteUri, CancellationToken token) => await jsonClient.GetAsync<TResponse>(relativeOrAbsoluteUri, token);
    public async Task<TResponse> PostAsync<TResponse>(IReturn<TResponse> request, CancellationToken token) => await jsonClient.PostAsync(request, token);
    public async Task<TResponse> PostAsync<TResponse>(string relativeOrAbsoluteUri, object requestDto, CancellationToken token) => await jsonClient.PostAsync<TResponse>(relativeOrAbsoluteUri, requestDto, token);
    public void On<T>(string methodName, Action<T> handler)
    {
        var handlerWrapper = new ServerPushHandler(methodName, [typeof(T)], args => handler((T)args[0]!));
        serverPushHandlers.Add(handlerWrapper);

        // If not null, register right away.
        // Otherwise, it will register on connect()
        if (null != signalRHub)
        {
            handlerWrapper.RegisterToClient(signalRHub);
        }
    }
}
