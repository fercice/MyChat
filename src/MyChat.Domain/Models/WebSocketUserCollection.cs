using System.Collections.Generic;
using System.Linq;

namespace MyChat.Domain.Models
{
    public class WebSocketUserCollection
    {
        private static List<WebSocketUser> _users = new List<WebSocketUser>();

        public static void Add(WebSocketUser user)
        {
            _users.Add(user);
        }

        public static void Remove(WebSocketUser user)
        {
            _users.Remove(user);
        }

        public static WebSocketUser Get(string userId)
        {
            var user = _users.FirstOrDefault(c=>c.Id == userId);

            return user;
        }

        public static List<WebSocketUser> GetRoomUsers(string roomNo)
        {
            var user = _users.Where(u => u.Room == roomNo);
            return user.ToList();
        }
    }
}
