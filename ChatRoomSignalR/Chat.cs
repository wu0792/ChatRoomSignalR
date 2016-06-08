using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace ChatRoomSignalR
{
    public class Chat : Hub
    {
        static Dictionary<string, string> users = new Dictionary<string, string>();

        public void Send(string to, string message)
        {
            var fromName = users.ContainsKey(Context.ConnectionId) ? users[Context.ConnectionId] : "-UNKNOWN-";
            var toName = to == "" ? "ALL" : users.ContainsKey(to) ? users[to] : "-UNKOWN-";
            var responesMsg = string.Format("[{0}]{1} said to {2}: {3}", DateTime.Now, fromName, toName, message);

            if (to == "")
            {
                Clients.All.send(responesMsg);
            }
            else
            {
                Clients.Caller.send(responesMsg);
                Clients.Client(to).send(responesMsg);
            }
        }

        public void Reg(string name)
        {
            if (!users.ContainsKey(Context.ConnectionId))
            {
                if (users.Any(t => t.Value == name))
                {
                    Clients.Caller.regFailure(string.Format("the name({0}) has been registered already, change one and try again.", name));
                }
                else
                {
                    users[Context.ConnectionId] = name;
                    Clients.Caller.regSuccess();
                    Clients.All.refreshUsers(Newtonsoft.Json.JsonConvert.SerializeObject(users.Select(t => new { id = t.Key, name = t.Value })));
                }
            }
        }

        public override Task OnDisconnected(bool stopCalled)
        {
            users.Remove(Context.ConnectionId);
            Clients.All.refreshUsers(Newtonsoft.Json.JsonConvert.SerializeObject(users.Select(t => new { id = t.Key, name = t.Value })));

            return base.OnDisconnected(stopCalled);
        }
    }
}