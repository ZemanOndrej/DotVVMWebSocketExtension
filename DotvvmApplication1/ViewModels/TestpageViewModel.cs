using System;
using DotVVMWebSocketExtension.WebSocketService;

namespace DotvvmApplication1.ViewModels
{
    public class TestpageViewModel : MasterpageViewModel
    {
	    public string Text { get; set; }

		public WebSocketFacade Hub { get; set; }

	    public TestpageViewModel(WebSocketFacade hub)
	    {
		    Hub = hub;
	    }

	    public void Start()
	    {
		    Console.WriteLine(Context.HttpContext.Request);
		    var viewModelFromClientAsync = Hub.UpdateViewModelInTaskFromCurrentClientAsync();
	    }
	}
}

