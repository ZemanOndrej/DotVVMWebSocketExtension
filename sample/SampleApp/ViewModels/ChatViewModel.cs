using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BL.DTO;
using BL.Facades;

namespace SampleApp.ViewModels
{
	public class ChatViewModel : MasterpageViewModel
	{
		public ChatService Service { get; set; }

		public ChatFacade ChatFacade { get; set; }

		public List<ChatMessageDto> Messages { get; set; }
		public UserDto CurrentUser { get; set; }
		public List<ChatRoomDto> ChatRooms { get; set; }
		public ChatRoomDto CurrentRoom { set; get; }
		public string NewMessage { get; set; }
		public string NewRoomName { get; set; }
		public bool IsInRoom { get; set; }
		public bool IsLoggedIn { get; set; }


		public ChatViewModel(ChatService wsService, ChatFacade facade)
		{
			ChatFacade = facade;
			CurrentUser = new UserDto();
			ChatRooms = new List<ChatRoomDto>();
			Messages = new List<ChatMessageDto>();
			CurrentRoom = new ChatRoomDto();

			Service = wsService;
		}

		public void LogIn()
		{
			if (string.IsNullOrEmpty(CurrentUser.Name) || string.IsNullOrEmpty(Service.ConnectionId)) return;
			CurrentUser.ConnectionId = Service.ConnectionId;
			ChatRooms = ChatFacade.GetAllChatRooms();
			CurrentUser.Id = ChatFacade.CreateUser(CurrentUser);
			IsLoggedIn = true;
		}

		public async Task SendMessage()
		{
			if (!string.IsNullOrEmpty(NewMessage) && CurrentRoom.Id > 0)
			{
				var msg = new ChatMessageDto
				{
					UserId = CurrentUser.Id,
					ChatRoomId = CurrentRoom.Id,
					Message = NewMessage,
					Time = DateTime.Now,
					UserName = CurrentUser.Name
				};
				msg.Id = ChatFacade.SendMessageToChatRoom(CurrentUser, msg);
				Messages.Add(msg);
				NewMessage = "";

				await Service.ChangeViewModelForConnectionsAsync((ChatViewModel viewModel) => { viewModel.Messages.Add(msg); },
					ChatFacade
						.GetAllUsersFromChatRoom(CurrentRoom.Id)
						.Select(s => s.ConnectionId)
						.ToList());
			}
		}

		public async Task CreateRoom()
		{
			if (string.IsNullOrEmpty(NewRoomName)) return;
			var newChatRoom = new ChatRoomDto
			{
				Name = NewRoomName
			};
			newChatRoom.Id = ChatFacade.CreateChatRoom(newChatRoom);
			ChatRooms.Add(newChatRoom);
			NewRoomName = "";

			await Service.ChangeViewModelForConnectionsAsync(
				(ChatViewModel viewModel) => { viewModel.ChatRooms.Add(newChatRoom); },
				ChatFacade.GetAllConnectedUsers()
					.Select(s => s.ConnectionId)
					.ToList());
		}

		public async Task JoinRoom(int id)
		{
			if (id == CurrentRoom.Id) return;
			if (CurrentRoom.Id != 0)
			{
				ChatFacade.LeaveChatRoom(CurrentRoom.Id,CurrentUser.Id);
				await Service.ChangeViewModelForConnectionsAsync(
					(ChatViewModel viewModel) =>
					{
						viewModel.CurrentRoom.UserList= viewModel.CurrentRoom.UserList.Where(u =>u.Id!=CurrentUser.Id).ToList();
						viewModel.Messages.Add(new ChatMessageDto
						{
							Message = "User has left",
							UserName = CurrentUser.Name,
							ChatRoomId = CurrentRoom.Id,
							Time = DateTime.Now,
							UserId = CurrentUser.Id
						});
					},
					ChatFacade
						.GetAllUsersFromChatRoom(CurrentRoom.Id)
						.Select(s => s.ConnectionId)
						.ToList());
			}

			CurrentRoom = ChatFacade.GetChatRoomById(id);
			ChatFacade.AddUserToChatRoom(id, CurrentUser);

			Messages = ChatFacade.GetRecentMessagesFromRoom(id);
			IsInRoom = true;
			CurrentRoom.UserList.Add(CurrentUser);
			await Service.ChangeViewModelForConnectionsAsync(
				(ChatViewModel viewModel) =>
				{
					viewModel.CurrentRoom.UserList.Add(CurrentUser);
					viewModel.Messages.Add(new ChatMessageDto
					{
						Message = "User has connected",
						UserName = CurrentUser.Name,
						ChatRoomId = CurrentRoom.Id,
						Time = DateTime.Now,
						UserId = CurrentUser.Id
					});
				},
				ChatFacade
					.GetAllUsersFromChatRoom(CurrentRoom.Id)
					.Select(s => s.ConnectionId)
					.ToList());
		}

		public override Task PreRender()
		{
			Service.SaveCurrentState();
			return base.PreRender();
		}
	}
}