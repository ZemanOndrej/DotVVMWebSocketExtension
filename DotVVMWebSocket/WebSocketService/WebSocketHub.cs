using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public abstract class WebSocketHub
	{
		protected WebSocketManagerService WebSocketManagerService { get; set; }

		protected WebSocketHub(WebSocketManagerService webSocketManagerService)
		{
			WebSocketManagerService = webSocketManagerService;
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

		public abstract Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, string message);
	}
}