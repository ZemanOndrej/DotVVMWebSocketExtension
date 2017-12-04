using DotVVMWebSocketExtension.WebSocketService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotvvmApplication1.ViewModels
{
	public class ChatViewModel : MasterpageViewModel
	{
		public int A { get; set; }
		public int B { get; set; }
		public int C { get; set; }

		public WebSocketHub Hub { get; set; }

		public List<string> Messages { get; set; }
		public string Message { get; set; }


		public ChatViewModel(WebSocketHub hub)
		{
			Messages = new List<string> {"welcome to the chat"};
			Hub = hub;
		}

		public async Task SendMessage()
		{
			if (!string.IsNullOrEmpty(Message))
			{
				Messages.Add(Message);
				await Hub.UpdateCurrentViewModelOnClient();
				Message = "";
			}
		}

		public void Sum()
		{
			C = A + B;
		}
	}
}