using System.Net.WebSockets;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1.Hubs
{
	public class MyHub : WebSocketHub
	{
		public MyHub(WebSocketManagerService webSocketManagerService, WebSocketViewModelSerializer viewModelSerializer,
			IDotvvmRequestContext context)
			: base(webSocketManagerService, viewModelSerializer, context)
		{
		}

		public override async Task OnConnected(WebSocket socket)
		{
			await base.OnConnected(socket);
			await SendMessageToAllAsync($" ${WebSocketManagerService.GetSocketId(socket)} user has connected");
		}

		public override async Task OnDisconnected(WebSocket socket)
		{
			await SendMessageToAllAsync($" ${WebSocketManagerService.GetSocketId(socket)} user has disconnected");

			await base.OnDisconnected(socket);
		}

		public override async Task ReceiveMessageAsync(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			await SendMessageToAllAsync($"{WebSocketManagerService.GetSocketId(socket)} said: {message}");
		}
	}
}