using elusoft.eluCloud.Model;
using elusoft.eluCloud.ServerPushes;
using ServiceStack;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace elusoft.eluCloud
{
    public sealed class Client
    {
        private IServerPushesHub signalRHub;
        private readonly JsonServiceClient jsonClient;
        private readonly List<ServerPushHandler> serverPushHandlers;
        private string vendor;
        private string apiKey;

        public string SessionId { get; private set; }
        public string SignalRConnectionId => signalRHub?.ConnectionId;
        public string ApiVersion { get; private set; }
        public string ServerVersion { get; private set; }

        public Client(string hostname = "localhost", int port = 31750, string scheme = "http")
        {
            jsonClient = new JsonServiceClient($"{scheme}://{hostname}:{port}") { StoreCookies = true };
            serverPushHandlers = new List<ServerPushHandler>();
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
        public async Task Authenticate(string vendor, string apiKey)
        {
            // store this for future if re-authentication is required
            this.vendor = vendor;
            this.apiKey = apiKey;

            LoginResponse loginResponse = jsonClient.Get(new Login());
            var encryptedPwd = computeSha256(apiKey, loginResponse.AuthSalt);
            var authResponse = jsonClient.Post(new Model.Authenticate
            {
                Provider = "api",
                UserName = vendor,
                Password = encryptedPwd,
            });

            InfoResponse infoResponse = jsonClient.Get(new Info());
            if (string.IsNullOrEmpty(infoResponse.ApiVersion))
            {
                ApiVersion = infoResponse.Version;
                ServerVersion = infoResponse.Version;

                signalRHub = new ServerPushesHubNetFramework
                {
                    ReconnectTimeout = TimeSpan.FromMinutes(5)
                };
            }
            else
            {
                ApiVersion = infoResponse.ApiVersion;
                ServerVersion = infoResponse.Version;

                signalRHub = new ServerPushesHubNetCore
                {
                    ReconnectTimeout = TimeSpan.FromMinutes(5)
                };
            }

            // signalR error handling
            signalRHub.OnError += SignalRHub_OnError;

            await signalRHub.Connect(jsonClient.BaseUri, jsonClient.CookieContainer, authResponse.SessionId);

            foreach (var actions in serverPushHandlers)
            {
                actions.RegisterToClient(signalRHub);
            }

            SessionId = authResponse.SessionId;
        }

        private async void SignalRHub_OnError(Exception obj)
        {
            await signalRHub.DisposeAsync();
            await Authenticate(vendor, apiKey);
        }

        private string computeSha256(string apiKey, string salt)
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

        public TResponse Get<TResponse>(IReturn<TResponse> request) => jsonClient.Get(request);
        public TResponse Get<TResponse>(string relativeOrAbsoluteUri) => jsonClient.Get<TResponse>(relativeOrAbsoluteUri);
        public TResponse Post<TResponse>(IReturn<TResponse> request) => jsonClient.Post(request);
        public TResponse Post<TResponse>(string relativeOrAbsoluteUri, object requestDto) => jsonClient.Post<TResponse>(relativeOrAbsoluteUri, requestDto);
        public void On<T>(string methodName, Action<T> handler)
        {
            var handlerWrapper = new ServerPushHandler(methodName, new[] { typeof(T) }, args => handler((T)args[0]));
            serverPushHandlers.Add(handlerWrapper);
            // register to underlying hub if connected.
            handlerWrapper.RegisterToClient(signalRHub);
        }
    }
}
