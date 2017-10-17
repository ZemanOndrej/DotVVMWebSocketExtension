using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1.ViewModels
{
	public class EventViewModel : DotvvmViewModelBase
	{
		private readonly MyEventHub hub;
		private readonly WebSocketManagerService wsService;

		public string SocketId { get; set; }

		private  Progress<ProgressReportEventArgs> progress = new Progress<ProgressReportEventArgs>();

		public EventViewModel(MyEventHub hub, WebSocketManagerService wsService)
		{
			this.hub = hub;
			this.wsService = wsService;
			SocketId = "tmpguid";

		}


		public async Task EventStart()
		{
			var socket = wsService.GetWebSocketById(SocketId);

			await hub.SendMessageAsync(socket, "o shit ayy lmao");


		}
	}
}