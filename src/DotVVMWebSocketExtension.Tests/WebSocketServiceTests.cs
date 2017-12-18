using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using DotVVM.Framework.ViewModel.Serialization;
using DotVVMWebSocketExtension.WebSocketService;
using Moq;
using Newtonsoft.Json.Linq;
using Xunit;

namespace DotVVMWebSocketExtension.Tests
{
	public class WebSocketServiceTests
	{
		private WebSocketService.WebSocketService service;
		private Mock<WebSocketViewModelSerializer> serializer;
		private Mock<WebSocketManager> manager;
		private Mock<IDotvvmRequestContext> context;
		private Mock<WebSocket> socket;
		private Mock<Connection> connection;
		private string guid = Guid.Empty.ToString();
		public Mock<IViewModelSerializationMapper> mapper { get; set; }


		public WebSocketServiceTests()
		{
			mapper = new Mock<IViewModelSerializationMapper>();

			context = new Mock<IDotvvmRequestContext>();
			serializer = new Mock<WebSocketViewModelSerializer>(mapper.Object);
			socket = new Mock<WebSocket>();
			manager = new Mock<WebSocketManager>();
			connection = new Mock<Connection>();
			service = new WebSocketService.WebSocketService(manager.Object, serializer.Object, context.Object)
			{
				Path = "/test"
			};
		}

		[Fact]
		public void OnDisconnectedTest()
		{
			manager.Object.Connections.TryAdd(guid, connection.Object);
			manager.Object.TaskList.TryAdd(guid,
				new HashSet<WebSocketTask> {new WebSocketTask {CancellationTokenSource = new CancellationTokenSource()}});
			Assert.NotEmpty(manager.Object.Connections);
			Assert.NotEmpty(manager.Object.TaskList[guid]);

			connection.Object.Socket = socket.Object;
			service.OnDisconnected(connection.Object);

			Assert.Empty(manager.Object.Connections);
			Assert.Empty(manager.Object.TaskList.Keys);
		}

		[Fact]
		public void OnDisconnectedSocketTest()
		{
			manager.Object.Connections.TryAdd(guid, connection.Object);
			manager.Object.TaskList.TryAdd(guid,
				new HashSet<WebSocketTask> {new WebSocketTask {CancellationTokenSource = new CancellationTokenSource()}});
			Assert.NotEmpty(manager.Object.Connections);
			Assert.NotEmpty(manager.Object.TaskList[guid]);

			connection.Object.Socket = socket.Object;
			service.OnDisconnected(socket.Object);

			Assert.Empty(manager.Object.Connections);
			Assert.Empty(manager.Object.TaskList.Keys);
		}


		[Fact]
		public void OnDisconnectedExceptiomTest()
		{
			Assert.Throws<NullReferenceException>(() => service.OnDisconnected(connection.Object));
		}

		[Fact]
		public async Task OnConnectedTest()
		{
			Assert.Empty(manager.Object.Connections);
			Assert.Empty(manager.Object.TaskList);

			connection.Object.Socket = socket.Object;
			await service.OnConnected(socket.Object);
			Assert.NotEmpty(manager.Object.Connections);
			Assert.Equal(manager.Object.Connections.Values.First(), new Connection {Socket = socket.Object});
		}

		[Fact]
		public async Task OnConnectedTwoTimesTest()
		{
			Assert.Empty(manager.Object.Connections);
			Assert.Empty(manager.Object.TaskList);

			connection.Object.Socket = socket.Object;
			await service.OnConnected(socket.Object);
			await service.OnConnected(socket.Object);
			Assert.Equal(manager.Object.Connections.Values.First(), new Connection {Socket = socket.Object});
			Assert.Equal(2, manager.Object.Connections.Values.Count);
		}

		[Fact]
		public async Task OnConnectedExceptiomTest()
		{
			await Assert.ThrowsAsync<NullReferenceException>(async () => await service.OnConnected(null));
		}

		[Fact]
		public void SaveStateTest()
		{
			context.SetupGet(c => c.ViewModel).Returns(new {a = 2, b = 3});
			context.SetupGet(c => c.CsrfToken).Returns("csrfstring");
			service = new WebSocketService.WebSocketService(manager.Object, serializer.Object, context.Object)
			{
				Path = "/test",
				ConnectionId = guid
			};

			connection.Object.ViewModelState.ChangedViewModelJson = JObject.Parse(@"{'CPU': 'Intel'}");

			manager.Object.Connections.TryAdd(guid, connection.Object);
			serializer.Setup(s => s.BuildViewModel(connection.Object.ViewModelState));

			service.SaveCurrentState();


			serializer.Verify(s => s.BuildViewModel(connection.Object.ViewModelState));
			Assert.Equal(context.Object.CsrfToken, connection.Object.ViewModelState.CsrfToken);
			Assert.Equal(context.Object.ViewModel, connection.Object.ViewModelState.LastSentViewModel);
			Assert.Equal(connection.Object.ViewModelState.LastSentViewModelJson,
				connection.Object.ViewModelState.ChangedViewModelJson);
		}

		[Fact]
		public void CreateTaskTest()
		{
			Assert.Empty(manager.Object.TaskList.Values);

			service = new WebSocketService.WebSocketService(manager.Object, serializer.Object, context.Object)
			{
				Path = "/test",
				ConnectionId = guid
			};
			manager.Object.Connections.TryAdd(guid, connection.Object);


			var id = service.CreateTask<WebSocketService.WebSocketService>(LongTaskAsync);

			Assert.NotEmpty(manager.Object.TaskList.Values);
			Assert.NotNull(manager.Object.TaskList.SelectMany(s => s.Value).First(s => s.TaskId == id));
		}

		[Fact]
		public void CreateTaskToSameConnectionTest()
		{
			Assert.Empty(manager.Object.TaskList.Values);

			service = new WebSocketService.WebSocketService(manager.Object, serializer.Object, context.Object)
			{
				Path = "/test",
				ConnectionId = guid
			};
			manager.Object.Connections.TryAdd(guid, connection.Object);


			var id = service.CreateTask<WebSocketService.WebSocketService>(LongTaskAsync);
			var id2 = service.CreateTask<WebSocketService.WebSocketService>(LongTaskAsync);

			Assert.NotEmpty(manager.Object.TaskList.Values);
			Assert.Equal(2, manager.Object.TaskList.SelectMany(s => s.Value).Count());
			Assert.NotNull(manager.Object.TaskList.SelectMany(s => s.Value).First(s => s.TaskId == id));
			Assert.NotNull(manager.Object.TaskList.SelectMany(s => s.Value).First(s => s.TaskId == id2));
		}

		[Fact]
		public void CreateTaskFailTest()
		{
			Assert.Throws<ArgumentNullException>(() => service.CreateTask<WebSocketService.WebSocketService>(LongTaskAsync));
			Assert.Empty(manager.Object.TaskList.Values);
		}

		[Fact]
		public void CreateTaskNullFunctionTest()
		{
			Assert.Throws<ArgumentNullException>(() => service.CreateTask<WebSocketService.WebSocketService>(null));
			Assert.Empty(manager.Object.TaskList.Values);
		}

		[Fact]
		public void StopTaskTest()
		{
			manager.Object.TaskList.TryAdd(guid, new HashSet<WebSocketTask>
			{
				new WebSocketTask
				{
					TaskId = guid,
					CancellationTokenSource = new CancellationTokenSource(),
					ConnectionId = guid
				}
			});
			Assert.NotEmpty(manager.Object.TaskList.Values);
			service.StopTask(guid);
			Assert.Empty(manager.Object.TaskList.SelectMany(s => s.Value));
		}

		[Fact]
		public void StopTaskNoGuidTest()
		{
			Assert.Throws<ArgumentNullException>(() => service.StopTask(null));
		}


		[Fact]
		public async Task ChangeViewModelTest()
		{
			service.ConnectionId = guid + "3";

			connection.Object.ViewModelState.LastSentViewModel = new TestClass();
			manager.Object.Connections.TryAdd(guid, connection.Object);

			await service.ChangeViewModelForConnectionsAsync((TestClass viewModel) => { viewModel.A = 54; },
				new List<string> {guid});

			Assert.Equal(54, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).A);
			Assert.Equal(32, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).B);
		}

		[Fact]
		public async Task ChangeViewModelForCurrentClientTest()
		{
			connection.Object.ViewModelState.LastSentViewModel = new TestClass();
			service.ConnectionId = guid;
			manager.Object.Connections.TryAdd(guid, connection.Object);

			await service.ChangeViewModelForCurrentConnectionAsync((TestClass viewModel) => { viewModel.A = 54; });


			Assert.Equal(54, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).A);
			Assert.Equal(32, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).B);
		}

		[Fact]
		public async Task ChangeViewModelForCurrentClientNullFuncTest()
		{
			connection.Object.ViewModelState.LastSentViewModel = new TestClass();
			service.ConnectionId = guid;
			manager.Object.Connections.TryAdd(guid, connection.Object);

			await Assert.ThrowsAsync<ArgumentNullException>(async () =>
				await service.ChangeViewModelForCurrentConnectionAsync<TestClass>(null));


			Assert.Equal(2, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).A);
			Assert.Equal(32, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).B);
		}

		[Fact]
		public async Task ChangeViewModelForClientsNullFuncTest()
		{
			connection.Object.ViewModelState.LastSentViewModel = new TestClass();
			service.ConnectionId = guid;
			manager.Object.Connections.TryAdd(guid, connection.Object);

			await Assert.ThrowsAsync<ArgumentNullException>(async () =>
				await service.ChangeViewModelForConnectionsAsync<TestClass>(null, null));


			Assert.Equal(2, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).A);
			Assert.Equal(32, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).B);
		}

		[Fact]
		public async Task ChangeViewModelNullguidTest()
		{
			connection.Object.ViewModelState.LastSentViewModel = new TestClass();
			service.ConnectionId = guid + "3";
			manager.Object.Connections.TryAdd(guid, connection.Object);

			await Assert.ThrowsAsync<ArgumentNullException>(() => service.ChangeViewModelForConnectionsAsync(
				(TestClass viewModel) => { viewModel.A = 54; },
				new List<string> {null}));


			Assert.Equal(2, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).A);
			Assert.Equal(32, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).B);
		}

		[Fact]
		public async Task ChangeViewModelCurrent()
		{
			connection.Object.ViewModelState.LastSentViewModel = new TestClass();
			service.ConnectionId = guid;
			manager.Object.Connections.TryAdd(guid, connection.Object);

			await service.ChangeViewModelForConnectionsAsync((TestClass viewModel) => { viewModel.A = 54; },
				new List<string> {guid});


			Assert.Equal(2, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).A);
			Assert.Equal(32, ((TestClass) connection.Object.ViewModelState.LastSentViewModel).B);
		}


		[Fact]
		public void SendSyncRequestTest()
		{
			service.ConnectionId = guid;
			manager.Object.Connections.TryAdd(guid, connection.Object);
			manager.Object.TaskList.TryAdd(guid, new HashSet<WebSocketTask>
			{
				new WebSocketTask
				{
					TaskId = guid,
					CancellationTokenSource = new CancellationTokenSource(),
					ConnectionId = guid
				}
			});
			var task = manager.Object.TaskList.SelectMany(s => s.Value).First(t => t.TaskId == guid);
			service.SendSyncRequestToClient(guid);
			task.TaskCompletion.SetResult(true);
		}

		[Fact]
		public async Task SendSyncRequestFailTest()
		{
			await Assert.ThrowsAsync<ArgumentNullException>(() => service.SendSyncRequestToClient(null));
			await Assert.ThrowsAsync<ArgumentNullException>(() => service.SendSyncRequestToClient(guid));
		}


		private async Task LongTaskAsync(WebSocketService.WebSocketService webSocketService,
			CancellationToken cancellationToken, string taskId)
		{
			for (int i = 1; i < 5; ++i)
			{
				await Task.Delay(20);
				cancellationToken.ThrowIfCancellationRequested();
				await Task.Delay(20);
			}
		}
	}
}