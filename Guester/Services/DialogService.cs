using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Views;
using Guester.Widgets;
using Mopups.Services;

namespace Guester;

public static class DialogService
{
    public static Task ShowAlertAsync(string title, string message, string accept) => Application.Current.MainPage.DisplayAlert(title, message, accept);

    public static async Task<bool> ShowWarningAsync(string title, string message, bool confirm)
    {
        var popup = new WarningView(title, message, confirm);
        await MopupService.Instance.PushAsync(popup, false);
        return await popup.PopupClosedTask;
    }

    public  static void OpenFunctionalMenu()
    {
        var popup = new FunctionalMenu();
        AppShell.Current.ShowPopup(popup);
        //await AppShell.Current.GoToAsync($"{nameof(TestPopup)}");
    }

    public static async Task ShowHtmlAsync(string html)
    {
        var popup = new CheckView(html);
        await MopupService.Instance.PushAsync(popup,false);
    }

    public static async Task<CashRegisterPopupData> ShowCaseRegisterView(string _title, bool canClose = false)
    {
        var popup = new CashRegisterView(_title, canClose);
        await MopupService.Instance.PushAsync(popup, false);
        return await popup.PopupClosedTask;
    }   
    
    
    public static async Task<bool> ShowAdminView( )
    {
        var popup = new AdminView();
        await MopupService.Instance.PushAsync(popup, false);
        return await popup.PopupClosedTask;
    }

    public static Action ShowActivityIndicator()
    {
        //var popup = new BusyPopup();
        //Application.Current.MainPage.ShowPopup(popup);
        //return () => popup.Close();
        var popup = new BusyMopup();

        MopupService.Instance.PushAsync(popup, false) ;
        return () => popup.Close();
    }

    public static Task ShowToast(string text) => Toast.Make(text, ToastDuration.Short).Show();
    public static Task ShowToast(string text, ToastDuration t) => Toast.Make(text, t).Show();

}
