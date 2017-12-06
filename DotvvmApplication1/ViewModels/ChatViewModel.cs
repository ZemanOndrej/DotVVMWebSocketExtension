using System;
using DotVVMWebSocketExtension.WebSocketService;
using System.Collections.Generic;
using System.Threading.Tasks;
using BL.DTO;
using BL.Facades;

namespace DotvvmApplication1.ViewModels
{
	public class ChatViewModel : MasterpageViewModel
	{
		public WebSocketHub Hub { get; set; }
		public ChatFacade ChatFacade { get; set; }

		public List<ChatMessageDto> Messages { get; set; }
		public string NewMessage { get; set; }
		public ChatRoomDto CurrentRoom { get; set; }
		public ChatRoomDto NewRoom { get; set; }
		public bool IsInRoom { get; set; }
		public UserDto CurrentUser { get; set; }
		public bool IsLoggedIn { get; set; }
		public List<ChatRoomDto> ChatRooms { get; set; }


		public ChatViewModel(WebSocketHub hub, ChatFacade facade)
		{
			ChatFacade = facade;
			CurrentUser = new UserDto();
			ChatRooms = ChatFacade.GetAllChatRooms();
			Messages = new List<ChatMessageDto>();
			CurrentRoom = new ChatRoomDto();
			NewRoom = new ChatRoomDto();

			Hub = hub;
		}

		public void LogIn()
		{
			if (string.IsNullOrEmpty(CurrentUser.Name) || string.IsNullOrEmpty(Hub.CurrentSocketId)) return;
			CurrentUser.SocketId = Hub.CurrentSocketId;
			CurrentUser.Id = ChatFacade.CreateUser(CurrentUser);
			IsLoggedIn = true;
		}

		public async Task SendMessage()
		{
			if (!string.IsNullOrEmpty(NewMessage) && CurrentRoom.Id > 0)
			{
				var msg = new ChatMessageDto
				{
					User = CurrentUser,
					ChatRoomId = CurrentRoom.Id,
					Message = NewMessage
				};
				await Hub.UpdateViewModelOnClient();
				NewMessage = "";
				var id = ChatFacade.SendMessageToChatRoom(msg);
				Console.WriteLine(id);


				Messages.Add(msg);
//				await Hub.UpdateViewModelOnClient();
//				Context.InterruptRequest();//todo
				ChatFacade.GetAllUsersFromChatRoom(1);
			}
		}

		public void CreateRoom()
		{
			if (string.IsNullOrEmpty(NewRoom.Name)) return;

			NewRoom.Id = ChatFacade.CreateChatRoom(NewRoom);

			ChatRooms.Add(NewRoom);
			NewRoom = new ChatRoomDto();
		}

		public void JoinRoom(int id)
		{
			if (id == CurrentRoom.Id) return;
			ChatFacade.AddUserToChatRoom(id, CurrentUser);
			CurrentRoom.Id = id;
			Messages = ChatFacade.GetAllMessagesFromRoom(id);
			IsInRoom = true;
		}
	}
}