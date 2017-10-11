using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1
{
	public class MyEventHub : WebSocketHub
	{
		public MyEventHub(WebSocketManagerService webSocketManagerService) : base(webSocketManagerService)
		{
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
	}

	public class ProgressReportEventArgs : EventArgs
	{
		public WebSocket Socket { get; set; }
		public int ReportValue { get; set; }
	}
}