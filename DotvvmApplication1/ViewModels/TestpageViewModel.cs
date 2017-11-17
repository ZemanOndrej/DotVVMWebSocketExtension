using System;

namespace DotvvmApplication1.ViewModels
{
    public class TestpageViewModel : MasterpageViewModel
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

