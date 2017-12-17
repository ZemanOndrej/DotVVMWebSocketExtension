using Newtonsoft.Json.Linq;


namespace DotVVMWebSocketExtension.WebSocketService
{
	/// <summary>
	/// Stores ViewModel state 
	/// </summary>
	public class ViewModelState
    {
	    public JObject LastSentViewModelJson { get; set; }

	    public JObject ChangedViewModelJson { get; set; }

	    public object LastSentViewModel { get; set; }

	    public string CsrfToken { get; set; }

	    public bool IsOk => LastSentViewModel != null;

    }
}
