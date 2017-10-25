using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketManagerService
	{
		public ConcurrentDictionary<string, WebSocket> Sockets { get; }

		public ConcurrentDictionary<string, HashSet<string>> SocketGroups { get; }

		public WebSocketManagerService()
		{
			SocketGroups = new ConcurrentDictionary<string, HashSet<string>>();
			Sockets = new ConcurrentDictionary<string, WebSocket>();
		}

		public WebSocket GetWebSocketById(string id)
		{
			Sockets.TryGetValue(id, out WebSocket ws);
			return ws;
		}

		public string GetSocketId(WebSocket socket)
		{
			return Sockets.FirstOrDefault(p => p.Value == socket).Key;
		}

		public string AddSocket(WebSocket socket)
		{
			var guid = Guid.NewGuid().ToString();

			return AddSocket(socket, guid);
		}

		public string AddSocket(WebSocket socket, string id)
		{
			if (!string.IsNullOrEmpty(id))
			{
				return Sockets.TryAdd(id, socket) ? id : null;
			}
			return null;
		}

		public async Task RemoveSocket(string socketId)
		{
			Sockets.TryRemove(socketId, out var socket);

			foreach (var socketGroup in SocketGroups)
			{
				socketGroup.Value.Remove(socketId);
			}

			await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed Peacefully", CancellationToken.None);
		}

		public async Task RemoveSocket(WebSocket socket) => await RemoveSocket(GetSocketId(socket));

		public string CreateNewGroup()
		{
			var id = Guid.NewGuid().ToString();
			SocketGroups.TryAdd(id, new HashSet<string>());
			return id;
		}

		public string CreateNewGroup(string id)
		{
			if (string.IsNullOrEmpty(id))
			{
				return null;
			}
			return SocketGroups.TryAdd(id, new HashSet<string>()) ? id : null;
		}

		public HashSet<string> RemoveGroup(string groupId)
		{
			var ok = SocketGroups.TryRemove(groupId, out var res);
			return ok ? res : null;
		}

		public void AddSocketToGroup(string socketId, string groupId)
		{
			if (SocketGroups.TryGetValue(groupId, out var templist))
			{
				templist.Add(socketId);
			}
		}

		public void AddSocketToGroup(WebSocket socket, string groupId) => AddSocketToGroup(GetSocketId(socket), groupId);

		public void RemoveSocketFromGroup(string socketId, string groupId)
		{
			if (SocketGroups.TryGetValue(groupId, out var templist))
			{
				templist.Remove(socketId);
			}
		}

		public void RemoveSocketFromGroup(WebSocket socket, string groupId) =>
			RemoveSocketFromGroup(GetSocketId(socket), groupId);
	}
}