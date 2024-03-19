using Microsoft.AspNet.SignalR.Client;
using Microsoft.AspNet.SignalR.Client.Http;
using Microsoft.AspNet.SignalR.Client.Hubs;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace elusoft.eluCloud.ServerPushes
{
    internal class ServerPushesHubNetFramework : IServerPushesHub
    {
        private CancellationTokenSource cts = new CancellationTokenSource();    
        private IHubProxy hub;
        private HubConnection connection;

        public static TimeSpan DefaultReconnectTimeout = TimeSpan.FromMinutes(5);

        public event Action<Exception> OnError;

        /// <summary>
        /// Timespan specifying when to reconnect to the server,
        /// in case the underlying mechanism could not re-establish the connection.
        /// Small interuptions are handled by the signalR. This is only for major issues
        /// For example, when someone turns off the server.
        /// </summary>
        public TimeSpan ReconnectTimeout { get; set; } = DefaultReconnectTimeout;
        public string ConnectionId { get; private set; } = string.Empty;

        public void On(string methodName, Type[] parameterTypes, Action<object[]> handler)
        {
            Subscription subscription = hub.Subscribe(methodName);
            Action<IList<JToken>> handlerWrapper = (tokens) => 
            {
                IList<object> args= new List<object>();
                for (int i = 0; i < parameterTypes.Length; i++)
                {
                    if (tokens.Count > i)
                    {
                        args.Add(tokens[i].ToObject(parameterTypes[i]));
                    }
                }

                if (null != handler)
                {
                    handler(args.ToArray());
                }
            };

            subscription.Received += handlerWrapper;
        }

        public async Task Connect(string baseUri, CookieContainer cookies, string sessionId)
        {
            connection = new HubConnection($"{baseUri}/live/signalr");
            hub = connection.CreateHubProxy("eci");
            connection.CookieContainer = cookies;
            connection.Headers.Add("ss-id", sessionId);
            connection.Error += SignalR_Error;
            ConnectionId = await startSignalR();
        }

        private async Task<string> startSignalR()
        {
            await connection.Start(new DefaultHttpClient());
            return connection.ConnectionId;
        }

        private async Task reconnectSignalR()
        {
            try
            {
                Thread.Sleep(ReconnectTimeout);
                if (connection.State != ConnectionState.Disconnected)
                {
                    // first try to close the connection
                    connection.Stop();
                }

                if (connection.State != ConnectionState.Disconnected)
                {
                    // If the connection didn't close, throw an exception.
                    // This will re-attempt to connect after ReconnectTimeout
                    throw new Exception("Connection has not yet been closed.");
                }

                ConnectionId = await startSignalR();
            }
            catch (Exception)
            {
                // infinitely try to reconnect until disposed
                await reconnectSignalR();
            }
        }

        private void SignalR_Error(Exception obj)
        {
            if (obj is TimeoutException te)
            {
                try
                {
                    Task.Run(async () =>
                    {
                        await reconnectSignalR();
                    }).Wait(cts.Token);
                }
                catch (OperationCanceledException)
                {
                    // no-op: this is fine, this happens when disposing
                }
            }
            else if (obj is HttpClientException httpEx && httpEx.Response?.StatusCode == HttpStatusCode.Unauthorized && null != OnError)
            {
                // This is thrown when hub client is unauthorized.
                // The error handling here might be improved, although
                // it might be sufficient to propagate the event higher
                // and re-authenticate the client
                OnError(obj);
            }
            else if (null != OnError)
            {
                OnError(obj);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (null == connection)
            {
                return;
            }

            await Task.Run(() =>
            {
                connection.Error -= SignalR_Error;
                cts.Cancel();
                connection.Dispose();
            });
        }
    }
}
