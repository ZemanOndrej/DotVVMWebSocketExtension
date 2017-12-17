using System;
using System.Threading;
using System.Threading.Tasks;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketTask
	{

		public string TaskId { get; set; }

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public string ConnectionId { get; set; }

		public TaskCompletionSource<bool> TaskCompletion { get; set; }

		public Func<Task> FunctionToInvoke { get; set; }

		public Task Task { get; set; }

	}
}