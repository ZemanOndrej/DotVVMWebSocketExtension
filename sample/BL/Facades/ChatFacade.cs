﻿using System;
using System.Collections.Generic;
using System.Linq;
using BL.DTO;
using DAL;
using DAL.Entities;
using Microsoft.EntityFrameworkCore;

namespace BL.Facades
{
	public class ChatFacade
	{
		private ChatDbContext Context { get; }

		public ChatFacade(ChatDbContext ctx) => Context = ctx;

		public int CreateUser(UserDto user)
		{
			var nuser = Mapping.Mapper.Map<User>(user);
			Context.Users.Add(nuser);
			Context.SaveChanges();
			return nuser.Id;
		}

		public int CreateChatRoom(ChatRoomDto room)
		{
			var newRoom = Mapping.Mapper.Map<ChatRoom>(room);
			if (string.IsNullOrEmpty(newRoom.Name)) return 0;
			Context.ChatRooms.Add(newRoom);
			Context.SaveChanges();

			return newRoom.Id;
		}

		public List<ChatRoomDto> GetAllChatRooms() => Mapping.Mapper.Map<List<ChatRoomDto>>(Context.ChatRooms.ToList());
		public List<UserDto> GetAllConnectedUsers() => Mapping.Mapper.Map<List<UserDto>>(Context.Users.ToList());

		public void AddUserToChatRoom(int roomId, UserDto user)
		{
			var usr = Context.Users.Include(u=>u.CurrentChatRoom).FirstOrDefault(u => u.Id == user.Id);
			var room = Context.ChatRooms.Include(s=>s.UserList).FirstOrDefault(g=>g.Id==roomId);
			usr.CurrentChatRoom?.UserList.Remove(usr);
			usr.CurrentChatRoom = room;
			if (!room.UserList.Contains(usr))
			{
				room.UserList.Add(usr);
			}
			Context.SaveChanges();
		}

		public List<UserDto> GetAllUsersFromChatRoom(int chatId)
		{
			var users = Context.ChatRooms.Include(i => i.UserList).FirstOrDefault(r => r.Id == chatId).UserList.ToList();
			return Mapping.Mapper.Map<List<UserDto>>(users);
		}

		public void LeaveChatRoom(int roomId, int userId)
		{
			var group = Context.ChatRooms.Find(roomId);
			group.UserList.Remove(new User {Id = userId});
			Context.SaveChanges();
		}

		public List<ChatMessageDto> GetAllMessagesFromRoom(int id)
		{
			var msgs = Context.ChatMessages.Where(c => c.ChatRoom.Id == id).OrderByDescending(m=>m.Time).Include(u => u.User).ToList();
			return Mapping.Mapper.Map<List<ChatMessageDto>>(msgs);
		}
		public List<ChatMessageDto> GetRecentMessagesFromRoom(int id)
		{
			var msgs = Context.ChatMessages.Where(c => c.ChatRoom.Id == id).OrderByDescending(m=>m.Time).Take(5).Include(u => u.User).ToList();
			return Mapping.Mapper.Map<List<ChatMessageDto>>(msgs);
		}

		public int SendMessageToChatRoom(UserDto user,ChatMessageDto messageDto)
		{
			var message = Mapping.Mapper.Map<ChatMessage>(messageDto);

			message.ChatRoom = Context.ChatRooms.Find(message.ChatRoom.Id);
			message.User = Context.Users.FirstOrDefault(u => u.Id == message.User.Id);
			Context.ChatMessages.Add(message);
			Context.SaveChanges();
			return message.Id;
		}

		public ChatMessageDto GetChatMessageById(int id)
		{
			return Mapping.Mapper.Map<ChatMessageDto>(Context.ChatMessages.Find(id));
		}
		public ChatRoomDto GetChatRoomById(int id)
		{
			return Mapping.Mapper.Map<ChatRoomDto>(Context.ChatRooms.Find(id));
		}

		public void DeteleDisconnectedUsers(string connectionId)
		{
			Context.Users.Remove(Context.Users.First(u => u.ConnectionId == connectionId));
			Context.SaveChanges();
		}


	}
}