using System;
using System.Threading;
using System.Threading.Tasks;
using DotVVMWebSocketExtension.WebSocketService;

namespace SampleApp.ViewModels
{
	public class ServerEventsViewModel : MasterpageViewModel
	{
		public WebSocketService Service { get; set; } 
		public string Text { get; set; }
		public long Percentage { get; set; }
		public bool IsPercentageVisible { get; set; }


		public ServerEventsViewModel(WebSocketService service)
		{
			Service = service;
			Text = "No action";
			IsPercentageVisible = false;
		}

		public void StartLongTask()
		{
			IsPercentageVisible = true;
			Percentage = 0;
			Text = "Action is starting";
			Service.CreateAndRunTask<ServerEventsViewModel>(LongTaskAsync);

		}

		public async Task LongTaskAsync(ServerEventsViewModel viewModel, CancellationToken token)
		{
			
			for (int i = 1; i < 101; ++i)
			{
				await Task.Delay(10);

				token.ThrowIfCancellationRequested();
//				if (i==50)
//				{
//					await service.UpdateViewModelInTaskFromCurrentClientAsync();
//				}

				viewModel.Percentage = i;
				await Service.ChangeViewModelForCurrentConnection();
				await Task.Delay(10);
			}
			viewModel.Text = "Task is Complete";
			viewModel.IsPercentageVisible = false;
			Console.WriteLine(Context);
			await Service.ChangeViewModelForCurrentConnection();
		}

		public void StopTask()
		{
			Service.StopTask();
		}


		public override Task PreRender()
		{
			Service.SaveCurrentState();
			return base.PreRender();
		}
	}
}