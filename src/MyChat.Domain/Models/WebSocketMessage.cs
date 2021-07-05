using System;

namespace MyChat.Domain.Models
{
    public class WebSocketMessage
    {
        public string userId { get; set; }

        public string action { get; set; }

        public string msg { get; set; }

        public string nick { get; set; }

        public string toUser { get; set; }
    }
}
