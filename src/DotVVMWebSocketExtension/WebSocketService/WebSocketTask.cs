using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json.Linq;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketTask
	{

		public Task Task { get; set; }

		public string TaskId { get; set; }

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public string ConnectionId { get; set; }


	}
}