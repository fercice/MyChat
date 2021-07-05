using MyChat.Domain.Handlers;
using MyChat.Domain.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Net.WebSockets;
using System.Threading.Tasks;

namespace MyChat.Presentation.Server
{
    public class WebSocketMiddleware
    {
        private readonly ILogger _logger;
        private readonly RequestDelegate _next;        
        private readonly WebSocketHandler _webSocketHandler;

        public WebSocketMiddleware(ILoggerFactory loggerFactory, 
                                   RequestDelegate next,
                                   WebSocketHandler webSocketHandler)
        {
            _next = next;
            _webSocketHandler = webSocketHandler;
            _logger = loggerFactory.CreateLogger<WebSocketMiddleware>();
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.Request.Path == "/chat")
            {
                if (context.WebSockets.IsWebSocketRequest)
                {
                    WebSocket webSocket = await context.WebSockets.AcceptWebSocketAsync();
                    string userId = Guid.NewGuid().ToString(); ;
                    var wsUser = new WebSocketUser
                    {
                        Id = userId,
                        WebSocket = webSocket
                    };
                    try
                    {
                        await _webSocketHandler.Handle(wsUser);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Echo websocket user {0} err .", userId);
                        await context.Response.WriteAsync("closed");
                    }
                }
                else
                {
                    context.Response.StatusCode = 404;
                }
            }
            else
            {
                await _next(context);
            }
        }
    }
}
