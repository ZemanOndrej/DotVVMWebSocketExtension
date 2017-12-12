using System;
using DotVVMWebSocketExtension.WebSocketService;

namespace SampleApp.ViewModels
{
    public class TestpageViewModel : MasterpageViewModel
    {
	    public string Text { get; set; }

		public WebSocketService Hub { get; set; }

	    public TestpageViewModel(WebSocketService hub)
	    {
		    Hub = hub;
	    }

	    public void Start()
	    {
		    Console.WriteLine(Context.HttpContext.Request);
//		    var viewModelFromClientAsync = Hub.UpdateViewModelInTaskFromCurrentClientAsync();
	    }
	}
}

