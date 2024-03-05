using Guester.Resources;
using Guester.Views;
using System.Reflection;
#if ANDROID
using Microsoft.Maui.Controls.Compatibility.Platform.Android;
#endif
namespace Guester;

public partial class App : Application
{
    private static string CurrentSalePointId { get => Preferences.Get(nameof(CurrentSalePointId), ""); set => Preferences.Set(nameof(CurrentSalePointId), value); }
    private static string CurrentBrandId { get => Preferences.Get(nameof(CurrentBrandId), ""); set => Preferences.Set(nameof(CurrentBrandId), value); }
    public static string CurrentCashShiftId { get => Preferences.Get(nameof(CurrentCashShiftId), ""); set => Preferences.Set(nameof(CurrentCashShiftId), value); }
    public App()
    {



        Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Mjk2OTc4MkAzMjMzMmUzMDJlMzBZOEwxRG05dUlSQVl0TEJ0NXU2Y2xZQzZCdlF2c3VmSW5GdW90NFdxTFRnPQ==");

        InitializeComponent();


        Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {

#if ANDROID
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#endif

#if IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif


        });

        Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {

#if ANDROID
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#endif

#if IOS || MACCATALYST
            //handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif


        });

        Microsoft.Maui.Handlers.PickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {

#if ANDROID
            handler.PlatformView.BackgroundTintList = Android.Content.Res.ColorStateList.ValueOf(Colors.Transparent.ToAndroid());
#endif

#if IOS || MACCATALYST
            handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif


        });

        Microsoft.Maui.Handlers.DatePickerHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
        {

#if ANDROID
            handler.PlatformView.TextAlignment = Android.Views.TextAlignment.Center;

#endif

#if IOS || MACCATALYST
            // handler.PlatformView.TextAlignment = UIKit.UITextAlignment.Center;

#endif
        });
        try
        {
            Task.Run(async () =>
            {
                await RealmService.Init();

            }).Wait();
            if (string.IsNullOrWhiteSpace(CurrentBrandId) || string.IsNullOrWhiteSpace(CurrentSalePointId) || RealmService.CurrentUser == null)
            {
                MainPage = new DeviceAuthPage(ServiceHelper.GetService<DeviceAuthViewModel>());
                return;
            }
         else
            {
                Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;
                Task.Run(async () =>
                {
                    await RealmService.Init();

                }).Wait();
                MainPage = new AppShell();
            }
        }catch(Exception ex) 
        {
            MainPage = new DeviceAuthPage(ServiceHelper.GetService<DeviceAuthViewModel>());
            return;
        }




    }


    public static string AppVersionn()
    {

        var assembly = typeof(App).GetTypeInfo().Assembly;
        var version = assembly.GetName().Version;
        return $"{version.Major}.{version.Minor}.{version.Build}";

    }
    public static string GetBuild()
    {
       
            var assembly = typeof(App).GetTypeInfo().Assembly;
            return assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        
    }

}
