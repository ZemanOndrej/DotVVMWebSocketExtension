﻿using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketFacade
	{
		#region Properties&Constructor

		protected readonly WebSocketManagerService WebSocketService;
		protected readonly IDotvvmRequestContext Context;
		protected readonly WebSocketViewModelSerializer Serializer;

		public string ConnectionId { get; set; }
		public string CurrentTaskId { get; set; }

		public WebSocketFacade(WebSocketManagerService webSocketService, WebSocketViewModelSerializer serializer,
			IDotvvmRequestContext context)
		{
			WebSocketService = webSocketService;
			Serializer = serializer;
			Context = context;
		}

		#endregion

		#region Connect&Disconnect

		public async Task OnConnected(WebSocket socket)
		{
			ConnectionId = WebSocketService.AddConnection(new Connection {Socket = socket});
			await SendMessageToSocketAsync(socket,
				JsonConvert.SerializeObject(new {socketId = ConnectionId, action = "webSocketInit"}, Formatting.None));
		}

		public void OnDisconnected(Connection connection, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
			string statusString = "Closed Peacefully")
		{
			WebSocketService.StopAllTasksForSocket(connection.Socket);

			WebSocketService.RemoveConnection(connection);
			connection.Dispose(status, statusString);
		}

		public void OnDisconnected(WebSocket socket, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
			string statusString = "Closed Peacefully")
		{
			WebSocketService.StopAllTasksForSocket(socket);
			WebSocketService.RemoveConnection(socket);
			socket.CloseAsync(status, statusString, CancellationToken.None);
			socket.Dispose();
		}

		#endregion

		public async Task ReceiveMessageAsync(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			Console.WriteLine(WebSocketService.TaskList.Count);
			var o = JsonConvert.DeserializeObject(message);
			//TODO taskmanagement

			await SendMessageToSocketAsync(socket,
				JsonConvert.SerializeObject(
					new
					{
						action = "pong",
						message = $"Your Message was recieved, socketid&{WebSocketService.GetConnectionId(socket)}, message: &{message}"
					}, Formatting.None)
			);
		}

		#region Send&Update ViewModel

		protected async Task SendMessageToSocketAsync(WebSocket socket, string message)
		{
			if (socket?.State == WebSocketState.Open)
			{
				await socket.SendAsync(new ArraySegment<byte>(
					Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}

		protected async Task SendMessageToSocketAsync(string socketId, string message) =>
			await SendMessageToSocketAsync(WebSocketService.GetConnetionById(socketId).Socket, message);


		protected async Task SendMessageToClientAsync(string message) =>
			await SendMessageToSocketAsync(WebSocketService.GetConnetionById(ConnectionId).Socket, message);

		protected async Task SendMessageToAllAsync(string message)
		{
			foreach (var connection in WebSocketService.Connections)
			{
				await SendMessageToSocketAsync(connection.Value.Socket, message);
			}
		}


		public async Task UpdateViewModelOnCurrentClientAsync()
		{
			if (Context != null)
			{
				Serializer.BuildViewModel(Context);
				var serializedString = Serializer.SerializeViewModel(Context, WebSocketService.GetConnetionById(ConnectionId));
				try
				{
					await SendMessageToClientAsync(serializedString);
				}
				catch (WebSocketException e)
				{
					var connection = WebSocketService.GetConnetionById(ConnectionId);
					connection.Dispose(WebSocketCloseStatus.InternalServerError, "server error");
					OnDisconnected(connection);
					Console.WriteLine(e);
				}
			}
		}

		public async Task SyncViewModelForSocketsAsync(List<string> socketIdList)
		{
			var currentConnection = WebSocketService.GetConnetionById(ConnectionId);

			foreach (var socketId in socketIdList)
			{
				if (Context == null) continue;
				Serializer.BuildViewModel(Context);
				var connetionById = WebSocketService.GetConnetionById(socketId);
				if (connetionById == null) continue;
				var serializedString = Serializer.SerializeViewModel(Context, connetionById);

				try
				{
					if (ConnectionId != socketId)
					{
						await SendMessageToSocketAsync(socketId, serializedString);
					}
				}
				catch (WebSocketException e)
				{
					OnDisconnected(currentConnection, WebSocketCloseStatus.InternalServerError, "server error");
					Console.WriteLine(e);
				}
			}
//			currentConnection.LastSentViewModelJson.ViewModelJson = LastSentViewModelJson.ViewModelJson;
		}

		#endregion

		#region TaskManagement

		public string CreateAndRunTask(Func<CancellationToken, Task> func)
		{
			var tokenSource = new CancellationTokenSource();
			return WebSocketService.AddTask(
				func.Invoke(tokenSource.Token).ContinueWith(s => StopTask(), tokenSource.Token),
				ConnectionId,
				tokenSource, Context);
		}

		public void StopTask()
		{
			WebSocketService.StopAllTasksForSocket(ConnectionId);
		}

		#endregion

		public async Task UpdateViewModelInTaskFromCurrentClientAsync()
		{
			await SendMessageToSocketAsync(ConnectionId,
				JsonConvert.SerializeObject(new {action = "viewModelSynchronizationRequest"}, Formatting.None));
		}

		public void SaveContext()
		{
			if (ConnectionId == null) return;
			Serializer.BuildViewModel(Context);
			WebSocketService.GetConnetionById(ConnectionId).LastSentViewModelJson = Context.ViewModelJson;
		}
	}
}