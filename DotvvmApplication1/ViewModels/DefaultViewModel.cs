using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DotvvmApplication1.Hubs;
using DotVVM.Framework.ViewModel;

namespace DotvvmApplication1.ViewModels
{
	public class DefaultViewModel : MasterpageViewModel
	{
		public int A { get; set; }
		public int B { get; set; }
		public int C { get; set; }

		public MyHub Hub { get; set; }

		public List<string> Messages { get; set; }
		public string Message { get; set; }


		public DefaultViewModel(MyHub hub)
		{
			Messages = new List<string>();
			Hub = hub;
		}

		public async Task SendMessage()
		{
			Messages.Add(Message);
			await Hub.UpdateCurrentViewModelOnClient();
		}

		public void Sum()
		{
			C = A + B;
		}
	}
}