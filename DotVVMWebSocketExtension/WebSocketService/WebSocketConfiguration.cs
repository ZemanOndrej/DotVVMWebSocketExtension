using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace DotVVMWebSocketExtension.WebSocketService
{
    public class WebSocketConfiguration
    {
		public Dictionary<Type, PathString> WebsocketPaths { get; set; }

	    public WebSocketConfiguration()
	    {
		    WebsocketPaths = new Dictionary<Type, PathString>();
	    }
    }
}
