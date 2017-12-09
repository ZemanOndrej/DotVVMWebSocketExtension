using System;
using DotVVMWebSocketExtension.WebSocketService;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BL.DTO;
using BL.Facades;

namespace DotvvmApplication1.ViewModels
{
	public class ChatViewModel : MasterpageViewModel
	{
		public WebSocketFacade Hub { get; set; }
		public ChatFacade ChatFacade { get; set; }

		public List<ChatMessageDto> Messages { get; set; }
		public UserDto CurrentUser { get; set; }
		public List<ChatRoomDto> ChatRooms { get; set; }
		public ChatRoomDto CurrentRoom { set; get; }
		public string NewMessage { get; set; }
		public string NewRoomName { get; set; }
		public bool IsInRoom { get; set; }
		public bool IsLoggedIn { get; set; }


		public ChatViewModel(WebSocketFacade wsHub, ChatFacade facade)
		{
			ChatFacade = facade;
			CurrentUser = new UserDto();
			ChatRooms = new List<ChatRoomDto>();
			Messages = new List<ChatMessageDto>();
			CurrentRoom = new ChatRoomDto();

			Hub = wsHub;
		}

		public void LogIn()
		{
			if (string.IsNullOrEmpty(CurrentUser.Name) || string.IsNullOrEmpty(Hub.ConnectionId)) return;
			CurrentUser.SocketId = Hub.ConnectionId;
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
					Time = DateTime.Now
				};
				//				await hub.UpdateViewModelOnCurrentClientAsync();
				;

				Messages.Add(ChatFacade.GetChatMessageById(ChatFacade.SendMessageToChatRoom(CurrentUser, msg)));
//				if (Messages.Count > 2)
//				{
//					Messages.RemoveAt(0);
//				}
//				await hub.UpdateViewModelOnCurrentClientAsync();

				await Hub.SyncViewModelForSocketsAsync(
					ChatFacade.GetAllUsersFromChatRoom(CurrentRoom.Id)
						.Select(s => s.SocketId)
						.ToList());
				NewMessage = "";

//				Context.InterruptRequest();//todo
//				ChatFacade.GetAllUsersFromChatRoom(1);
			}
		}

		public async Task CreateRoom()
		{
			if (string.IsNullOrEmpty(NewRoomName)) return;

			var newChatRoom = new ChatRoomDto {Name = NewRoomName};
			newChatRoom.Id = ChatFacade.CreateChatRoom(newChatRoom);

			ChatRooms.Add(newChatRoom);
			await Hub.SyncViewModelForSocketsAsync(
				ChatFacade.GetAllConnectedUsers()
					.Select(s => s.SocketId)
					.ToList());
			NewRoomName = "";
		}

		public void JoinRoom(int id)
		{
			if (id == CurrentRoom.Id) return;
			ChatFacade.AddUserToChatRoom(id, CurrentUser);
			CurrentRoom.Id = id;
			Messages = ChatFacade.GetAllMessagesFromRoom(id);
			IsInRoom = true;
		}

		public override Task PreRender()
		{
			Console.WriteLine(Hub.ConnectionId);
			Hub.SaveContext();
			return base.PreRender();
		}
	}
}