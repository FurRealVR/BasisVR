using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Basis.Network.Server.SocketChat
{
    internal class CommandHandler
    {
        public static async Task processCommandAsync(WebSocket ws, String command)
        {
            switch (command.ToLower()) {
                case "ping":
                    var ping = Encoding.UTF8.GetBytes("PONG!");
                    await ws.SendAsync(new ArraySegment<byte>(ping), WebSocketMessageType.Text, true, CancellationToken.None);
                    break;
                default:
                    var defaultResponse = Encoding.UTF8.GetBytes("Not Implemented.");
                    await ws.SendAsync(new ArraySegment<byte>(defaultResponse), WebSocketMessageType.Text, true, CancellationToken.None);
                    break;
            }
            
        }
    }
}
