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
using WebSocketManager = DotVVMWebSocketExtension.WebSocketService.WebSocketManager;

namespace DotVVMWebSocketExtension.Tests
{
	public class WebSocketManagerTests
	{
		private string testStr = "testString";
		private string testStr2 = "testString2";
		private string testStr3 = "testString3";

		private WebSocketManager manager;
		private Mock<Connection> connection;
		private Mock<WebSocket> socket;

		public WebSocketManagerTests()
		{
			manager = new WebSocketManager();
			socket = new Mock<WebSocket>();
			connection = new Mock<Connection>();
			connection.Object.Socket = socket.Object;
		}

		[Fact]
		public void AddSocketWithCustomIdToListTest()
		{
			var res = manager.AddConnection(connection.Object, testStr);

			var testList = new ConcurrentDictionary<string, Connection>();
			testList.TryAdd(testStr, connection.Object);

			Assert.Equal(res, testStr);
			Assert.Contains(testStr, manager.Connections.Keys);
			Assert.Contains(connection.Object, manager.Connections.Values);
			Assert.Equal(testList, manager.Connections);
		}

		[Fact]
		public void AddSocketWithGuidToListTest()
		{
			manager.AddConnection(connection.Object);
			Assert.Contains(connection.Object, manager.Connections.Values);
		}

		[Fact]
		public void RemoveSocketFromListTest()
		{
			manager.AddConnection(connection.Object, testStr);

			manager.RemoveConnection(connection.Object);

			Assert.DoesNotContain(testStr, manager.Connections.Keys);
		}

		[Fact]
		public void RemoveSocketFromListWithNullTest()
		{
			manager.AddConnection(connection.Object, testStr);
			Assert.Throws<ArgumentNullException>(() => manager.RemoveConnection((string) null));
			Assert.Throws<ArgumentNullException>(() => manager.RemoveConnection((WebSocket) null));
			Assert.Throws<ArgumentNullException>(() => manager.RemoveConnection((Connection) null));
			Assert.Contains(testStr, manager.Connections.Keys);
		}

		[Fact]
		public void RemoveSocketFromListWithWrongIdTest()
		{
			manager.AddConnection(connection.Object, testStr);
			Assert.Throws<ArgumentNullException>(() => manager.RemoveConnection(""));
			Assert.Contains(testStr, manager.Connections.Keys);
		}

		[Fact]
		public void GetSocketIdTest()
		{
			manager.AddConnection(connection.Object, testStr);

			Assert.Contains(testStr, manager.Connections.Keys);
			Assert.Contains(connection.Object, manager.Connections.Values);

			var id = manager.GetConnectionId(connection.Object);
			Assert.Equal(testStr, id);
		}

		[Fact]
		public void GetSocketByIdTest()
		{
			manager.AddConnection(connection.Object, testStr);

			var res = manager.GetConnetionById(testStr);

			Assert.Equal(res, connection.Object);
		}

		[Fact]
		public void AddTaskTest()
		{
			manager.AddConnection(connection.Object, testStr);

			Assert.Throws<KeyNotFoundException>(() => manager.TaskList[testStr]);
			manager.AddTask(testStr, new CancellationTokenSource());

			Assert.NotEmpty(manager.TaskList[testStr]);
		}

		[Fact]
		public void AddTaskNullTest()
		{
			manager.AddConnection(connection.Object, testStr);
			Assert.Throws<ArgumentNullException>(() => manager.AddTask(null, new CancellationTokenSource()));
			Assert.Throws<ArgumentNullException>(() => manager.AddTask(testStr, null));
		}

		[Fact]
		public void AddTaskMultipleTest()
		{
			manager.AddConnection(connection.Object, testStr);

			Assert.Throws<KeyNotFoundException>(() => manager.TaskList[testStr]);
			manager.AddTask(testStr, new CancellationTokenSource());

			Assert.NotEmpty(manager.TaskList[testStr]);
			manager.AddTask(testStr, new CancellationTokenSource());
			Assert.Equal(2, manager.TaskList[testStr].Count);
			Assert.Equal(1, manager.TaskList.Keys.Count);


			manager.AddTask(testStr2, new CancellationTokenSource());
			Assert.Equal(2, manager.TaskList[testStr].Count);
			Assert.Single(manager.TaskList[testStr2]);
			Assert.Equal(2, manager.TaskList.Keys.Count);
		}

		[Fact]
		public void RemoveTaskWithIdTest()
		{
			manager.AddConnection(connection.Object, testStr);

			var taskId = manager.AddTask(testStr, new CancellationTokenSource());

			manager.RemoveTaskWithId(taskId);
			Assert.Empty(manager.TaskList[testStr]);
		}

		[Fact]
		public void RemoveTaskWithInvalidValuesTest()
		{
			manager.AddConnection(connection.Object, testStr);
			manager.AddTask(testStr, new CancellationTokenSource());
			Assert.Throws<ArgumentNullException>(() => manager.RemoveTaskWithId(null));
			Assert.Throws<ArgumentNullException>(() => manager.RemoveTaskWithId(""));
			Assert.Throws<NullReferenceException>(() => manager.RemoveTaskWithId(testStr2));
			Assert.NotEmpty(manager.TaskList[testStr]);
		}

		[Fact]
		public void RemoveAllTasksForConnectionTest()
		{
			manager.AddConnection(connection.Object, testStr);
			manager.AddTask(testStr, new CancellationTokenSource());
			manager.AddTask(testStr, new CancellationTokenSource());

			Assert.Equal(2, manager.TaskList[testStr].Count);

			manager.RemoveAllTasksForConnection(testStr);

			Assert.Empty(manager.TaskList[testStr]);
		}
	}
}