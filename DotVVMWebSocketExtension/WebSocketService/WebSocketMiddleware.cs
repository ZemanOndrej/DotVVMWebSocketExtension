using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using DotVVM.Framework.Hosting.Middlewares;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketMiddleware : DotvvmMiddlewareBase
	{
		private readonly RequestDelegate _next;
		private WebSocketHub Hub { get; }

		public WebSocketMiddleware(RequestDelegate next, WebSocketHub hub)
		{
			_next = next;
			Hub = hub;
		}

		public async Task Invoke(HttpContext context)
		{
			if (!context.WebSockets.IsWebSocketRequest)
			{
				await _next(context);
				return;
			}
			WebSocket socket;

			if (context.WebSockets.WebSocketRequestedProtocols.Count > 0)
			{
				socket = await context.WebSockets.AcceptWebSocketAsync(context.WebSockets.WebSocketRequestedProtocols[0]);
			}
			else
			{
				socket = await context.WebSockets.AcceptWebSocketAsync();
			}
			await Hub.OnConnected(socket);
			await Receive(socket);
			await _next(context);
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
			else if (result.MessageType == WebSocketMessageType.Close)
			{
				await Hub.OnDisconnected(socket);
			}
		}
	}
}