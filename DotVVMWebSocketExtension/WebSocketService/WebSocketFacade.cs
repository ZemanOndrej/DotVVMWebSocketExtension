using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;


namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketFacade
	{
		#region PropertiesAndConstructor

		protected readonly WebSocketManagerService WebSocketService;
		protected readonly IDotvvmRequestContext Context;
		protected readonly WebSocketViewModelSerializer Serializer;

		public string CurrentSocketId { get; set; }
		public string CurrentTaskId { get; set; }

		public WebSocketFacade(WebSocketManagerService webSocketService, WebSocketViewModelSerializer serializer,
			IDotvvmRequestContext context)
		{
			WebSocketService = webSocketService;
			Serializer = serializer;
			Context = context;
			if (context != null && CurrentSocketId == null)
			{
				CurrentSocketId = WebSocketService.AddConnection(new Connection(context));
			}
		}

		#endregion

		#region Connect/Disconnect

		public void OnConnected(WebSocket socket, string connectionId)
		{
			WebSocketService.AddSocketToConnection(connectionId, socket);
		}

		public void OnDisconnected(Connection connection, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
			string statusString="Closed Peacefully")
		{
			WebSocketService.StopAllTasksForSocket(connection.Socket);

			WebSocketService.RemoveConnection(connection);
			connection.Dispose(status,statusString);
		}
		public void OnDisconnected(WebSocket socket, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
			string statusString="Closed Peacefully")
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


		#region Send/Update ViewModel

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
			await SendMessageToSocketAsync(WebSocketService.GetConnetionById(CurrentSocketId).Socket, message);

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
				var serializedString = Serializer.SerializeViewModel(Context, WebSocketService.GetConnetionById(CurrentSocketId));
				try
				{
					await SendMessageToClientAsync(serializedString);
				}
				catch (WebSocketException e)
				{
					var connection = WebSocketService.GetConnetionById(CurrentSocketId);
					connection.Dispose(WebSocketCloseStatus.InternalServerError, "server error");
					OnDisconnected(connection);
					Console.WriteLine(e);
				}
			}
		}

		public async Task SyncViewModelForSocketsAsync(List<string> socketIdList)
		{
			foreach (var socketId in socketIdList)
			{
				if (socketId != CurrentSocketId)
				{
					if (Context != null)
					{
						Serializer.BuildViewModel(Context);
						var serializedString = Serializer.SerializeViewModel(Context,WebSocketService.GetConnetionById(CurrentSocketId));
						try
						{
							await SendMessageToSocketAsync(socketId, serializedString);
						}
						catch (WebSocketException e)
						{
							var connection = WebSocketService.GetConnetionById(socketId);
							OnDisconnected(connection, WebSocketCloseStatus.InternalServerError, "server error");
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

		public async Task UpdateViewModelInTaskFromCurrentClientAsync()
		{
//			await SendMessageToSocketAsync(CurrentSocketId,
//				JsonConvert.SerializeObject(new {action = "viewModelSynchronizationRequest",taskId=CurrentTaskId}, Formatting.None));
		}
	}
}