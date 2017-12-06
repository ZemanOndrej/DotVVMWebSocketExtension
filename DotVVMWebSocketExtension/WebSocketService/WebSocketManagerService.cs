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

		public ConcurrentDictionary<string, WebSocket> Sockets { get; }


		public ConcurrentDictionary<string,
			HashSet<(Task Task, string TaskId, CancellationTokenSource CancellationTokenSource,IDotvvmRequestContext Context)>> TaskList { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WebSocketManagerService"/> class.
		/// </summary>
		public WebSocketManagerService()
		{
			Sockets = new ConcurrentDictionary<string, WebSocket>();
			TaskList = new ConcurrentDictionary<string, HashSet<(Task Task, string TaskId, CancellationTokenSource CancellationTokenSource, IDotvvmRequestContext Context)>>();
		}

		#endregion

		#region SocketManagement

		/// <summary>
		/// Returns WebSocket Object with ID
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		public WebSocket GetSocketById(string id)
		{
			Sockets.TryGetValue(id, out WebSocket ws);
			return ws;
		}

		/// <summary>
		/// Returns Socket Id of given WebSocket object
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <returns></returns>
		public string GetSocketId(WebSocket socket)
		{
			return Sockets.FirstOrDefault(p => p.Value == socket).Key;
		}

		/// <summary>
		/// Creates new GUID and calls AddSocket() with that GUID as string
		/// </summary>
		/// <param name="socket">The socket object</param>
		/// <returns></returns>
		public string AddSocket(WebSocket socket)
		{
			var guid = Guid.NewGuid().ToString();

			return AddSocket(socket, guid);
		}

		/// <summary>
		/// Adds the socket to the list with given guid string
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="id">The identifier.</param>
		/// <returns></returns>
		public string AddSocket(WebSocket socket, string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				return Sockets.TryAdd(id, socket) ? id : null;
			}
			return null;
		}

		/// <summary>
		/// Removes the socket from list and closes connection normally
		/// </summary>
		/// <param name="socketId">The socket identifier.</param>
		/// <returns></returns>
		public async Task RemoveSocket(string socketId)
		{
			Sockets.TryRemove(socketId, out var socket);

			await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed Peacefully", CancellationToken.None);
		}

		/// <summary>
		/// Gets Id of websocket and calls RemoveSocket()
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <returns></returns>
		public async Task RemoveSocket(WebSocket socket) => await RemoveSocket(GetSocketId(socket));

		#endregion

		#region TaskManagement

		/// <summary>
		/// Adds the task to the list with websocket id and cancellation token
		/// </summary>
		/// <param name="socketId">The socket identifier.</param>
		/// <param name="task">The task.</param>
		/// <param name="token">The token.</param>
		/// <param name="context">Context of HTTP request</param>
		public string AddTask( Task task,string socketId, CancellationTokenSource token, IDotvvmRequestContext context)
		{
			var taskId = Guid.NewGuid().ToString();
			if (TaskList.ContainsKey(socketId))
			{
				if (TaskList.TryGetValue(socketId, out var set))
				{
					set.Add((task,taskId, token, context));
				}
			}
			else
			{
				TaskList.TryAdd(socketId,
					new HashSet<(Task, string, CancellationTokenSource,IDotvvmRequestContext)>
					{
						(task, taskId, token,context)
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

		public void StopAllTasksForSocket(WebSocket socket) => StopAllTasksForSocket(GetSocketId(socket));

		#endregion

	}
}