﻿using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace DotVVMWebSocketExtension.WebSocketService
{
	/// <summary>
	/// WebSocket Middleware class that handles websocket connection
	/// </summary>
	public class WebSocketMiddleware
	{
		private readonly RequestDelegate _next;
		private WebSocketService Service { get; }

		public WebSocketMiddleware(RequestDelegate next, WebSocketService service)
		{
			_next = next;
			Service = service;
		}

		/// <summary>
		/// Invoked after previous middleware called it.
		/// Accepts WebSocket connection and calls OnConnected after connection is established
		/// </summary>
		/// <param name="context">The context.</param>
		/// <returns></returns>
		public async Task Invoke(HttpContext context)
		{
			if (!context.WebSockets.IsWebSocketRequest)
			{
				await _next(context);
				return;
			}
			var socket = await context.WebSockets.AcceptWebSocketAsync();

			await Service.OnConnected(socket);
			await HandleWebSocketCommunication(socket);
			await _next(context);
		}


		/// <summary>
		/// Handles the web socket communication until conenction is open.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <returns></returns>
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
						Service.ReceiveViewModel(socket, result, messageResult);
					}
					else if (result.MessageType == WebSocketMessageType.Close)
					{
						Service.OnDisconnected(socket);
					}

				}
			}
		}
	}
}