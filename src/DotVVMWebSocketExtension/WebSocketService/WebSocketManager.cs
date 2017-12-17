using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Microsoft.AspNetCore.Http;

namespace DotVVMWebSocketExtension.WebSocketService
{
	/// <summary>
	/// service that stores SocketList and LongRunningTask list and operations on them
	/// </summary>
	public class WebSocketManager
	{
		#region PropsAndConstructor

		public ConcurrentDictionary<string, Connection> Connections { get; }

		public ConcurrentDictionary<string, HashSet<WebSocketTask>> TaskList { get; }

		public ConcurrentDictionary<Type, PathString> WebSocketPaths { get; set; }


		/// <summary>
		/// Initializes a new instance of the <see cref="WebSocketManager"/> class.
		/// </summary>
		public WebSocketManager()
		{
			Connections = new ConcurrentDictionary<string, Connection>();
			TaskList = new ConcurrentDictionary<string, HashSet<WebSocketTask>>();
			WebSocketPaths = new ConcurrentDictionary<Type, PathString>();
		}

		#endregion

		#region ConnectionManagement

		/// <summary>
		/// Returns WebSocketManager Object with ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Connection GetConnetionById(string id)
		{
			Connections.TryGetValue(id, out var connection);
			return connection;
		}

		/// <summary>
		/// Returns Socket Id of given WebSocketManager object
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <returns></returns>
		public string GetConnectionId(WebSocket socket) => Connections.FirstOrDefault(p => p.Value.Socket == socket).Key;

		public string GetConnectionId(Connection connection) => GetConnectionId(connection.Socket);

		/// <summary>
		/// Creates new GUID and calls AddConnection() with that GUID as string
		/// </summary>
		/// <param name="connection"></param>
		/// <returns></returns>
		public string AddConnection(Connection connection)
		{
			var guid = Guid.NewGuid().ToString();

			return AddConnection(connection, guid);
		}

		/// <summary>
		/// Adds the socket to the list with given guid string
		/// </summary>
		/// <param name="connection"></param>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public string AddConnection(Connection connection, string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				return Connections.TryAdd(id, connection) ? id : null;
			}
			return null;
		}

		/// <summary>
		/// Removes the socket from list and closes connectionId normally
		/// </summary>
		/// <param name="connectionId">The socket identifier.</param>
		/// <returns></returns>
		public void RemoveConnection(string connectionId)
		{
			Connections.TryRemove(connectionId, out var connection);

			connection.Dispose();
		}

		/// <summary>
		/// Gets Id of websocket and calls RemoveConnection()
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <returns></returns>
		public void RemoveConnection(WebSocket socket) => RemoveConnection(GetConnectionId(socket));

		public void RemoveConnection(Connection connection) => RemoveConnection(GetConnectionId(connection));

		#endregion

		#region TaskManagement

		/// <summary>
		/// Adds the task to the list with connectionId id and cancellation token
		/// </summary>
		/// <param name="connectionId">The socket identifier.</param>
		/// <param name="token">The token.</param>
		public string AddTask(string connectionId, CancellationTokenSource token)
		{
			var taskId = Guid.NewGuid().ToString();
			if (TaskList.ContainsKey(connectionId))
			{
				if (TaskList.TryGetValue(connectionId, out var set))
				{
					set.Add(new WebSocketTask
					{
						CancellationTokenSource = token,
						TaskId = taskId,
						ConnectionId = connectionId,
						TaskCompletion = new TaskCompletionSource<bool>()
					});
				}
			}
			else
			{
				TaskList.TryAdd(connectionId,
					new HashSet<WebSocketTask>
					{
						new WebSocketTask
						{
							CancellationTokenSource = token,
							TaskId = taskId,
							ConnectionId = connectionId,
							TaskCompletion = new TaskCompletionSource<bool>()
						}

					});
			}
			return taskId;
		}


		/// <summary>
		/// Stops the task with identifier.
		/// </summary>
		/// <param name="taskId">The task identifier.</param>
		public void StopTaskWithId(string taskId)
		{
			var task = TaskList.SelectMany(s => s.Value).FirstOrDefault(t => t.TaskId == taskId);
			task?.CancellationTokenSource.Cancel();
		}

		/// <summary>
		/// Stops all tasks for connection.
		/// </summary>
		/// <param name="connectionId">The socket identifier.</param>
		public void StopAllTasksForConnection(string connectionId)
		{
			TaskList.TryGetValue(connectionId, out var set);
			set?.ToList().ForEach(s => s.CancellationTokenSource.Cancel());
			set?.Clear();
		}

		public void StopAllTasksForConnection(WebSocket socket) => StopAllTasksForConnection(GetConnectionId(socket));

		#endregion
	}
}