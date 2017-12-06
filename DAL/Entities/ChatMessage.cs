using System.ComponentModel.DataAnnotations;

namespace DAL.Entities
{
	public class ChatMessage
	{
		public int Id { get; set; }
		[Required]
		public string Message { get; set; }
		[Required]
		public User User { get; set; }
		[Required]
		public ChatRoom ChatRoom { get; set; }
	}
}