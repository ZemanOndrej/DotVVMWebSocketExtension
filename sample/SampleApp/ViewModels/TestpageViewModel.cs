using System;
using DotVVMWebSocketExtension.WebSocketService;

namespace SampleApp.ViewModels
{
    public class TestpageViewModel : MasterpageViewModel
    {
	    public string Text { get; set; }
		public WebSocketService Service { get; set; }

	    public TestpageViewModel(WebSocketService service)
	    {
		    Service = service;
	    }

	    public void Start()
	    {
		    Console.WriteLine(Context.HttpContext.Request);
//		    var updateViewModelInTaskFromCurrentClientAsync = 
	    }
	}
}

