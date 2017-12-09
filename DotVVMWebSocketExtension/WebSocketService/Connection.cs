﻿using System;
using System.Net.WebSockets;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class Connection : IDisposable
	{
		public JObject LastSentViewModelJson { get; set; }
		public WebSocket Socket { get; set; }
		public bool IsConnected => Socket?.State == WebSocketState.Open;

		protected bool Equals(Connection other)
		{
			return Equals(Socket, other.Socket);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != GetType()) return false;
			return Equals((Connection) obj);
		}

		public override int GetHashCode()
		{
			return (Socket != null ? Socket.GetHashCode() : 0);
		}

		public void Dispose(WebSocketCloseStatus status,string closeStatusString)
		{
			Socket.CloseAsync(status, closeStatusString, CancellationToken.None);
			Socket?.Dispose();

		}

		public void Dispose() => Dispose(WebSocketCloseStatus.NormalClosure, "Closed Peacefully");
	}
}