using System;
using System.Collections.Generic;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Basis.Network.Server.ChatService
{
    internal class WebSocketChat
    {
        public static async Task AcceptWebsocket(HttpListener list, CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                var ctx = await list.GetContextAsync();
                if (ctx.Request.IsWebSocketRequest)
                {
                    ProcessSock(ctx);
                }
                else
                {
                    ctx.Response.StatusCode = 408;
                }
            }
        }

        private static async void ProcessSock(HttpListenerContext ctx)
        {
            var wsCtx = await ctx.AcceptWebSocketAsync(null);
            var ws = wsCtx.WebSocket;
            var buff = new byte[1024];

            while (ws.State == WebSocketState.Open)
            {
                var result = await ws.ReceiveAsync(new ArraySegment<byte>(buff), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var message = Encoding.UTF8.GetString(buff, 0, result.Count);
                    var sendMessage = Encoding.ASCII.GetBytes(message);
                    await ws.SendAsync(new ArraySegment<byte>(sendMessage), WebSocketMessageType.Text, true, CancellationToken.None);
                }
            }
            ws.Dispose();
            var serverMessage = Encoding.UTF8.GetBytes("A User have left the server!");
            await ws.SendAsync(new ArraySegment<byte>(serverMessage), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        public static string GetLocalIPAddress() {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList) {
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork) {
                    return ip.ToString();
                }
            }
            throw new Exception("No network adapters with an IPv4 address in the system!");
        }
    }
}
