using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
	public class ChatRoom 
	{
		public int Id { get; set; }

		[Required]
		public List<ChatMessage> ChatMessages { get; set; }

		public List<User> UserList { get; set; }

		public string Name { get; set; }

		public ChatRoom()
		{
			ChatMessages = new List<ChatMessage>();
			UserList = new List<User>();

		}
	}
}