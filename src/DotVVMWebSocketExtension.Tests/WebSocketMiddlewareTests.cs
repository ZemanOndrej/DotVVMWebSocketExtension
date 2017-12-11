using System;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVMWebSocketExtension.WebSocketService;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DotVVMWebSocketExtension.Tests
{
	public class WebSocketMiddlewareTests
	{
		private readonly Mock<WebSocketManager> wsm;
		private readonly Mock<WebSocket> websocket;
		private readonly Mock<HttpContext> contextMock;
		private readonly Mock<DotvvmRequestContext> dotvvmContextMock;
		private readonly Mock<WebSocketViewModelSerializer> serializer;
		private readonly Mock<WebSocketFacade> hubMock;
		private readonly Mock<HttpRequest> requestMock;

		public WebSocketMiddlewareTests()
		{
			contextMock = new Mock<HttpContext>();
			dotvvmContextMock = new Mock<DotvvmRequestContext>();
			serializer = new Mock<WebSocketViewModelSerializer>(new Mock<IViewModelSerializationMapper>().Object);
			hubMock = new Mock<WebSocketFacade>(new Mock<WebSocketManagerService>().Object, serializer.Object,
				dotvvmContextMock.Object);
			requestMock = new Mock<HttpRequest>();
			wsm = new Mock<WebSocketManager>();
			websocket = new Mock<WebSocket>();

		}

		[Fact]
		public async Task NotAWebsocketRequestTest()
		{

			hubMock.Setup(s => s.OnConnected(new Mock<WebSocket>().Object)).Throws(new Exception("should not have been called"));
			contextMock.Setup(x => x.Request).Returns(requestMock.Object);
			contextMock.SetupGet(p => p.WebSockets).Returns(wsm.Object);

			wsm.SetupGet(ex => ex.IsWebSocketRequest).Returns(false);
			wsm.Setup(s => s.WebSocketRequestedProtocols.Count).Throws(new Exception("should not have been called"));

			var middleware = new WebSocketMiddleware(innerHttpContext => Task.FromResult(0), hubMock.Object);
			await middleware.Invoke(contextMock.Object);
		}

		[Fact]
		public async Task WebSocketClogingTest()
		{
			contextMock.SetupGet(p => p.WebSockets).Returns(wsm.Object);
			contextMock.Setup(x => x.Request).Returns(requestMock.Object);

			wsm.SetupGet(ex => ex.IsWebSocketRequest).Returns(true);
			wsm.Setup(s => s.WebSocketRequestedProtocols.Count).Returns(0);
			wsm.Setup(s => s.AcceptWebSocketAsync()).Returns(Task.FromResult(websocket.Object));

			websocket.SetupGet(s => s.State).Returns(WebSocketState.Open);
			websocket.Setup(s => s.ReceiveAsync(new ArraySegment<byte>(new byte[1024 * 4]), CancellationToken.None))
				.Returns(Task.FromResult(new Mock<WebSocketReceiveResult>(123, WebSocketMessageType.Close, true).Object));

			var middleware = new WebSocketMiddleware(innerHttpContext => Task.FromResult(0), hubMock.Object);
			var t = Task.Run(() => middleware.Invoke(contextMock.Object));
			await Task.Delay(100);
			hubMock.Verify(m => m.OnConnected(websocket.Object));
			wsm.Verify(m => m.AcceptWebSocketAsync());
			hubMock.Verify(m => m.OnDisconnected(websocket.Object));
		}

		[Fact]
		public async Task WebSocketTextMessageTest()
		{
			var result = new Mock<WebSocketReceiveResult>(123, WebSocketMessageType.Text, true);
			var buffer = new byte[1024 * 4];
			contextMock.SetupGet(p => p.WebSockets).Returns(wsm.Object);
			contextMock.Setup(x => x.Request).Returns(requestMock.Object);

			wsm.SetupGet(ex => ex.IsWebSocketRequest).Returns(true);
			wsm.Setup(s => s.WebSocketRequestedProtocols.Count).Returns(0);
			wsm.Setup(s => s.AcceptWebSocketAsync()).Returns(Task.FromResult(websocket.Object));

			websocket.SetupGet(s => s.State).Returns(WebSocketState.Open);
			websocket.Setup(s => s.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None))
				.Returns(Task.FromResult(result.Object));

			var middleware = new WebSocketMiddleware(innerHttpContext => Task.FromResult(0), hubMock.Object);
			var t = Task.Run(() => middleware.Invoke(contextMock.Object));
			await Task.Delay(500);
			hubMock.Verify(m => m.OnConnected(websocket.Object));
			wsm.Verify(m => m.AcceptWebSocketAsync());
			hubMock.Verify(h => h.ReceiveMessageAsync(websocket.Object, result.Object, ""));
		}
	}
}