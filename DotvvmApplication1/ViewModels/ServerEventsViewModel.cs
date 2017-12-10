﻿using System.Threading;
using System.Threading.Tasks;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1.ViewModels
{
	public class ServerEventsViewModel : MasterpageViewModel
	{
		public WebSocketFacade Hub { get; set; } // TODO NAME OF HUB VARIABLE
		public string Text { get; set; }
		public long Percentage { get; set; }
		public bool IsPercentageVisible { get; set; }


		public ServerEventsViewModel(WebSocketFacade hub)
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
			Hub.CreateAndRunTask(LongTaskAsync);
		}

		public async Task LongTaskAsync( CancellationToken token)
		{
			
//			for (int i = 1; i < 101; ++i)
//			{
//				await Task.Delay(10);
//
//				token.ThrowIfCancellationRequested();
////				if (i==50)
////				{
////					await hub.UpdateViewModelInTaskFromCurrentClientAsync();
////				}
//
//				Percentage = i;
//				await Hub.UpdateViewModelOnCurrentClientAsync();
//				await Task.Delay(10);
//			}
//			Text = "Task is Complete";
//			IsPercentageVisible = false;
//			await Hub.UpdateViewModelOnCurrentClientAsync();
		}

		public void StopTask()
		{
			Hub.StopTask();
		}
	}
}