using DotVVM.Framework.Hosting;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketService
	{
		#region Properties&Constructor

		protected readonly WebSocketManager WebSocketManager;
		protected readonly IDotvvmRequestContext Context;
		protected readonly WebSocketViewModelSerializer Serializer;

		public string ConnectionId { get; set; }
		public string CurrentTaskId { get; set; }
		public string Path { get; set; }

		public WebSocketService(WebSocketManager webSocketManager, WebSocketViewModelSerializer serializer,
			IDotvvmRequestContext context)
		{
			WebSocketManager = webSocketManager;
			Serializer = serializer;
			Context = context;
			webSocketManager.WebSocketPaths.TryGetValue(GetType(), out var socketPath);
			Path = socketPath.Value;
		}

		#endregion

		#region Connect&Disconnect

		public virtual async Task OnConnected(WebSocket socket)
		{
			ConnectionId = WebSocketManager.AddConnection(new Connection {Socket = socket});
			await SendMessageToClientAsync(socket,
				JsonConvert.SerializeObject(new {socketId = ConnectionId, action = "webSocketInit"}, Formatting.None));
		}

		public virtual void OnDisconnected(Connection connection, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
			string statusString = "Closed Peacefully")
		{
			WebSocketManager.StopAllTasksForSocket(connection.Socket);

			WebSocketManager.RemoveConnection(connection);
			connection.Dispose(status, statusString);
		}

		public virtual void OnDisconnected(WebSocket socket, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
			string statusString = "Closed Peacefully")
		{
			WebSocketManager.StopAllTasksForSocket(socket);
			WebSocketManager.RemoveConnection(socket);
			socket.CloseAsync(status, statusString, CancellationToken.None);
			socket.Dispose();
		}

		#endregion

		public async Task ReceiveViewModelAsync(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			Console.WriteLine(WebSocketManager.TaskList.Count);
			var o = JsonConvert.DeserializeObject(message);
			//TODO taskmanagement

			await SendMessageToClientAsync(socket,
				JsonConvert.SerializeObject(
					new
					{
						action = "pong",
						message = $"Your Message was recieved, socketid&{WebSocketManager.GetConnectionId(socket)}, message: &{message}"
					}, Formatting.None)
			);
		}

		#region Send&Update ViewModel

		protected async Task SendMessageToClientAsync(WebSocket socket, string message)
		{
			if (socket?.State == WebSocketState.Open)
			{
				await socket.SendAsync(new ArraySegment<byte>(
					Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}

		protected async Task SendMessageToClientAsync(string socketId, string message) =>
			await SendMessageToClientAsync(WebSocketManager.GetConnetionById(socketId).Socket, message);

		protected async Task SendMessageToClientAsync(string message) =>
			await SendMessageToClientAsync(WebSocketManager.GetConnetionById(ConnectionId).Socket, message);

		protected async Task SendMessageToAllAsync(string message)
		{
			foreach (var connection in WebSocketManager.Connections)
			{
				await SendMessageToClientAsync(connection.Value.Socket, message);
			}
		}

		#endregion

		public async Task ChangeViewModelForCurrentConnection()
		{
			var connection = WebSocketManager.GetConnetionById(ConnectionId);

			Serializer.BuildViewModel(connection.ViewModelState);
			var serializedString = Serializer.SerializeViewModel(connection.ViewModelState);
			try
			{
				await SendMessageToClientAsync(serializedString);
			}
			catch (WebSocketException e)
			{
				connection.Dispose(WebSocketCloseStatus.InternalServerError, "server error");
				OnDisconnected(connection);
				Console.WriteLine(e);
			}
		}

		public async Task ChangeViewModelForConnectionsAsync<T>(Action<T> action, List<string> connectionIdList)
		{
			foreach (var connectionId in connectionIdList)
			{
				if (ConnectionId == connectionId) continue;
				var connection = WebSocketManager.GetConnetionById(connectionId);
				if (connection == null) continue;
				action.Invoke((T) connection.ViewModelState.LastSentViewModel);
				Serializer.BuildViewModel(connection.ViewModelState);

				var serializedString = Serializer.SerializeViewModel(connection.ViewModelState);

				try
				{
					await SendMessageToClientAsync(connectionId, serializedString);
				}
				catch (WebSocketException e)
				{
					OnDisconnected(connection, WebSocketCloseStatus.InternalServerError, "server error");
					Console.WriteLine(e);
				}
			}
		}


		#region TaskManagement

		public string CreateAndRunTask<T>(Func<T, CancellationToken, Task> func)
		{
			var tokenSource = new CancellationTokenSource();
			var connection = WebSocketManager.GetConnetionById(ConnectionId);
			var lastSentViewModel = (T) Context.ViewModel;

			connection.ViewModelState.LastSentViewModel = lastSentViewModel;

			return WebSocketManager.AddTask(
				func.Invoke(lastSentViewModel, tokenSource.Token),
//					.ContinueWith(s => StopTask(), tokenSource.Token),
				ConnectionId,
				tokenSource);
		}

		public void StopTask()
		{
			WebSocketManager.StopAllTasksForSocket(ConnectionId);
		}

		#endregion

		public void SaveCurrentState()
		{
			if (ConnectionId == null) return;

			var connection = WebSocketManager.GetConnetionById(ConnectionId);

			connection.ViewModelState.CsrfToken = Context.CsrfToken;
			connection.ViewModelState.LastSentViewModel = Context.ViewModel;
			Serializer.BuildViewModel(connection.ViewModelState);
			connection.ViewModelState.LastSentViewModelJson = connection.ViewModelState.ChangedViewModelJson;
		}
	}

}