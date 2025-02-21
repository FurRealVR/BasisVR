using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Basis.Network.Server.SocketChat
{
    public class WebSocketChatMain
    {
        private static List<WebSocket> _connectedSockets = new List<WebSocket>();

        public static async Task AcceptWebsocket(HttpListener list, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var ctx = await list.GetContextAsync();
                if (ctx.Request.IsWebSocketRequest)
                {
                    await ProcessSock(ctx);
                }
                else
                {
                    ctx.Response.StatusCode = 408;
                }
            }
        }

        private static async Task ProcessSock(HttpListenerContext ctx)
        {
            var wsCtx = await ctx.AcceptWebSocketAsync(null);
            var ws = wsCtx.WebSocket;
            _connectedSockets.Add(ws);
            var buff = new byte[1024];

            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buff), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buff, 0, result.Count);
                    if (message.StartsWith("/"))
                    {
                        CommandHandler.processCommandAsync(ws, message.Substring(1));
                    }
                    else
                    {
                        var sendMessage = Encoding.ASCII.GetBytes(message);
                        await SendMessageToAllAsync(sendMessage, ws);
                    }
                }
            }

            _connectedSockets.Remove(ws);
            ws.Dispose();
            var serverMessage = Encoding.UTF8.GetBytes("A User have left the server!");
            await ws.SendAsync(new ArraySegment<byte>(serverMessage), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private static async Task SendMessageToAllAsync(byte[] message, WebSocket senderSocket)
        {
            var tasks = new List<Task>();

            foreach (var socket in _connectedSockets)
            {
                if (socket != senderSocket)
                {
                    tasks.Add(socket.SendAsync(new ArraySegment<byte>(message), WebSocketMessageType.Text, true, CancellationToken.None));
                }
            }

            await Task.WhenAll(tasks);
        }

        public static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
