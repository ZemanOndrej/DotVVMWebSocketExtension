using System.Threading;
using System.Threading.Tasks;
using DotVVM.Framework.Hosting;
using Newtonsoft.Json.Linq;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public class WebSocketTask
	{
		public WebSocketTask(Task task, string taskId, CancellationTokenSource token, IDotvvmRequestContext context)
		{
			Task = task;
			TaskId = taskId;
			CancellationTokenSource = token;
			Context = context;
			LastSentViewModel =new JObject();
		}

		public Task Task { get; set; }

		public string TaskId { get; set; }

		public CancellationTokenSource CancellationTokenSource { get; set; }

		public IDotvvmRequestContext Context { get; set; }

		public JObject LastSentViewModel { get; set; }


	}
}