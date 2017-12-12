using System.Net.WebSockets;
using BL.Facades;
using DotVVM.Framework.Hosting;
using DotVVMWebSocketExtension.WebSocketService;

namespace SampleApp
{
	public class ChatService : WebSocketService
	{

		public ChatFacade Facade { get; set; }
		public ChatService(WebSocketManager webSocketManager, WebSocketViewModelSerializer serializer,
			IDotvvmRequestContext context,ChatFacade facade) : base(webSocketManager, serializer, context)
		{
			Facade = facade;
		}

		public override void OnDisconnected(Connection connection, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
			string statusString = "Closed Peacefully")
		{
			base.OnDisconnected(connection, status, statusString);
		}

		public override void OnDisconnected(WebSocket socket, WebSocketCloseStatus status = WebSocketCloseStatus.NormalClosure,
			string statusString = "Closed Peacefully")
		{
			Facade.DeteleDisconnectedUsers( WebSocketManager.GetConnectionId(socket));
			base.OnDisconnected(socket, status, statusString);
		}
	}
}