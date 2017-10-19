using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public static class WebSocketExtensions
	{
		public static IServiceCollection AddWebSocketManagerService(this IServiceCollection services)
		{
			services.AddSingleton<WebSocketManagerService>();

			foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
			{
				if (type.GetTypeInfo().BaseType == typeof(WebSocketHub))
				{
					services.AddSingleton(type);
				}
			}

			return services;
		}

		public static IApplicationBuilder MapWebSocketService(this IApplicationBuilder app, PathString path, WebSocketHub hub)
		{
			app.UseWebSockets();
			return app.Map(path, a => a.UseMiddleware<WebSocketMiddleware>(hub));
		}
	}
}