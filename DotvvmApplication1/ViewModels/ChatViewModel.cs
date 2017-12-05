using DotVVMWebSocketExtension.WebSocketService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotvvmApplication1.ViewModels
{
	public class ChatViewModel : MasterpageViewModel
	{
		public WebSocketHub Hub { get; set; }

		public List<string> Messages { get; set; }
		public string Message { get; set; }
		public string GroupId { get; set; }


		public ChatViewModel(WebSocketHub hub)
		{
			Messages = new List<string> ();
			Hub = hub;
			GroupId = "1337";
		}

		public async Task SendMessage()
		{
			if (!string.IsNullOrEmpty(Message))
			{
				Messages.Add(Message);
				await Hub.SendViewModelToGroup();
				Message = "";
				await Hub.UpdateViewModelOnClient();
				Context.InterruptRequest();//todo
			}
		}

		public void CreateGroup()
		{

			Hub.CreateGroup(GroupId);
		}

		public void JoinGroup()
		{
			Hub.JoinGroup(GroupId);

			Messages.Add("you have joined group "+GroupId);
		}

	}
}