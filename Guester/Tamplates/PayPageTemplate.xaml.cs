namespace Guester.Tamplates;

public partial class PayPageTemplate : ContentView
{
	PayPageViewModel vm;
	public PayPageTemplate()
	{
		InitializeComponent();
		vm= HomePageViewModel.getInstancePayPageInstance();
		BindingContext = vm;
	}
}