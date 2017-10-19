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
		public ConcurrentDictionary<string, WebSocket> Sockets { get; } = new ConcurrentDictionary<string, WebSocket>();

		public ConcurrentDictionary<string, HashSet<string>> SocketGroups { get; } =
			new ConcurrentDictionary<string, HashSet<string>>();

		public WebSocket GetWebSocketById(string id)
		{
			Sockets.TryGetValue(id, out WebSocket ws);
			return ws;
		}

		public string GetSocketId(WebSocket socket)
		{
			return Sockets.FirstOrDefault(p => p.Value == socket).Key;
		}

		public void AddSocket(WebSocket socket)
		{
			Sockets.TryAdd(socket.SubProtocol ?? Guid.NewGuid().ToString(), socket);
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

		public HashSet<string> GetAllSocketsFromGroup(string socketGroupId)
		{
			SocketGroups.TryGetValue(socketGroupId, out var resultList);
			return resultList;
		}

		public void AddSocketToGroup(string socketId, string groupId)
		{
			if (SocketGroups.TryGetValue(groupId, out var templist))
			{
				templist.Add(socketId);
			}
		}

		public void RemoveSocketFromGroup(string socketId, string groupId)
		{
			if (SocketGroups.TryGetValue(groupId, out var templist))
			{
				templist.Remove(socketId);
			}
		}
	}
}