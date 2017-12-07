﻿using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketHub
	{
		#region PropertiesAndConstructor

		protected readonly WebSocketManagerService WebSocketService;
		protected readonly IDotvvmRequestContext Context;
		protected readonly WebSocketViewModelSerializer Serializer;

		public string CurrentSocketId { get; set; }
		public string CurrentTaskId { get; set; }

		public WebSocketHub(WebSocketManagerService webSocketService, WebSocketViewModelSerializer serializer,
			IDotvvmRequestContext context)
		{
			WebSocketService = webSocketService;
			Serializer = serializer;
			Context = context;
		}

		#endregion

		#region Connect/Disconnect

		public virtual async Task OnConnected(WebSocket socket)
		{
			WebSocketService.AddSocket(socket);
			CurrentSocketId = WebSocketService.GetSocketId(socket);
			await SendMessageToSocketAsync(socket,
				JsonConvert.SerializeObject(new {socketId = CurrentSocketId, action = "webSocketInit"}, Formatting.None));
		}

		public virtual async Task OnDisconnected(WebSocket socket)
		{
			WebSocketService.StopAllTasksForSocket(socket);

			await WebSocketService.RemoveSocket(socket);
		}

		#endregion

		public virtual async Task ReceiveMessageAsync(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			Console.WriteLine(WebSocketService.TaskList.Count);
			var o = JsonConvert.DeserializeObject(message);
			//TODO taskmanagement

			await SendMessageToSocketAsync(socket,
				JsonConvert.SerializeObject(
					new
					{
						action = "pong",
						message = $"Your Message was recieved, socketid&{WebSocketService.GetSocketId(socket)}, message: &{message}"
					}, Formatting.None)
			);
		}


		#region Send/Update ViewModel

		public async Task SendMessageToSocketAsync(WebSocket socket, string message)
		{
			if (socket?.State == WebSocketState.Open)
			{
				await socket.SendAsync(new ArraySegment<byte>(
					Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}

		public async Task SendMessageToSocketAsync(string socketId, string message) =>
			await SendMessageToSocketAsync(WebSocketService.GetSocketById(socketId), message);


		public async Task SendMessageToClientAsync(string message) =>
			await SendMessageToSocketAsync(WebSocketService.GetSocketById(CurrentSocketId), message);

		public async Task SendMessageToAllAsync(string message)
		{
			foreach (var pair in WebSocketService.Sockets)
			{
				await SendMessageToSocketAsync(pair.Value, message);
			}
		}





		public async Task UpdateViewModelOnClient()
		{
			if (Context != null)
			{
				Serializer.BuildViewModel(Context);
				var serializedString = Serializer.SerializeViewModel(Context);
				try
				{
					await SendMessageToClientAsync(serializedString);
				}
				catch (WebSocketException e)
				{
					var socket = WebSocketService.GetSocketById(CurrentSocketId);
					await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "server error", CancellationToken.None);
					await OnDisconnected(socket);
					Console.WriteLine(e);
				}
			}
		}

		public async Task SyncViewModelForSockets(List<string> socketIdList)
		{
			foreach (var socketId in socketIdList)
			{
				if (socketId != CurrentSocketId)
				{
					if (Context != null)
					{
						Serializer.BuildViewModel(Context);
						var serializedString = Serializer.SerializeViewModel(Context);
						try
						{
							await SendMessageToSocketAsync(socketId,serializedString);
						}
						catch (WebSocketException e)
						{
							var socket = WebSocketService.GetSocketById(socketId);
							await socket.CloseAsync(WebSocketCloseStatus.InternalServerError, "server error", CancellationToken.None);
							await OnDisconnected(socket);
							Console.WriteLine(e);
						}
					}
				}
			}
		}

		#endregion


		#region TaskManagement

		public string CreateAndRunTask(Func<CancellationToken, Task> func)
		{
			var tokenSource = new CancellationTokenSource();
			return WebSocketService.AddTask(
				func.Invoke(tokenSource.Token).ContinueWith(s => StopTask(), tokenSource.Token),
				CurrentSocketId,
				tokenSource, Context);
		}

		public void StopTask()
		{
			WebSocketService.StopAllTasksForSocket(CurrentSocketId);
		}

		#endregion

		public async Task GetViewModelFromClientAsync()
		{
			await SendMessageToSocketAsync(CurrentSocketId,
				JsonConvert.SerializeObject(new {action = "viewModelSynchronizationRequest",taskId=CurrentTaskId}, Formatting.None));
		}


	}
}