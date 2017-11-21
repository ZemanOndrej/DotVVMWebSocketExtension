using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVMWebSocketExtension.WebSocketService
{
	/// <summary>
	/// service that stores SocketList, SocketGroupList and LongRunningTask list and operations on them
	/// </summary>
	public class WebSocketManagerService
	{
		#region PropsAndConstructor

		public ConcurrentDictionary<string, WebSocket> Sockets { get; }

		public ConcurrentDictionary<string, HashSet<string>> SocketGroups { get; }

		public ConcurrentDictionary<string,
			HashSet<(Task Task, CancellationTokenSource CancellationTokenSource, string TaskId)>> TaskList { get; }

		/// <summary>
		/// Initializes a new instance of the <see cref="WebSocketManagerService"/> class.
		/// </summary>
		public WebSocketManagerService()
		{
			SocketGroups = new ConcurrentDictionary<string, HashSet<string>>();
			Sockets = new ConcurrentDictionary<string, WebSocket>();
			TaskList = new ConcurrentDictionary<string, HashSet<(Task, CancellationTokenSource, string)>>();
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

			foreach (var socketGroup in SocketGroups)
			{
				socketGroup.Value.Remove(socketId);
			}

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
		public string AddTask(string socketId, Task task, CancellationTokenSource token)
		{
			var taskId = Guid.NewGuid().ToString();
			if (TaskList.ContainsKey(socketId))
			{
				if (TaskList.TryGetValue(socketId, out var set))
				{
					set.Add((task, token, taskId));
				}
			}
			else
			{
				TaskList.TryAdd(socketId,
					new HashSet<(Task, CancellationTokenSource, string)>
					{
						(task, token, taskId)
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

		#region GroupManagement

		/// <summary>
		/// Creates the new group.
		/// </summary>
		/// <returns> ID</returns>
		public string CreateNewGroup()
		{
			var id = Guid.NewGuid().ToString();
			SocketGroups.TryAdd(id, new HashSet<string>());
			return id;
		}

		/// <summary>
		/// Creates the new group with Id
		/// </summary>
		/// <param name="groupId">The group identifier.</param>
		/// <returns>ID</returns>
		public string CreateNewGroup(string groupId)
		{
			if (string.IsNullOrEmpty(groupId) || SocketGroups.ContainsKey(groupId))
			{
				return null;
			}
			return SocketGroups.TryAdd(groupId, new HashSet<string>()) ? groupId : null;
		}

		/// <summary>
		/// Removes the group with ID
		/// </summary>
		/// <param name="groupId">The group identifier.</param>
		/// <returns>socket hashSet from deleted group</returns>
		public HashSet<string> RemoveGroup(string groupId)
		{
			var ok = SocketGroups.TryRemove(groupId, out var res);
			return ok ? res : null;
		}

		/// <summary>
		/// Adds the socket to group.
		/// </summary>
		/// <param name="socketId">The socket identifier.</param>
		/// <param name="groupId">The group identifier.</param>
		public void AddSocketToGroup(string socketId, string groupId)
		{
			if (SocketGroups.TryGetValue(groupId, out var templist))
			{
				templist.Add(socketId);
			}
		}

		/// <summary>
		/// Adds the socket to group.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="groupId">The group identifier.</param>
		public void AddSocketToGroup(WebSocket socket, string groupId) => AddSocketToGroup(GetSocketId(socket), groupId);

		/// <summary>
		/// Removes the socket from group.
		/// </summary>
		/// <param name="socketId">The socket identifier.</param>
		/// <param name="groupId">The group identifier.</param>
		public void RemoveSocketFromGroup(string socketId, string groupId)
		{
			if (SocketGroups.TryGetValue(groupId, out var templist))
			{
				templist.Remove(socketId);
			}
		}

		/// <summary>
		/// Removes the socket from group.
		/// </summary>
		/// <param name="socket">The socket.</param>
		/// <param name="groupId">The group identifier.</param>
		public void RemoveSocketFromGroup(WebSocket socket, string groupId) =>
			RemoveSocketFromGroup(GetSocketId(socket), groupId);

		#endregion
	}
}