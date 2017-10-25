using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVMWebSocketExtension.WebSocketService;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace DotVVMWebSocketExtension.Tests
{
	public class WebSocketManagerServiceTests
	{
		private string testStr = "testString";
		private string testStr2 = "testString2";
		private string testStr3 = "testString3";
		private string testGroupName = "testString";

		[Fact]
		public void AddSocketWithCustomIdToListTest()
		{
			var service = new WebSocketManagerService();
			var socketMock = new Mock<WebSocket>();
			var res = service.AddSocket(socketMock.Object, testStr);

			var testList = new ConcurrentDictionary<string, WebSocket>();
			testList.TryAdd(testStr, socketMock.Object);

			Assert.Equal(res, testStr);
			Assert.Contains(testStr, service.Sockets.Keys);
			Assert.Contains(socketMock.Object, service.Sockets.Values);
			Assert.Equal(testList, service.Sockets);
		}

		[Fact]
		public void AddSocketWithGuidToListTest()
		{
			var service = new WebSocketManagerService();
			var socketMock = new Mock<WebSocket>();
			service.AddSocket(socketMock.Object);
			Assert.Contains(socketMock.Object, service.Sockets.Values);
		}

		[Fact]
		public async Task RemoveSocketFromListTest()
		{
			var service = new WebSocketManagerService();
			var socketMock = new Mock<WebSocket>();
			service.AddSocket(socketMock.Object, testStr);
			socketMock.Setup(s => s.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed Peacefully", CancellationToken.None))
				.Returns(Task.FromResult(0));
			await service.RemoveSocket(socketMock.Object);
			socketMock.Verify(s =>
				s.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closed Peacefully", CancellationToken.None));
			Assert.DoesNotContain(testStr, service.Sockets.Keys);
			Assert.DoesNotContain(testStr, service.SocketGroups.Values.SelectMany(i => i).Distinct());
		}

		[Fact]
		public void GetSocketIdTest()
		{
			var service = new WebSocketManagerService();
			var socketMock = new Mock<WebSocket>();
			service.AddSocket(socketMock.Object, testStr);

			Assert.Contains(testStr, service.Sockets.Keys);
			Assert.Contains(socketMock.Object, service.Sockets.Values);

			var id = service.GetSocketId(socketMock.Object);
			Assert.Equal(testStr, id);
		}


		[Fact]
		public void GetSocketByIdTest()
		{
			var service = new WebSocketManagerService();
			var socketMock = new Mock<WebSocket>();
			service.AddSocket(socketMock.Object, testStr);

			var res = service.GetWebSocketById(testStr);

			Assert.Equal(res, socketMock.Object);
		}

		[Fact]
		public void AddSocketToGroupTest()
		{
			var service = new WebSocketManagerService();
			var socketMock = new Mock<WebSocket>();
			service.AddSocket(socketMock.Object, testStr);
			service.CreateNewGroup(testGroupName);

			var testList = new ConcurrentDictionary<string, HashSet<string>>();
			testList.TryAdd(testGroupName, new HashSet<string> {testStr});


			service.AddSocketToGroup(socketMock.Object, testGroupName);

			service.SocketGroups.TryGetValue(testGroupName, out var list);
			Assert.Contains(testStr, list);
			Assert.Equal(testList, service.SocketGroups);
		}

		[Fact]
		public void RemoveSocketFromGroupTest()
		{
			var service = new WebSocketManagerService();
			var socketMock = new Mock<WebSocket>();
			service.AddSocket(socketMock.Object, testStr);
			service.CreateNewGroup(testGroupName);
			service.AddSocketToGroup(socketMock.Object, testGroupName);

			service.RemoveSocketFromGroup(socketMock.Object,testGroupName);

			service.SocketGroups.TryGetValue(testGroupName, out var list);
			Assert.Empty(list);

		}
	}
}