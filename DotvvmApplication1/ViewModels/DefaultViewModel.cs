using DotVVMWebSocketExtension.WebSocketService;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DotvvmApplication1.ViewModels
{
	public class DefaultViewModel : MasterpageViewModel
	{
		public int A { get; set; }
		public int B { get; set; }
		public int C { get; set; }

		public WebSocketHub Hub { get; set; }

		public List<string> Messages { get; set; }
		public string Message { get; set; }


		public DefaultViewModel(WebSocketHub hub)
		{
			Messages = new List<string>();
			Hub = hub;
		}

		public async Task SendMessage()
		{
			Messages.Add(Message);
			await Hub.UpdateCurrentViewModelOnClient();
		}

		public void Sum()
		{
			C = A + B;
		}
	}
}