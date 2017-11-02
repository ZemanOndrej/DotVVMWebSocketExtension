using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1.Hubs
{
	public class ServerEventHub : WebSocketHub
	{
		public ServerEventHub(WebSocketManagerService webSocketManagerService, WebSocketViewModelSerializer serializer,
			IDotvvmRequestContext context, WebSocketConfiguration conf)
			: base(webSocketManagerService, serializer, context, conf)
		{
		}
	}
}