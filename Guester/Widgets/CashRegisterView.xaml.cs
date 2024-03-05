using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Storage;
using Mopups.Services;
using System.Xml.Linq;


namespace Guester.Widgets;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CashRegisterView
{
    CashRegistorViewModel vm;
    private TaskCompletionSource<CashRegisterPopupData> _taskCompletionSource;
    public Task<CashRegisterPopupData> PopupClosedTask => _taskCompletionSource.Task;
    public CashRegisterView(string _title,bool can_close)
    {
        InitializeComponent();
        vm = (CashRegistorViewModel)BindingContext;
        vm.Title = _title;
        vm.CanClose = can_close;
    
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        //vm.OnAppearing();

        _taskCompletionSource = new TaskCompletionSource<CashRegisterPopupData>();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _taskCompletionSource.SetResult(vm.CashRegisterPopupData);
    }
}


 

    //private async void Share_Tapped(object sender, EventArgs e)
    //{
    //    await Task.Delay(200);
    //    if (webView != null)
    //    {
    //        string jsFunction = "convertHTMLtoPDF()";
    //        await webView.EvaluateJavaScriptAsync(jsFunction);
    //    }


    //}

    //private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    //{

    //}

