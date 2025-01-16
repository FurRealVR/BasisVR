using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace Basis.Network.Server.SocketChat
{
    internal class CommandHandler
    {
        public static async Task processCommandAsync(WebSocket ws, String command)
        {
            await ws.SendAsync("Not implemented.");
        }
    }
}
