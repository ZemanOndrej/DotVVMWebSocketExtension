using DotVVM.Framework.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ResourceManagement;
using DotVVM.Framework.Utils;
using DotVVM.Framework.ViewModel.Serialization;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public abstract class WebSocketHub
	{
		protected readonly WebSocketManagerService WebSocketManagerService;
		protected readonly IViewModelSerializer serializer;
		protected readonly IDotvvmRequestContext Context;
		protected readonly IViewModelSerializationMapper mapper;


		public string SocketId { get; set; }
		public string GroupId { get; set; }

		protected WebSocketHub(WebSocketManagerService webSocketManagerService, IViewModelSerializer serializer,
			IDotvvmRequestContext context, IViewModelSerializationMapper mapper)
		{
			WebSocketManagerService = webSocketManagerService;
			this.serializer = serializer;
			Context = context;
			this.mapper = mapper;
		}

		public virtual async Task OnConnected(WebSocket socket)
		{
			WebSocketManagerService.AddSocket(socket);
			SocketId = WebSocketManagerService.GetSocketId(socket);
			await Task.Delay(4); //TODO WTF
			await SendMessageAsync(socket,
				JsonConvert.SerializeObject(new {socketId = SocketId, type = "webSocketInit"}, Formatting.None));
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

		public async Task SendMessageAsync(string message)
		{
			await SendMessageAsync(WebSocketManagerService.GetWebSocketById(SocketId), message);
		}

		public async Task SendMessageToAllAsync(string message)
		{
			foreach (var pair in WebSocketManagerService.Sockets)
			{
				await SendMessageAsync(pair.Value, message);
			}
		}

		public async Task UpdateViewModelOnAllClients(IDotvvmRequestContext context)
		{
			serializer.BuildViewModel(context);
			await SendMessageToAllAsync(serializer.SerializeViewModel(context));
		}

		public async Task UpdateYourViewModelOnClient(IDotvvmRequestContext context)
		{
			//			var ser = (DefaultViewModelSerializer) serializer;

			Context.ViewModelJson?.Remove("viewModelDiff");

			serializer.BuildViewModel(context);
			await SendMessageAsync(serializer.SerializeViewModel(context));
		}

		public virtual async Task ReceiveAsync(WebSocket socket, WebSocketReceiveResult result, string message)
		{
			await SendMessageAsync(socket,
				$"Your Message was recieved, socketid&{WebSocketManagerService.GetSocketId(socket)}, message: &{message}");
		}



		public async Task UpdateYourViewModelOnClient()
		{
			Context.ViewModelJson?.Remove("viewModelDiff");
			BuildViewModel();


			await SendMessageAsync(serializer.SerializeViewModel(Context));
		}

		private void BuildViewModel()
		{
			var jsonSerializer = CreateJsonSerializer();
			var viewModelConverter = new ViewModelJsonConverter(Context.IsPostBack, mapper)
			{
				UsedSerializationMaps = new HashSet<ViewModelSerializationMap>()
			};
			jsonSerializer.Converters.Add(viewModelConverter);
			var writer = new JTokenWriter();
			try
			{
				jsonSerializer.Serialize(writer, Context.ViewModel);
			}
			catch (Exception ex)
			{
				throw new Exception($"Could not serialize viewModel of type { Context.ViewModel.GetType().Name }. Serialization failed at property { writer.Path }.", ex);
			}

			writer.Token["$csrfToken"] = Context.CsrfToken;


			var result = new JObject();
			result["viewModel"] = writer.Token;
//			result["url"] = Context.HttpContext?.Request?.Url?.PathAndQuery; TODO wtf
			result["virtualDirectory"] = Context.HttpContext?.Request?.PathBase?.Value?.Trim('/') ?? "";
			if (Context.ResultIdFragment != null)
			{
				result["resultIdFragment"] = Context.ResultIdFragment;
			}
			if (Context.IsPostBack || Context.IsSpaRequest)
			{
				result["action"] = "successfulCommand";
				var renderedResources = new HashSet<string>(Context.ReceivedViewModelJson?["renderedResources"]?.Values<string>() ?? new string[] { });
				result["resources"] = BuildResourcesJson(Context, rn => !renderedResources.Contains(rn));
			}
			else
			{
				result["renderedResources"] = JArray.FromObject(Context.ResourceManager.RequiredResources);
			}

			Context.ViewModelJson = result;
		}

		protected virtual JsonSerializer CreateJsonSerializer() => CreateDefaultSettings().Apply(JsonSerializer.Create);

		public static JsonSerializerSettings CreateDefaultSettings()
		{
			var s = new JsonSerializerSettings()
			{
				DateTimeZoneHandling = DateTimeZoneHandling.Unspecified
			};
			s.Converters.Add(new DotvvmDateTimeConverter());
			s.Converters.Add(new StringEnumConverter());
			return s;
		}


		public JObject BuildResourcesJson(IDotvvmRequestContext context, Func<string, bool> predicate)
		{
			var manager = context.ResourceManager;
			var resourceObj = new JObject();
			foreach (var resource in manager.GetNamedResourcesInOrder())
			{
				if (predicate(resource.Name))
				{
					using (var str = new StringWriter())
					{
						resourceObj[resource.Name] = JValue.CreateString(resource.GetRenderedTextCached(context));
					}
				}
			}
			return resourceObj;
		}
	}
}