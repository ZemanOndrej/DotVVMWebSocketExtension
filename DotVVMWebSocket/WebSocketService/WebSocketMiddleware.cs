using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketMiddleware
	{
		private readonly RequestDelegate _next;
		private WebSocketHub Hub { get; }

		public WebSocketMiddleware(RequestDelegate next,
			WebSocketHub hub)
		{
			_next = next;
			Hub = hub;
		}

		public async Task Invoke(HttpContext context)
		{
			if (!context.WebSockets.IsWebSocketRequest)
			{
				return;
			}

			var socket = await context.WebSockets.AcceptWebSocketAsync();
			await Hub.OnConnected(socket);
			await Receive(socket);
//			await _next(context);
		}

		private async Task Receive(WebSocket socket)
		{
			var buffer = new byte[1024 * 4];

			while (socket.State == WebSocketState.Open)
			{
				var result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

				await HandleMessage(socket, result, Encoding.UTF8.GetString(buffer).TrimEnd('\0'));
			}
		}

		private async Task HandleMessage(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			if (result.MessageType == WebSocketMessageType.Text)
			{
				await Hub.ReceiveAsync(socket, result, message);
			}

			if (result.MessageType == WebSocketMessageType.Close)
			{
				await Hub.OnDisconnected(socket);
			}
		}
	}
}