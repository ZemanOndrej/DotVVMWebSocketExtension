﻿using System;
using System.IO;
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
		private WebSocketFacade Facade { get; }

		public WebSocketMiddleware(RequestDelegate next, WebSocketFacade facade)
		{
			_next = next;
			Facade = facade;
		}

		public async Task Invoke(HttpContext context)
		{
			if (!context.WebSockets.IsWebSocketRequest)
			{
				await _next(context);
				return;
			}
			var socket = await context.WebSockets.AcceptWebSocketAsync();

			await Facade.OnConnected(socket);
			await HandleWebSocketCommunication(socket);
			await _next(context);
		}

		private async Task HandleWebSocketCommunication(WebSocket socket)
		{
			var buffer = new ArraySegment<byte>(new byte[4 * 1024]);
			while (socket.State == WebSocketState.Open)
			{
				using (var ms = new MemoryStream())
				{
					WebSocketReceiveResult result;
					do
					{
						result = await socket.ReceiveAsync(buffer, CancellationToken.None);
						ms.Write(buffer.Array, buffer.Offset, result.Count);

					} while (!result.EndOfMessage);

					ms.Seek(0, SeekOrigin.Begin);

					if (result.MessageType == WebSocketMessageType.Text)
					{
						string messageResult;
						using (var reader = new StreamReader(ms, Encoding.UTF8))
						{
							messageResult = await reader.ReadToEndAsync();
						}
						await Facade.ReceiveMessageAsync(socket, result, messageResult);
					}
					else if (result.MessageType == WebSocketMessageType.Close)
					{
						await Facade.OnDisconnected(socket);
					}

				}
			}
		}
	}
}