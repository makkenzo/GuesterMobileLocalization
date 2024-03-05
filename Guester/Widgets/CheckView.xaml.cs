using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Storage;
using Mopups.Services;

using System.Xml.Linq;


namespace Guester.Widgets;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class CheckView 
{
    CheckViewModel vm;
    public CheckView(string html)
    {
        InitializeComponent();
        vm = (CheckViewModel)BindingContext;
        vm.HttmlPath = html;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        vm.OnAppearing();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
    }
    private async void Share_Tapped(object sender, EventArgs e)
    {
        await Task.Delay(200);
        if (webView != null)
        {
            string jsFunction = "convertHTMLtoPDF()";
            await webView.EvaluateJavaScriptAsync(jsFunction);
        }


    }

    private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
    {

    }
}
