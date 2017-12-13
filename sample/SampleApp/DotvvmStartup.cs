using DotVVM.Framework.Configuration;
using DotVVM.Framework.ResourceManagement;
using DotVVMWebSocketExtension.WebSocketService;

namespace SampleApp
{
    public class DotvvmStartup : IDotvvmStartup
    {
        // For more information about this class, visit https://dotvvm.com/docs/tutorials/basics-project-structure
        public void Configure(DotvvmConfiguration config, string applicationPath)
        {
            ConfigureRoutes(config, applicationPath);
            ConfigureControls(config, applicationPath);
            ConfigureResources(config, applicationPath);
        }

        private void ConfigureRoutes(DotvvmConfiguration config, string applicationPath)
        {
            config.RouteTable.Add("Chat", "chat", "Views/chatPage.dothtml");
			config.RouteTable.Add("Event","","Views/serverEvents.dothtml");
			config.RouteTable.Add("TestPage","testPage","Views/testPage.dothtml");
			config.RouteTable.Add("Calculator","calculator","Views/simpleCalculator.dothtml");

            // Uncomment the following line to auto-register all dothtml files in the Views folder
            // config.RouteTable.AutoDiscoverRoutes(new DefaultRouteStrategy(config));    
        }

        private void ConfigureControls(DotvvmConfiguration config, string applicationPath)
        {
            // register code-only controls and markup controls
        }

        private void ConfigureResources(DotvvmConfiguration config, string applicationPath)
        {
			// register custom resources and adjust paths to the built-in resources

			config.Resources.Register("websocketScript", new ScriptResource
			{
				Location = new EmbeddedResourceLocation(typeof(WebSocketMiddleware).Assembly, "DotVVMWebSocketExtension.Resources.websocketScript.js"),
				Dependencies = new string[0]
			});
	        config.Resources.Register("bootstrap-css", new StylesheetResource()
	        {
		        Location = new UrlResourceLocation("~/Styles/bootstrap.min.css")
	        });
	        config.Resources.Register("bootstrap", new ScriptResource()
	        {
		        Location = new UrlResourceLocation("~/Scripts/bootstrap.min.js"),
		        Dependencies = new[] { "bootstrap-css" }
	        });

		}
    }
}
