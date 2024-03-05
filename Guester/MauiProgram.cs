using banditoth.MAUI.DeviceId;
using CommunityToolkit.Maui;
using FFImageLoading.Maui;
using Guester.Tamplates;
using Microsoft.AppCenter.Crashes;
using Microsoft.AppCenter;
using Microsoft.Extensions.Logging;
using Mopups.Hosting;
using Syncfusion.Maui.Core.Hosting;
using Microsoft.AppCenter.Analytics;
namespace Guester;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.UseMauiCommunityToolkit()
			.ConfigureMopups()
            .ConfigureDeviceIdProvider()
            .UseFFImageLoading()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("FontAwesome6BrandsRegular400.otf", "FAB");
                fonts.AddFont("FontAwesome6FreeSolid900.otf", "FAS");
                fonts.AddFont("FontAwesome6FreeRegular400.otf", "FAR");
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        builder.Services.AddSingleton<IConnectivity>(Connectivity.Current);

        builder.Services.AddSingleton<HomePageViewModel>();
        builder.Services.AddSingleton<HomePage>();

        builder.Services.AddSingleton<DeviceAuthViewModel>();
        builder.Services.AddSingleton<DeviceAuthPage>();
        DependencyService.Register<IPrintService, PrintService>();
        DependencyService.Register<ICacheService>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddSingleton<LoginPage>();

        builder.Services.AddSingleton<OrdersContentViewModel>();
        builder.Services.AddSingleton<OrdersContent>();

        builder.Services.AddSingleton<ChecksContentViewModel>();
        builder.Services.AddSingleton<ChecksContent>();

        builder.Services.AddSingleton<OrderDetailPageViewModel>();
        builder.Services.AddSingleton<OrderDatail>();

        builder.Services.AddSingleton<WarehouseContentViewModel>();
        builder.Services.AddSingleton<WarehouseContent>();   
        
        builder.Services.AddSingleton<PayPageViewModel>();
        builder.Services.AddSingleton<PayPageTemplate>();     
        
        builder.Services.AddSingleton<HallMapContent>();
        builder.Services.AddSingleton<HallMapContentViewModel>();

        //builder.Services.AddSingletonWithShellRoute<OrderDatailPage, OrderDetailPageViewModel>(nameof(OrderDatailPage));

        //builder.Services.AddTransientWithShellRoute<ReportDetailPage, ReportDetailViewModel>(nameof(ReportDetailPage));

        AppCenter.Start("uwp={9559e1d4-776f-4645-ab4c-807f08c685f3;" +
                        "android=f869dc3b-0b0c-4f99-84c5-d5761726ebcf;" +
                        "ios=b6e7e0ea-5fa4-42f3-a516-76e2f267e6b8;" +
                        "macos=23116a44-4382-4e9e-a288-b89fbf85528a;",
                        typeof(Analytics), typeof(Crashes));

        System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);

#if DEBUG
        builder.Logging.AddDebug();
#endif
        builder.ConfigureSyncfusionCore();
        return builder.Build();
	}

}
