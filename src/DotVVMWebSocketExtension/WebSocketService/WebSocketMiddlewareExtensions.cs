using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public static class WebSocketMiddlewareExtensions
	{

		/// <summary>
		/// Adds services needed for WebSocket Extension for DotVVM
		/// </summary>
		/// <param name="services">The services.</param>
		/// <returns></returns>
		public static IServiceCollection AddWebSocketManagerService(this IServiceCollection services)
		{
			services.TryAddSingleton<WebSocketManagerService>();
			services.TryAddSingleton<WebSocketViewModelSerializer>();
			services.TryAddScoped<WebSocketFacade>();
			return services;
		}

		/// <summary>
		/// Uses WebSocket Middleware
		/// Maps url starting with "/ws" for the WebSocket middleware.
		/// </summary>
		/// <param name="app">The application.</param>
		/// <returns></returns>
		public static IApplicationBuilder MapWebSocketService(this IApplicationBuilder app)
		{
			app.UseWebSockets();
			return app.Map("/ws", a => a.UseMiddleware<WebSocketMiddleware>());
		}
	}
}