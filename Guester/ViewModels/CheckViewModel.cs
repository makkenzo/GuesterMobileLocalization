using Microsoft.AppCenter.Crashes;
using Mopups.Services;

namespace Guester.ViewModels;

public partial class CheckViewModel: BaseViewModel
{
    [ObservableProperty]
    HtmlWebViewSource htmlWebViewSource;

    [ObservableProperty]
    private string httmlPath;


    internal void OnAppearing()
    {
        try
        {
            HtmlWebViewSource = new HtmlWebViewSource();
            if (!string.IsNullOrWhiteSpace(HttmlPath))
            {
                HtmlWebViewSource.Html = HttmlPath;
            }
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private async Task Close()
    {
        try
        {
            await MopupService.Instance.PopAsync();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }
}
