using System;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1.Hubs
{
	public class ServerEventHub : WebSocketHub
	{
		public ServerEventHub(WebSocketManagerService webSocketManagerService, IViewModelSerializer serializer,
			IDotvvmRequestContext context, IViewModelSerializationMapper mapper) : base(webSocketManagerService, serializer, context,mapper)
		{
		}




	}
}