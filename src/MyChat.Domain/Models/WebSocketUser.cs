using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyChat.Domain.Models
{
    public class WebSocketUser
    {
        public WebSocket WebSocket { get; set; }

        public string Id { get; set; }

        public string Room { get; set; }

        public string Nickname { get; set; }

        public Task SendWebSocketMessageAsync(string message)
        {
            var msg = Encoding.UTF8.GetBytes(message);
            return WebSocket.SendAsync(new ArraySegment<byte>(msg, 0, msg.Length), WebSocketMessageType.Text, true, CancellationToken.None);
        }
    }
}
