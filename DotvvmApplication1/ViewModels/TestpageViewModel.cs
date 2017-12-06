using System;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1.ViewModels
{
    public class TestpageViewModel : MasterpageViewModel
    {
	    public string Text { get; set; }

		public WebSocketHub Hub { get; set; }

	    public TestpageViewModel(WebSocketHub hub)
	    {
		    Hub = hub;
	    }

	    public void Start()
	    {
		    Console.WriteLine(Context.HttpContext.Request);
		    var viewModelFromClientAsync = Hub.GetViewModelFromClientAsync();
	    }
	}
}

