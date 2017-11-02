using DotVVM.Framework.Hosting;
using System;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Builder.Internal;
using Newtonsoft.Json;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public abstract class WebSocketHub
	{
		protected readonly WebSocketManagerService WebSocketManagerService;
		protected readonly IDotvvmRequestContext Context;
		protected readonly WebSocketViewModelSerializer serializer;


		public string CurrentSocketId { get; set; }
		public string CurrentGroupId { get; set; }
		public string SocketPath { get; set; } //TODO

		protected WebSocketHub(WebSocketManagerService webSocketManagerService, WebSocketViewModelSerializer serializer,
			IDotvvmRequestContext context, WebSocketConfiguration conf)
		{

			WebSocketManagerService = webSocketManagerService;
			this.serializer = serializer;
			Context = context;
			if (context == null) return;
			conf.WebsocketPaths.TryGetValue(GetType(), out var socketPath);
			SocketPath = socketPath.Value;
		}

		public virtual async Task OnConnected(WebSocket socket)
		{
			WebSocketManagerService.AddSocket(socket);
			CurrentSocketId = WebSocketManagerService.GetSocketId(socket);
			await SendMessageToSocketAsync(socket,
				JsonConvert.SerializeObject(new {socketId = CurrentSocketId, type = "webSocketInit"}, Formatting.None));
		}

		public virtual async Task OnDisconnected(WebSocket socket)
		{
			await WebSocketManagerService.RemoveSocket(socket);
		}

		public virtual async Task ReceiveMessageAsync(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			await SendMessageToSocketAsync(socket,
				$"Your Message was recieved, socketid&{WebSocketManagerService.GetSocketId(socket)}, message: &{message}");
		}

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
			await SendMessageToSocketAsync(WebSocketManagerService.GetWebSocketById(socketId), message);
		}

		public async Task SendMessageToClientAsync(string message)
		{
			await SendMessageToSocketAsync(WebSocketManagerService.GetWebSocketById(CurrentSocketId), message);
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
					Context.ViewModelJson?.Remove("viewModelDiff");

					await SendMessageToClientAsync(serializer.SerializeViewModel(Context));
				}
				catch (WebSocketException e)
				{
					Console.WriteLine(e);
				}
			}
		}

		public void CreateAndRunTask(Func<Progress<string>, CancellationToken, Task> func, Progress<string> progress)
		{
			var tokenSource = new CancellationTokenSource();
			WebSocketManagerService.AddTask(CurrentSocketId, Task.Run(() => func.Invoke(progress, tokenSource.Token)),
				tokenSource);
		}

		public void StopTask()
		{
			WebSocketManagerService.StopTaskForSocket(CurrentSocketId);
		}
	}
}