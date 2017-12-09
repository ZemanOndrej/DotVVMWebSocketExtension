using AutoMapper;
using BL.DTO;
using DAL.Entities;
using Microsoft.Extensions.Options;

namespace BL
{
	public class Mapping
	{
		public static IMapper Mapper { get; }

		static Mapping()
		{
			var config = new MapperConfiguration(c =>
			{
				c.CreateMap<ChatRoom, ChatRoomDto>().ReverseMap();
				c.CreateMap<User, UserDto>().ReverseMap();

				c.CreateMap<ChatMessage, ChatMessageDto>()
					.ForMember(d => d.ChatRoomId, options => options.MapFrom(src => src.ChatRoom.Id))
					.ForMember(d => d.UserId, options => options.MapFrom(src => src.User.Id))
					.ForMember(d=>d.UserName, options => options.MapFrom(src => src.User.Name));

				c.CreateMap<ChatMessageDto, ChatMessage>()
					.ForMember(m=>m.ChatRoom,opts=>opts.MapFrom(src=>new ChatRoom{Id = src.ChatRoomId}))
					.ForMember(m=>m.User,opts=>opts.MapFrom(src=>new User{Id = src.UserId}));

			});
			Mapper = config.CreateMapper();
		}
	}
}