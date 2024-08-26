using System;
using System.Net;
using System.Threading.Tasks;

namespace elusoft.eluCloud.ServerPushes
{
    internal interface IServerPushesHub : IAsyncDisposable
    {
        /// <summary>
        /// This is filled as soon as the connection is established.
        /// In case of failures and reconnections, the connection id may change
        /// </summary>
        string ConnectionId { get; }
        /// <summary>
        /// Establishes a signalR connection with the server
        /// </summary>
        /// <param name="baseUri">For localhost, this typically looks like http://localhos:31750/</param>
        /// <param name="cookies"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task Connect(string baseUri, CookieContainer cookies, string sessionId);
        /// <summary>
        /// Registers a server event handler
        /// </summary>
        /// <param name="methodName"></param>
        /// <param name="parameterTypes"></param>
        /// <param name="handler"></param>
        void On(string methodName, Type[] parameterTypes, Action<object[]> handler);

        /// <summary>
        /// This event occurs when there is some error and the connection cannot be re-established.
        /// This should normally invoke the whole authentication flow. An example might be an expired session.
        /// </summary>
        event Action<Exception> OnError;
    }
}
