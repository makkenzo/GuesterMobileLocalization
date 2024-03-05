using System;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Printing;
using Windows.Devices.WiFi;
using Windows.Graphics.Printing;
using Windows.Networking.Connectivity;

[assembly: Dependency(typeof(IPrintService))]
namespace Guester
{

    public class PrintService : IPrintService
    {
        public async Task PrintHtml(string htmlContent, string jobName)
        {

            return;
            var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
            var printManager = PrintManagerInterop.GetForWindow(hWnd);
            //var printManager = PrintManager.GetForCurrentView();
            printManager.PrintTaskRequested += (sender, args) =>
            {
                var printTask = args.Request.CreatePrintTask(jobName, async context =>
                {
                    var webView = new WebView();
                    webView.Source = new HtmlWebViewSource { Html = htmlContent };

                    // Wait for the WebView to load content
                    await Task.Delay(2000);

                    var printDocument = new PrintDocument();
                    printDocument.Paginate += (s, e) =>
                    {
                        printDocument.SetPreviewPageCount(1, PreviewPageCountType.Intermediate);
                    };

                    printDocument.GetPreviewPage += (s, e) =>
                    {
                        // Use WebView directly for preview
                        var printPage = CreateNativeUIElement(webView);

                        printDocument.SetPreviewPage(e.PageNumber, printPage);
                    };

                    printDocument.AddPages += (s, e) =>
                    {
                        // Use WebView directly for printing
                        var printPage = CreateNativeUIElement(webView);
                        printDocument.AddPage(printPage);
                        printDocument.AddPagesComplete();
                    };
                });
            };

            await PrintManager.ShowPrintUIAsync();
        }

        private UIElement CreateNativeUIElement(WebView webView)
        {
            var handler = webView?.Handler;
            if (handler != null)
            {
                var nativeView = handler.ContainerView; // Check if ContainerView is the correct property
                return nativeView as UIElement;
            }
            return null;
        }
    }
}
