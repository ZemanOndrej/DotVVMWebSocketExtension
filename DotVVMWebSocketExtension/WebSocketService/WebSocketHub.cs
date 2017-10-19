using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel;
using DotVVM.Framework.ViewModel.Serialization;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public abstract class WebSocketHub
	{
		protected readonly WebSocketManagerService WebSocketManagerService;
		private readonly IViewModelSerializer serializer;

		public WebSocketHub(WebSocketManagerService webSocketManagerService, IViewModelSerializer serializer)
		{
			this.WebSocketManagerService = webSocketManagerService;
			this.serializer = serializer;
		}

		public virtual async Task OnConnected(WebSocket socket)
		{
			WebSocketManagerService.AddSocket(socket);
		}

		public virtual async Task OnDisconnected(WebSocket socket)
		{
			await WebSocketManagerService.RemoveSocket(socket);
		}

		public async Task SendMessageAsync(WebSocket socket, string message)
		{
			if (socket.State == WebSocketState.Open)
			{
				await socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true,
					CancellationToken.None);
			}
		}

		public async Task SendMessageToAllAsync(string message)
		{
			foreach (var pair in WebSocketManagerService.Sockets)
			{
				await SendMessageAsync(pair.Value, message);
			}
		}

		public async Task UpdateViewModelOnClient( IDotvvmRequestContext context)
		{

			serializer.BuildViewModel(context);
			await SendMessageToAllAsync(serializer.SerializeViewModel(context));
		}

		public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, string message);
	}
}