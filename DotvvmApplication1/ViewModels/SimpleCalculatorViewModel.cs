using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotVVM.Framework.ViewModel;

namespace DotvvmApplication1.ViewModels
{
    public class SimpleCalculatorViewModel : DotvvmViewModelBase
    {
	    public int A { get; set; }
	    public int B { get; set; }
	    public int C { get; set; }

	    public void Sum()
	    {
		    C = A + B;
		    Console.WriteLine(Context.HttpContext.Request);
	    }
	}
}

