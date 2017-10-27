using System;
using System.Threading;
using System.Threading.Tasks;
using DotvvmApplication1.Hubs;
using DotVVM.Framework.ViewModel;

namespace DotvvmApplication1.ViewModels
{
	public class ServerEventsViewModel : MasterpageViewModel
	{
		public ServerEventHub Hub { get; set; }
		public string Text { get; set; }

		public ServerEventsViewModel(ServerEventHub hub)
		{
			Hub = hub;
			Text = "No action";
		}

		public void StartLongTask()
		{
			Text = "event is starting";

			Task.Run(() => LongTaskAsync( new Progress<string>(async value =>
			{
				Text = value;
				await Hub.UpdateCurrentViewModelOnClient();
			})));
		}

		public void LongTaskAsync(IProgress<string> progressHandler)
		{
			for (int i = 0; i < 100; ++i)
			{

				Thread.Sleep(50);

				progressHandler.Report("Stage " + i);
				Console.WriteLine("stage " + i);

				Thread.Sleep(50);
			}
			progressHandler.Report("Task is Complete");
		}
	}
}