using System.Threading;
using System.Threading.Tasks;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1.ViewModels
{
	public class ServerEventsViewModel : MasterpageViewModel
	{
		public WebSocketHub Hub { get; set; } // TODO NAME OF HUB VARIABLE
		public string Text { get; set; }
		public long Percentage { get; set; }
		public bool IsPercentageVisible { get; set; }


		public ServerEventsViewModel(WebSocketHub hub)
		{
			Hub = hub;
			Text = "No action";
			IsPercentageVisible = false;
		}

		public void StartLongTask()
		{
			Hub.CreateAndRunTask(LongTaskAsync);
		}

		public async Task LongTaskAsync( CancellationToken token)
		{
			IsPercentageVisible = true;
			await Hub.UpdateCurrentViewModelOnClient();
			Percentage = 0;
			for (int i = 0; i < 10; ++i)
			{
				await Task.Delay(101);

				token.ThrowIfCancellationRequested();

				Percentage = i;
				await Hub.UpdateCurrentViewModelOnClient();
				await Task.Delay(100);
			}
			Text = "Task is Complete";
			IsPercentageVisible = false;
			await Hub.UpdateCurrentViewModelOnClient();
		}

		public void StopTask()
		{
			Hub.StopTask();
		}
	}
}