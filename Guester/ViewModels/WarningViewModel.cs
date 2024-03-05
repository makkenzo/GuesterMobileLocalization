using Microsoft.AppCenter.Crashes;
using Mopups.Services;

namespace Guester.ViewModels;

public partial class WarningViewModel:BaseViewModel
{
    [ObservableProperty]
    private string message,title;

    [ObservableProperty]
    private bool isNeedConfirm;

   

    public bool Result { get; set; }

    public WarningViewModel()
    {
    }

    [RelayCommand]
    private async Task CloseTapped(bool res)
    {
        try
        {
            Result = res;
            await MopupService.Instance.PopAsync();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }
}
