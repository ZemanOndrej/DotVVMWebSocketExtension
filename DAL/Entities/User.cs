using System.Collections.Generic;

namespace DAL.Entities
{
	public class User
	{
		public int Id { get; set; }
		public string SocketId { get; set; }
		public string Name { get; set; }
		public ChatRoom CurrentChatRoom { get; set; }
		public List<ChatMessage> Messages { get; set; }

		public User()
		{
			Messages = new List<ChatMessage>();
		}
	}
}