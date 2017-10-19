using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1
{
	public class MyEventHub : WebSocketHub
	{
		public override async Task OnConnected(WebSocket socket)
		{
			await base.OnConnected(socket);
			await SendMessageToAllAsync($" ${WebSocketManagerService.GetSocketId(socket)} user has connected");
		}


		public event EventHandler ProgressReport;

		public async Task PerformScanAsync(IProgress<ProgressReportEventArgs> progress, WebSocket socket)
		{
			for (int i = 0; i < 10; i++)
			{
				await Task.Delay(50);
				progress?.Report(new ProgressReportEventArgs {ReportValue = i, Socket = socket});
			}
		}


		public override async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, string message)
		{
		}

		public MyEventHub(WebSocketManagerService webSocketManagerService, IViewModelSerializer viewModelSerializer)
			: base(webSocketManagerService, viewModelSerializer)
		{
		}
	}

	public class ProgressReportEventArgs : EventArgs
	{
		public WebSocket Socket { get; set; }
		public int ReportValue { get; set; }
	}
}