using Microsoft.AspNetCore.Builder;
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
			services.TryAddScoped<WebSocketHub>();

			return services;
		}

		public static IApplicationBuilder MapWebSocketService(this IApplicationBuilder app)
		{
			return app.Map("/ws", a => a.UseMiddleware<WebSocketMiddleware>());
		}
	}
}