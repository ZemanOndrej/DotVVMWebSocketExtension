using System;
using System.Threading;
using System.Threading.Tasks;
using DotvvmApplication1.Hubs;
using DotVVM.Framework.ViewModel;

namespace DotvvmApplication1.ViewModels
{
	public class ServerEventsViewModel : MasterpageViewModel
	{
		public ServerEventHub Hub { get; set; }// TODO NAME OF HUB VARIABLE
		public string Text { get; set; }

		public ServerEventsViewModel(ServerEventHub hub)
		{
			Hub = hub;
			Text = "No action";
		}

		public void StartLongTask()
		{

			Hub.CreateAndRunTask(LongTaskAsync, new Progress<string>(async value =>
			{
				Text = value;
				await Hub.UpdateCurrentViewModelOnClient();
			}));

		}

		public async Task LongTaskAsync(IProgress<string> progressHandler, CancellationToken token)
		{
			progressHandler.Report("Task is starting");

			for (int i = 0; i < 5; ++i)
			{
				 await Task.Delay(500);
									   
				token.ThrowIfCancellationRequested();

				progressHandler.Report("Stage " + i);
				Console.WriteLine("stage " + i);
				await Task.Delay(500);

			}
			progressHandler.Report("Task is Complete");
		}

		public void StopTask()
		{
			Hub.StopTask();
		}
	}
}