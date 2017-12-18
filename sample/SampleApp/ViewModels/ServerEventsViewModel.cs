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
		public string Text2 { get; set; }
		public long Percentage { get; set; }
		public bool IsPercentageVisible { get; set; }


		public ServerEventsViewModel(WebSocketService service)
		{
			Service = service;
			Text = "No action";
			Text2 = "1337";
			IsPercentageVisible = false;
		}

		public void StartLongTask()
		{
			IsPercentageVisible = true;
			Percentage = 0;
			Text = "Action is starting";
			Service.CreateTask<WebSocketService>(LongTaskAsync);
		}

		public async Task LongTaskAsync(WebSocketService webSocketService, CancellationToken cancellationToken, string taskId)
		{
			for (int i = 1; i < 500; ++i)
			{
				await Task.Delay(20);
				cancellationToken.ThrowIfCancellationRequested();
				if (i == 50)
				{
					await webSocketService.SendSyncRequestToClient(taskId);
				}

				await webSocketService.ChangeViewModelForCurrentConnectionAsync((ServerEventsViewModel changes) =>
				{
					changes.Percentage = i;
				});
				await Task.Delay(20);
			}


			await webSocketService.ChangeViewModelForCurrentConnectionAsync((ServerEventsViewModel changes) =>
			{
				changes.Text = "Task is Complete";
				changes.IsPercentageVisible = false;
			});
		}


		public override Task PreRender()
		{
			Service.SaveCurrentState();
			return base.PreRender();
		}
	}
}