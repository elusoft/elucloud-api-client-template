using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace elusoft.eluCloud.ServerPushes
{
    public class ServerPushHandler
    {
        private readonly string methodName;
        private readonly Action<object[]> action;
        private readonly Type[] parameters;

        public ServerPushHandler(string methodName, Type[] parameters, Action<object[]> action)
        {
            this.methodName = methodName;
            this.action = action;
            this.parameters = parameters;
        }

        internal void RegisterToClient(IServerPushesHub client)
        {
            client?.On(methodName, parameters, action);
        }
    }
}
