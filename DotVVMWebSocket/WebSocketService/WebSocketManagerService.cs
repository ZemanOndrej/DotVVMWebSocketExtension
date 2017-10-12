using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketManagerService
	{
		public ConcurrentDictionary<string, WebSocket> Sockets { get; } = new ConcurrentDictionary<string, WebSocket>();


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
			Sockets.TryAdd(Guid.NewGuid().ToString(), socket);
		}

		public async Task RemoveSocket(string id)
		{
			Sockets.TryRemove(id, out WebSocket socket);

			await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed Peacefully", CancellationToken.None);
		}

		public async Task RemoveSocket(WebSocket socket) => await RemoveSocket(GetSocketId(socket));
	}
}