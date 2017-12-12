using BL.Facades;
using DAL;
using DotVVMWebSocketExtension.WebSocketService;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace SampleApp
{
	public class Startup
	{

		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		// For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddWebEncoders();

			services.AddWebSocketManagerService();


			services.AddEntityFrameworkSqlServer()
				.AddDbContext<ChatDbContext>(options =>
					options.UseInMemoryDatabase("ChatDatabase"));
			services.AddScoped<ChatFacade>();
//			services.AddDbContext<ChatDbContext>(options =>
//				options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

			services.AddDotVVM(options => { options.AddDefaultTempStorages("Temp"); });
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory)
		{
			loggerFactory.AddConsole();

			// use DotVVM
			var dotvvmConfiguration = app.UseDotVVM<DotvvmStartup>(env.ContentRootPath);
			app.UseWebSockets();
			app.MapWebSocketService();
			app.MapWebSocketService("/wsChat", app.ApplicationServices.GetService<ChatService>());
			app.UseStaticFiles();
			app.UseDefaultFiles();
		}
	}
}