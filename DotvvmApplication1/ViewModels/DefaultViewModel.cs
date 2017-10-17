using System.Collections.Generic;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1.ViewModels
{
	public class DefaultViewModel : DotvvmViewModelBase
	{

		public int A { get; set; }
		public int B { get; set; }
		public int C { get; set; }

		private readonly MyHub Hub;

		public List<string> Messages { get; set; }
		public string Message { get; set; }


		public DefaultViewModel(MyHub hub)
		{
			Messages = new List<string>();
			Hub = hub;

		}


		public async Task SendMessage()
		{

			Messages.Add(Message);
			//			await Hub.UpdateViewModelOnClient();
			//			await Hub.SendMessageToAllAsync(Message);
			await Hub.UpdateViewModelOnClient(Context);

		}

		public void Sum()
		{
			C = A + B;
		}

	}
}
