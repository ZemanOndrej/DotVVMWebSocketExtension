using System;
using System.ComponentModel.DataAnnotations;

namespace BL.DTO
{
	public class ChatMessageDto
	{
		public int Id { get; set; }
		[Required]
		public string Message { get; set; }
		[Required]
		public UserDto User { get; set; }
		[Required]
		public int ChatRoomId { get; set; }

		[Required]
		public DateTime Time { get; set; }
	}
}