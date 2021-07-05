using MyChat.Domain.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MyChat.Domain.Handlers
{
    public class WebSocketHandler
    {
        private readonly ILogger _logger;

        public WebSocketHandler(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<WebSocketHandler>();
        }

        public async Task Handle(WebSocketUser webSocket)
        {
            WebSocketUserCollection.Add(webSocket);
            _logger.LogInformation($"WebSocket user added.");
           
            WebSocketReceiveResult result = null;
            do
            {
                var buffer = new byte[1024 * 1];
                result = await webSocket.WebSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                if (result.MessageType == WebSocketMessageType.Text && !result.CloseStatus.HasValue)
                {
                    var msgString = Encoding.UTF8.GetString(buffer);
                    _logger.LogInformation($"WebSocket user ReceiveAsync message {msgString}.");
                    var message = JsonConvert.DeserializeObject<WebSocketMessage>(msgString);
                    message.userId = webSocket.Id;
                    WebSocketMessageRoute(message);
                }
            }
            while (!result.CloseStatus.HasValue);
            WebSocketUserCollection.Remove(webSocket);
            _logger.LogInformation($"WebSocket user closed.");
        }

        private void WebSocketMessageRoute(WebSocketMessage message)
        {
            var user = WebSocketUserCollection.Get(message.userId);
            var users = WebSocketUserCollection.GetRoomUsers(user.Room);

            switch (message.action)
            {
                case "join":
                    if (string.IsNullOrEmpty(message.nick))
                    {
                        user.SendWebSocketMessageAsync($"Please provide a Nickname");
                        break;
                    }

                    var usersFindNickname = WebSocketUserCollection.GetRoomUsers(message.msg);
                    var userFindNickname = usersFindNickname.Find(c => c.Nickname == message.nick);
                    if (userFindNickname != null)
                    {
                        user.SendWebSocketMessageAsync($"Sorry, the Nickname " + message.nick + " is already taken. Please choose different one");
                        break;
                    }

                    user.Room = message.msg;
                    user.Nickname = message.nick;
                    user.SendWebSocketMessageAsync($"{message.nick} join room {user.Room}.");

                    users = WebSocketUserCollection.GetRoomUsers(user.Room);
                    users.ForEach(u =>
                    {
                        if (u.Id != user.Id)
                        {                          
                            u.SendWebSocketMessageAsync($"{message.nick} join room {user.Room}.");
                        }
                    });

                    _logger.LogInformation($"WebSocket user {message.userId} join room {user.Room}.");

                    break;
                case "send_to_room":
                    if (string.IsNullOrEmpty(user.Room))
                    {
                        break;
                    }

                    if (string.IsNullOrEmpty(message.toUser))
                    {
                        users.ForEach(u =>
                        {
                            u.SendWebSocketMessageAsync(message.nick + ": " + message.msg);
                        });
                    }
                    else
                    {
                        var toUser = users.Find(c => c.Nickname == message.toUser);
                        if (toUser != null)
                        {
                            user.SendWebSocketMessageAsync("you says to " + message.toUser + ": " + message.msg);
                            toUser.SendWebSocketMessageAsync(message.nick + " says to you: " + message.msg);
                        }
                        else
                        {
                            user.SendWebSocketMessageAsync($"Sorry, the User '" + message.toUser + "' not in the room at the moment");
                        }
                    }
                   
                    _logger.LogInformation($"WebSocket user {message.userId} send message {message.msg} to room {user.Room}");

                    break;
                case "leave":
                    if (string.IsNullOrEmpty(user.Room))
                    {                        
                        break;
                    }

                    var roomNo = user.Room;

                    user.Room = "";                    

                    users.ForEach(u =>
                    {
                        u.SendWebSocketMessageAsync($"{message.nick} leave room {roomNo}.");
                    });

                    _logger.LogInformation($"WebSocket user {message.userId} leave room {roomNo}");

                    break;
                default:
                    break;
            }
        }
    }
}
