using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BL.DTO
{
	public class ChatRoomDto
	{
		public int Id { get; set; }

		[Required]
		public List<ChatMessageDto> ChatMessages { get; set; }

		public List<UserDto> UserList { get; set; }

		public string Name { get; set; }

		public ChatRoomDto()
		{
			ChatMessages = new List<ChatMessageDto>();
			UserList = new List<UserDto>();

		}
	}
}