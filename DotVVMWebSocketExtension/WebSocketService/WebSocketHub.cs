using DotVVM.Framework.Hosting;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public abstract class WebSocketHub
	{
		protected readonly WebSocketManagerService WebSocketManagerService;
		protected readonly IViewModelSerializer serializer;
		protected readonly IDotvvmRequestContext Context;


		public string SocketId { get; set; }
		public string GroupId { get; set; }

		protected WebSocketHub(WebSocketManagerService webSocketManagerService, IViewModelSerializer serializer, IDotvvmRequestContext context)
		{
			WebSocketManagerService = webSocketManagerService;
			this.serializer = serializer;
			Context = context;
		}

		public virtual async Task OnConnected(WebSocket socket)
		{

			WebSocketManagerService.AddSocket(socket);
			SocketId = WebSocketManagerService.GetSocketId(socket);
			await Task.Delay(1);//TODO WTF
			await SendMessageAsync(socket,
				JsonConvert.SerializeObject(new { socketId = SocketId,type="webSocketInit" }, Formatting.None));


		}

		public virtual async Task OnDisconnected(WebSocket socket)
		{
			await WebSocketManagerService.RemoveSocket(socket);
		}

		public async Task SendMessageAsync(WebSocket socket, string message)
		{
			if (socket.State == WebSocketState.Open)
			{
				await socket.SendAsync(new ArraySegment<byte>(
					Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}

		public async Task SendMessageToAllAsync(string message)
		{
			foreach (var pair in WebSocketManagerService.Sockets)
			{
				await SendMessageAsync(pair.Value, message);
			}
		}

		public async Task UpdateViewModelOnClient(IDotvvmRequestContext context)
		{
			serializer.BuildViewModel(context);
			await SendMessageToAllAsync(serializer.SerializeViewModel(context));
		}

		public virtual async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			await SendMessageAsync(socket,
				$"Your Message was recieved, socketid&{WebSocketManagerService.GetSocketId(socket)}, message: &{message}");
		}
	}
}