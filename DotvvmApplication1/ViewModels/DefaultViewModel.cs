using DotVVM.Framework.ViewModel;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1.ViewModels
{
	public class DefaultViewModel : DotvvmViewModelBase
	{

		public string Title { get; set; }

		public MyHub Hub { get; set; }


		public DefaultViewModel()
		{
			Title = "Hello from DotVVM!";
		}


		public void CreateTask()
		{
			

		}


	}
}
