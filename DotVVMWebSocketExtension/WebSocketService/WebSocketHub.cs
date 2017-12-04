using DotVVM.Framework.Hosting;
using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketHub
	{
		#region PropertiesAndConstructor

		protected readonly WebSocketManagerService WebSocketManagerService;
		protected readonly IDotvvmRequestContext Context;
		protected readonly WebSocketViewModelSerializer serializer;

		public string CurrentSocketId { get; set; }
		public string CurrentGroupId { get; set; }

		public WebSocketHub(WebSocketManagerService webSocketManagerService, WebSocketViewModelSerializer serializer,
			IDotvvmRequestContext context)
		{
			WebSocketManagerService = webSocketManagerService;
			this.serializer = serializer;
			Context = context;
		}

		#endregion

		#region Connect/Disconnect

		public virtual async Task OnConnected(WebSocket socket)
		{
			WebSocketManagerService.AddSocket(socket);
			CurrentSocketId = WebSocketManagerService.GetSocketId(socket);
			await SendMessageToSocketAsync(socket,
				JsonConvert.SerializeObject(new {socketId = CurrentSocketId, type = "webSocketInit"}, Formatting.None));
		}

		public virtual async Task OnDisconnected(WebSocket socket)
		{
			WebSocketManagerService.StopAllTasksForSocket(socket);

			await WebSocketManagerService.RemoveSocket(socket);
		}

		#endregion

		public virtual async Task ReceiveMessageAsync(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			await SendMessageToSocketAsync(socket,
				$"Your Message was recieved, socketid&{WebSocketManagerService.GetSocketId(socket)}, message: &{message}");
		}


		#region Send/Update ViewModel

		public async Task SendMessageToSocketAsync(WebSocket socket, string message)
		{
			if (socket?.State == WebSocketState.Open)
			{
				await socket.SendAsync(new ArraySegment<byte>(
					Encoding.UTF8.GetBytes(message)), WebSocketMessageType.Text, true, CancellationToken.None);
			}
		}

		public async Task SendMessageToSocketAsync(string socketId, string message)
		{
			await SendMessageToSocketAsync(WebSocketManagerService.GetSocketById(socketId), message);
		}

		public async Task SendMessageToClientAsync(string message)
		{
			await SendMessageToSocketAsync(WebSocketManagerService.GetSocketById(CurrentSocketId), message);
		}

		public async Task SendMessageToAllAsync(string message)
		{
			foreach (var pair in WebSocketManagerService.Sockets)
			{
				await SendMessageToSocketAsync(pair.Value, message);
			}
		}

		public async Task SentMessageToGroup(string groupId, string message)
		{
			foreach (var socketId in WebSocketManagerService.SocketGroups[groupId])
			{
				await SendMessageToSocketAsync(socketId, message);
			}
		}

		public async Task UpdateCurrentViewModelOnClient()
		{
			if (Context != null)
			{
				try
				{
					serializer.BuildViewModel(Context);
					var serializedString = serializer.SerializeViewModel(Context);

					await SendMessageToClientAsync(serializedString);
				}
				catch (WebSocketException e)
				{
					Console.WriteLine(e);
				}
			}
		}

		#endregion

		#region TaskManagement

		public string CreateAndRunTask(Func< CancellationToken, Task> func)
		{
			var tokenSource = new CancellationTokenSource();
			return WebSocketManagerService.AddTask(CurrentSocketId, Task.Run(() => func.Invoke( tokenSource.Token)),
				tokenSource);
		}

		public void StopTask()
		{
			WebSocketManagerService.StopAllTasksForSocket(CurrentSocketId);
		}

		#endregion

		public async Task GetViewModelFromClientAsync()
		{
		}
	}
}