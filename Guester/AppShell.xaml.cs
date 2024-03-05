using Guester.Services;
using Guester.Views;
using Mopups.PreBaked.Interfaces;

namespace Guester;

public partial class AppShell : Shell
{


    public AppShell()
	{
		InitializeComponent();




        //main.Items.Add(new ShellContent()
        //{
        //    Title = "Login",
        //    ContentTemplate = new DataTemplate(() => new LoginPage()),
        //    Route = nameof(LoginPage)

        //});
        //main.Items.Add(new ShellContent()
        //{
        //    Title = "Home",
        //    ContentTemplate = new DataTemplate(() => new HomePage()),
        //    Route = nameof(HomePage)

        //});

     //Routing.RegisterRoute(nameof(OrderDetailPage), typeof(OrderDetailPage));
     //Routing.RegisterRoute(nameof(PayPage), typeof(PayPage));

        var dip = new banditoth.MAUI.DeviceId.DeviceIdProvider();
        RealmService.CurrentDivaceId = dip.GetDeviceId();
    }
}
