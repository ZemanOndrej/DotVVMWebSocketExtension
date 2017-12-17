using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using Newtonsoft.Json;

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

		/// <summary>
		/// Called when connection is established and communication is available.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <returns></returns>
		public virtual async Task OnConnected(WebSocket socket)
		{
			ConnectionId = WebSocketManager.AddConnection(new Connection {Socket = socket});
			await SendMessageToClientAsync(socket,
				JsonConvert.SerializeObject(new {socketId = ConnectionId, action = WebSocketRequestType.WebSocketInit}, Formatting.None));
		}

		/// <summary>
		/// Called when you want to disconnect connection.
		/// </summary>
		/// <param name="connection">The connection.</param>
		/// <param name="status">The status.</param>
		/// <param name="statusString">The status string.</param>
		public virtual void OnDisconnected(Connection connection,
			WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
			string statusString = "Closed Peacefully")
		{
			WebSocketManager.StopAllTasksForConnection(connection.Socket);

			WebSocketManager.RemoveConnection(connection);
			connection.Dispose(status, statusString);
		}

		/// <summary>
		/// Called when socket is disconnected by client.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="status">The status.</param>
		/// <param name="statusString">The status string.</param>
		public virtual void OnDisconnected(WebSocket socket, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
			string statusString = "Closed Peacefully")
		{
			WebSocketManager.StopAllTasksForConnection(socket);
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
		internal void ReceiveViewModel(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			var taskId = Serializer.PopulateViewModel(WebSocketManager.GetConnetionById(ConnectionId).ViewModelState, message);

			var task = WebSocketManager.TaskList.SelectMany(s => s.Value).FirstOrDefault(s => s.TaskId == taskId);
			task?.TaskCompletion.SetResult(true);
		}

		#region SendStringMessage

		/// <summary>
		/// Sends the message to client asynchronous.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="message">The message.</param>
		/// <returns></returns>
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

		#endregion

		/// <summary>
		/// Changes the view model for connections asynchronous.
		/// Changes ViewModels for connections specified
		/// sends changes to connected clients
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action">The action.</param>
		/// <param name="connectionIdList">The connection identifier list.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Changes the view model for current connection.
		/// When in task you can change ViewModel on current client and send changes to it
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="action">The action.</param>
		/// <returns></returns>
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

		/// <summary>
		/// Creates the task that will be invoked after request is processed in filter.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func">The function.</param>
		/// <returns></returns>
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


		/// <summary>
		/// Stops the task with id for current connection.
		/// </summary>
		/// <param name="taskId">The task identifier.</param>
		public void StopTask(string taskId)
		{
			WebSocketManager.StopTaskWithId(taskId);
		}

		#endregion

		/// <summary>
		/// Saves the state of the current ViewModel.
		/// Should be called in PreRender method in ViewModel
		/// </summary>
		public void SaveCurrentState()
		{
			if (ConnectionId == null) return;

			var connection = WebSocketManager.GetConnetionById(ConnectionId);

			connection.ViewModelState.CsrfToken = Context.CsrfToken;
			connection.ViewModelState.LastSentViewModel = Context.ViewModel;
			Serializer.BuildViewModel(connection.ViewModelState);
			connection.ViewModelState.LastSentViewModelJson = connection.ViewModelState.ChangedViewModelJson;
		}

		/// <summary>
		/// Sends the synchronize request to client.
		/// Used in task to update the state of current ViewModel stored on server
		/// </summary>
		/// <param name="taskId">The task identifier.</param>
		/// <returns></returns>
		public async Task SendSyncRequestToClient(string taskId)
		{
			await SendMessageToClientAsync(
				JsonConvert.SerializeObject(new {action = WebSocketRequestType.WebSocketViewModelSync, taskId}, Formatting.None));

			var task = WebSocketManager.TaskList.SelectMany(s => s.Value).FirstOrDefault(s => s.TaskId == taskId);
			if (task != null)
			{
				await task.TaskCompletion.Task;
			}
		}
	}
}