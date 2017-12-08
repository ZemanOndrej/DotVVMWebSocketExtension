using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;

namespace DotVVMWebSocketExtension.WebSocketService
{
	/// <summary>
	/// service that stores SocketList and LongRunningTask list and operations on them
	/// </summary>
	public class WebSocketManagerService
	{
		#region PropsAndConstructor

		public ConcurrentDictionary<string, Connection> Connections { get; }


		public ConcurrentDictionary<string, HashSet<WebSocketTask>> TaskList { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WebSocketManagerService"/> class.
		/// </summary>
		public WebSocketManagerService()
		{
			Connections = new ConcurrentDictionary<string, Connection>();
			TaskList = new ConcurrentDictionary<string, HashSet<WebSocketTask>>();
		}

		#endregion

		#region SocketManagement

		/// <summary>
		/// Returns WebSocket Object with ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public Connection GetConnetionById(string id)
		{
			Connections.TryGetValue(id, out var connection);
			return connection;
		}

		/// <summary>
		/// Returns Socket Id of given WebSocket object
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


		public string AddContext(IDotvvmRequestContext context, string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				return Connections.TryAdd(id, new Connection(context)) ? id : null;
			}
			return null;
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

		public void AddSocketToConnection(string connectionId, WebSocket socket)
		{
			GetConnetionById(connectionId).Socket=socket;
		}

		/// <summary>
		/// Removes the socket from list and closes connection normally
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
		/// Adds the task to the list with websocket id and cancellation token
		/// </summary>
		/// <param name="connectionId">The socket identifier.</param>
		/// <param name="task">The task.</param>
		/// <param name="token">The token.</param>
		/// <param name="context">Context of HTTP request</param>
		public string AddTask(Task task, string connectionId, CancellationTokenSource token, IDotvvmRequestContext context)
		{
			var taskId = Guid.NewGuid().ToString();
			if (TaskList.ContainsKey(connectionId))
			{
				if (TaskList.TryGetValue(connectionId, out var set))
				{
					set.Add(new WebSocketTask(task, taskId, token, context));
				}
			}
			else
			{
				TaskList.TryAdd(connectionId,
					new HashSet<WebSocketTask>
					{
						new WebSocketTask(task, taskId, token, context)
					});
			}
			return taskId;
		}

		public void StopTaskWithId(string taskId, string socketId)
		{
			TaskList.TryGetValue(socketId, out var set);
			if (set == null)
			{
				return;
			}
			var task = set.First(t => t.TaskId == taskId);

			task.CancellationTokenSource.Cancel();
			set.Remove(task);
		}

		public void StopAllTasksForSocket(string socketId)
		{
			TaskList.TryGetValue(socketId, out var set);
			set?.ToList().ForEach(s => s.CancellationTokenSource.Cancel());
			set?.Clear();
		}

		public void StopAllTasksForSocket(WebSocket socket) => StopAllTasksForSocket(GetConnectionId(socket));

		#endregion


	}
}