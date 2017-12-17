using DotVVM.Framework.Hosting;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.Runtime.Filters;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public partial class WebSocketService
	{
		public class WebSocketActionFilter : ActionFilterAttribute
		{
			/// <summary>
			/// Called after page is processed and ready to be sent to client.
			/// Invokes all tasks that have been added so they cant change ViewModel in request
			/// </summary>
			/// <param name="context"></param>
			/// <returns></returns>
			protected override Task OnPageLoadedAsync(IDotvvmRequestContext context)
			{
				var wsMgr = (WebSocketManager) context.Services.GetService(typeof(WebSocketManager));

				foreach (var task in wsMgr.TaskList.SelectMany(s => s.Value))
				{
					if (task.Task==null)
					{
						task.Task = task.FunctionToInvoke.Invoke();
					}
				}
				return base.OnPageLoadedAsync(context);
			}
		}
	}
}