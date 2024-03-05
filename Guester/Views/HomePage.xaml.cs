
using Realms.Sync;
using Syncfusion.Maui.DataSource.Extensions;

namespace Guester.Views;



public partial class HomePage : ContentPage
{
    HomePageViewModel vm;
    private IConnectivity connectivity;

    public HomePage(HomePageViewModel _vm, IConnectivity _connectivity)
    {

        InitializeComponent();
        BindingContext = vm = _vm;
        connectivity = _connectivity;

    }


  

    protected override  void OnAppearing()
    {
        if (vm is null)
            return;
        base.OnAppearing();
         vm.OnAppearing();

      /*  vm.webView = weview;*/
        // on android 34 error when register receiver
        // connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
    }

    private void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e)
    {
        vm.IsInternet = e.NetworkAccess == NetworkAccess.Internet;
    }

    protected override void OnDisappearing()
    {
        if (vm is null)
            return;
        base.OnDisappearing();
        vm.OnDissapearing();
        connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;

    }

    private void Entry_Focused(object sender, FocusEventArgs e)
    {
        if (!e.IsFocused)
        {
            
        }
    }

    private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
       
    }
}
