using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

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

		[Required]
		[Column(TypeName = "datetime2")]
		public DateTime Time { get; set; }
	}
}