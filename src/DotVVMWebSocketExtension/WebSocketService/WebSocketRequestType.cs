namespace DotVVMWebSocketExtension.WebSocketService
{
	public static class WebSocketRequestType
	{
		public static string WebSocketInit => "webSocketInit";
		public static string WebSocketViewModelSync => "viewModelSynchronizationRequest";
		public static string SuccessfulCommand => "successfulCommand";
		public static string Ping => "ping";
	}
}