using System;
using System.Threading;
using System.Threading.Tasks;
using DotVVMWebSocketExtension.WebSocketService;

namespace SampleApp.ViewModels
{
	public class ServerEventsViewModel : MasterpageViewModel
	{
		public WebSocketService Hub { get; set; } 
		public string Text { get; set; }
		public long Percentage { get; set; }
		public bool IsPercentageVisible { get; set; }


		public ServerEventsViewModel(WebSocketService hub)
		{
			Hub = hub;
			Text = "No action";
			IsPercentageVisible = false;
		}

		public void StartLongTask()
		{
			IsPercentageVisible = true;
			Percentage = 0;
			Text = "Action is starting";
			Hub.CreateAndRunTask<ServerEventsViewModel>(LongTaskAsync);

		}

		public async Task LongTaskAsync(ServerEventsViewModel viewModel, CancellationToken token)
		{
			
			for (int i = 1; i < 101; ++i)
			{
				await Task.Delay(10);

				token.ThrowIfCancellationRequested();
//				if (i==50)
//				{
//					await hub.UpdateViewModelInTaskFromCurrentClientAsync();
//				}

				viewModel.Percentage = i;
				await Hub.ChangeViewModelForCurrentConnection();
				await Task.Delay(10);
			}
			viewModel.Text = "Task is Complete";
			viewModel.IsPercentageVisible = false;
			Console.WriteLine(Context);
			await Hub.ChangeViewModelForCurrentConnection();
		}

		public void StopTask()
		{
			Hub.StopTask();
		}


		public override Task PreRender()
		{
			Hub.SaveCurrentState();
			return base.PreRender();
		}
	}
}