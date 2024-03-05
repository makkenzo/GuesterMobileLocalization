namespace Guester.Views;

public partial class LoginPage : ContentPage
{
	LoginViewModel vm;

    public LoginPage()
    {
        InitializeComponent();
        vm = new LoginViewModel();
        BindingContext = vm;

    }


    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.OnAppearing();        
    }
}
