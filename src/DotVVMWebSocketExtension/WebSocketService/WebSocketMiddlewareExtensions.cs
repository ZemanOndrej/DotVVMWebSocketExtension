using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DotVVMWebSocketExtension.WebSocketService
{
	public static class WebSocketMiddlewareExtensions
	{
		/// <summary>
		/// Adds services needed for WebSocketManager Extension for DotVVM
		/// </summary>
		/// <param name="services">The services.</param>
		/// <returns></returns>
		public static IServiceCollection AddWebSocketManagerService(this IServiceCollection services)
		{
			services.TryAddSingleton<WebSocketManager>();
			services.TryAddSingleton<WebSocketViewModelSerializer>();
			services.TryAddScoped<WebSocketService>();
			foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
			{
				if (type.GetTypeInfo().BaseType == typeof(WebSocketService))
				{
					services.AddScoped(type);
				}
			}
			return services;
		}

		/// <summary>
		/// Uses WebSocketManager Middleware
		/// Maps url starting with "/ws" for the WebSocketManager middleware.
		/// </summary>
		/// <param name="app">The application.</param>
		/// <param name="path"> Path of the websocket endpoint default is "/ws"</param>
		/// <param name="service"> Service that should be injected into WebSocketMiddleware on request</param>
		/// <returns></returns>
		public static IApplicationBuilder MapWebSocketService(this IApplicationBuilder app,string path="/ws", WebSocketService service = null)
		{

			var mgr =app.ApplicationServices.GetService<WebSocketManager>();
			if (service == null)
			{
				mgr.WebSocketPaths.TryAdd(typeof(WebSocketService),path);
				return app.Map(path, a => a.UseMiddleware<WebSocketMiddleware>());
			}
			mgr.WebSocketPaths.TryAdd(service.GetType(), path);

			return app.Map(path, a => a.UseMiddleware<WebSocketMiddleware>(service));
		}
	}
}