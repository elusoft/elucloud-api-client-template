using Microsoft.AspNetCore.SignalR.Client;
using System.Net;

namespace elusoft.eluCloud.Client.ServerPushes;

internal class ServerPushesHubNetCore : IServerPushesHub
{
    // This token is used when the instance is disposed
    private CancellationTokenSource cts = new CancellationTokenSource();
    private HubConnection? hub;

    public static TimeSpan DefaultReconnectTimeout = TimeSpan.FromMinutes(5);

    public event Action<Exception?>? OnError;

    /// <summary>
    /// Timespan specifying when to reconnect to the server,
    /// in case the underlying mechanism could not re-establish the connection.
    /// Small interuptions are handled by the signalR. This is only for major issues
    /// For example, when someone turns off the server.
    /// </summary>
    public TimeSpan ReconnectTimeout { get; set; } = DefaultReconnectTimeout;

    public string? ConnectionId { get; private set; }

    public void On(string methodName, Type[] parameterTypes, Action<object?[]> handler)
    {
        hub?.On(methodName, parameterTypes, (parameters, state) =>
        {
            var currentHandler = (Action<object?[]>)state;
            currentHandler(parameters);
            return Task.CompletedTask;
        }, handler);
    }

    public async Task ConnectAsync(string baseUri, string sessionId, CancellationToken token)
    {
        hub = new HubConnectionBuilder()
            .WithAutomaticReconnect()
            .WithUrl($"{baseUri}/hubs/api", options =>
            {
                options.Headers.Add("X-ss-id", sessionId);
                
            }).Build();

        hub.Closed += Hub_Closed;
        await hub.StartAsync(token);
        ConnectionId = hub.ConnectionId;
    }

    private async Task reconnect()
    {
        try
        {
            if (null == hub) return;

            Console.WriteLine("Reconnecting after: " + ReconnectTimeout);
            Thread.Sleep(ReconnectTimeout);
            if (hub.State != HubConnectionState.Disconnected)
            {
                // first try to close the connection
                await hub.StopAsync(cts.Token);
            }

            if (hub.State != HubConnectionState.Disconnected)
            {
                // If the connection didn't close, throw an exception.
                // This will re-attempt to connect after ReconnectTimeout
                throw new Exception("Connection has not yet been closed.");
            }

            await hub.StartAsync(cts.Token);
            ConnectionId = hub.ConnectionId;
        }
        catch (Exception)
        {
            // infinitely try to reconnect until disposed
            await reconnect();
        }
    }

    private async Task Hub_Closed(Exception? arg)
    {
        if (arg is OperationCanceledException && !cts.IsCancellationRequested)
        {
            await reconnect();
        }
        else if (null != OnError)
        {
            OnError(arg);
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (null == hub)
        {
            return;
        }

        hub.Closed -= Hub_Closed;
        cts.Cancel();
        await hub.DisposeAsync();
        hub = null;
    }
}
