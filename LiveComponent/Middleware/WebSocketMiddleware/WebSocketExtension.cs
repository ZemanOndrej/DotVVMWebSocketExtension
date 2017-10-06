using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace LiveComponent.Middleware
{
    public static class WebSocketExtension
    {
        public static IServiceCollection AddWebSocketManager(this IServiceCollection services)
        {
            services.AddTransient<WebSocketService>();

            foreach (var type in Assembly.GetEntryAssembly().ExportedTypes)
            {
                if (type.GetTypeInfo().BaseType == typeof(WebSocketHub))
                {
                    services.AddSingleton(type);
                }
            }

            return services;
        }

        public static IApplicationBuilder MapWebSocketHub(this IApplicationBuilder app, PathString path,
            WebSocketHub handler)
        {
            return app.Map(path, (_app) => _app.UseMiddleware<WebSocketHubMiddleware>(handler));
        }
    }
}