using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Remotion.Linq.Clauses;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketService
	{
		#region Properties&Constructor

		protected readonly WebSocketManager WebSocketManager;
		protected readonly IDotvvmRequestContext Context;
		protected readonly WebSocketViewModelSerializer Serializer;

		public string ConnectionId { get; set; }
		public string Path { get; set; }

//		private SemaphoreSlim signal = new SemaphoreSlim(0, 1);

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

		public virtual void OnDisconnected(Connection connection,
			WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
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

		public void ReceiveViewModel(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			Serializer.PopulateViewModel(WebSocketManager.GetConnetionById(ConnectionId).ViewModelState, message);
//			signal.Release();
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


		public async Task ChangeViewModelForConnectionsAsync<T>(Action<T> action, List<string> connectionIdList)
			where T : DotvvmViewModelBase
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

		public async Task ChangeViewModelForCurrentConnection<T>(Action<T> action) where T : DotvvmViewModelBase
		{
			var connection = WebSocketManager.GetConnetionById(ConnectionId);

			lock (connection)
			{
				action.Invoke((T) connection.ViewModelState.LastSentViewModel);

				Serializer.BuildViewModel(connection.ViewModelState);
			}
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

		#region TaskManagement

		public string CreateAndRunTask<T>(Func<T, CancellationToken, Task> func) where T : WebSocketService
		{
			var tokenSource = new CancellationTokenSource();
			var connection = WebSocketManager.GetConnetionById(ConnectionId);

			connection.ViewModelState.LastSentViewModel = Context.ViewModel;

			return WebSocketManager.AddTask(
				func.Invoke((T) this, tokenSource.Token),
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

		public async Task SendSyncReqeustToClient()
		{
			await SendMessageToClientAsync(
				JsonConvert.SerializeObject(new {action = "viewModelSynchronizationRequest"}, Formatting.None));

//			await signal.WaitAsync();
		}
	}
}