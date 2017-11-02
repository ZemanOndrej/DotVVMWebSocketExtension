using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public static class WebSocketExtensions
	{
		public static IServiceCollection AddWebSocketManagerService(this IServiceCollection services)
		{
			services.TryAddSingleton<WebSocketManagerService>();
			services.TryAddSingleton<WebSocketViewModelSerializer>();

			foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
			{
				if (type.GetTypeInfo().BaseType == typeof(WebSocketHub))
				{
					services.TryAddScoped(type);
				}
			}
			return services;
		}

		public static IApplicationBuilder MapWebSocketService(this IApplicationBuilder app, PathString path, WebSocketHub hub)
		{
			return app.Map(path, a => a.UseMiddleware<WebSocketMiddleware>(hub));
		}
	}
}