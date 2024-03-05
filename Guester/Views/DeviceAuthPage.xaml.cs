namespace Guester.Views;

public partial class DeviceAuthPage : ContentPage
{

	private DeviceAuthViewModel vm;

	public DeviceAuthPage(DeviceAuthViewModel _vm)
	{
		InitializeComponent();
		BindingContext = vm = _vm;
	}

    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.OnAppearing();
    }

    void SwichPassVisible(System.Object sender, System.EventArgs e)
    {
        PassEntry.IsPassword = !PassEntry.IsPassword;
    }


    protected override void OnDisappearing()
    {
        base.OnDisappearing();
       // vm.OnDisappearing();
    }

   

}
