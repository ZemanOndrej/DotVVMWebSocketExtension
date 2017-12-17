using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
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
	public partial class WebSocketService
	{
		#region Properties&Constructor

		protected readonly WebSocketManager WebSocketManager;
		protected readonly IDotvvmRequestContext Context;
		protected readonly WebSocketViewModelSerializer Serializer;


		public string ConnectionId { get; set; }
		public string Path { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WebSocketService"/> class.
		/// </summary>
		/// <param name="webSocketManager">The web socket manager.</param>
		/// <param name="serializer">The serializer.</param>
		/// <param name="context">The request context.</param>
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


		/// <summary>
		/// Receives the ViewModel and updates stored ViewModel.
		/// </summary>
		/// <param name="socket">The socket that sent ViewModel</param>
		/// <param name="result">The result.</param>
		/// <param name="message">The message.</param>
		public void ReceiveViewModel(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			var taskId = Serializer.PopulateViewModel(WebSocketManager.GetConnetionById(ConnectionId).ViewModelState, message);

			var task = WebSocketManager.TaskList.SelectMany(s => s.Value).FirstOrDefault(s => s.TaskId == taskId);
			task?.TaskCompletion.SetResult(true);
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

		public string CreateTask<T>(Func<T, CancellationToken, string, Task> func) where T : WebSocketService
		{
			var tokenSource = new CancellationTokenSource();
			var connection = WebSocketManager.GetConnetionById(ConnectionId);

			connection.ViewModelState.LastSentViewModel = Context.ViewModel;

			var taskId = WebSocketManager.AddTask(
				ConnectionId,
				tokenSource
			);

			var task = WebSocketManager.TaskList.SelectMany(v => v.Value).FirstOrDefault(t => t.TaskId == taskId);
			if (task != null)
			{
				task.FunctionToInvoke = () =>
					func.Invoke((T) this, tokenSource.Token, taskId).ContinueWith(s => StopTask(taskId), tokenSource.Token);
			}

			return taskId;
		}

		public void StopTask(string taskId)
		{
			WebSocketManager.StopTaskWithId(taskId, ConnectionId);
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

		public async Task SendSyncRequestToClient(string taskId)
		{
			await SendMessageToClientAsync(
				JsonConvert.SerializeObject(new {action = "viewModelSynchronizationRequest", taskId}, Formatting.None));

			var task = WebSocketManager.TaskList.SelectMany(s => s.Value).FirstOrDefault(s => s.TaskId == taskId);
			if (task != null)
			{
				await task.TaskCompletion.Task;
			}
		}
	}
}