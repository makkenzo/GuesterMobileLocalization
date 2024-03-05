using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Net;
using Android.Net.Wifi;
using Android.OS;
using Android.Print;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

[assembly: Dependency(typeof(IPrintService))]
namespace Guester
{
    public class PrintService : IPrintService
    {

        public Task PrintHtml(string htmlContent, string jobName)
        {
            try
            {
                // Connect to the printer using TCP socket
                var ip = Preferences.Get("ip", "");
                if (string.IsNullOrWhiteSpace(ip))
                {
                    DialogService.ShowToast("No Ip address");
                    throw new Exception("No IP");
                }
                using (var client = new TcpClient($"{ip}", 9100))  // Assuming the printer uses port 9100
                using (var stream = client.GetStream())
                {


                    var s = Encoding.Default;
                    byte[] postData = System.Text.Encoding.GetEncoding(866).GetBytes(htmlContent);

                    stream.Write(postData);
                }
            }
            catch (Exception ex)
            {
                // Handle connection or printing error
                //  Console.WriteLine($"Error printing directly to printer: {ex.Message}");
            }
            return Task.CompletedTask;
            var webView = new Android.Webkit.WebView(MainActivity.Instance);
            webView.Settings.JavaScriptEnabled = true;
            webView.LoadDataWithBaseURL(null, htmlContent, "text/html", "UTF-8", null);


            var printManager = (PrintManager)MainActivity.Instance.GetSystemService(Context.PrintService);
            var printAdapter = new CustomPrintDocumentAdapter(webView.CreatePrintDocumentAdapter(), jobName);


            printManager.Print(jobName, printAdapter, null);
            return Task.CompletedTask;
        }


        public class CustomPrintDocumentAdapter : PrintDocumentAdapter
        {
            private readonly PrintDocumentAdapter _originalPrintDocumentAdapter;
            private readonly string _jobName;

            public CustomPrintDocumentAdapter(PrintDocumentAdapter originalPrintDocumentAdapter, string jobName)
            {
                _originalPrintDocumentAdapter = originalPrintDocumentAdapter;
                _jobName = jobName;
            }

            public override void OnLayout(PrintAttributes oldAttributes, PrintAttributes newAttributes, CancellationSignal cancellationSignal, LayoutResultCallback callback, Bundle extras)
            {
                _originalPrintDocumentAdapter.OnLayout(oldAttributes, newAttributes, cancellationSignal, callback, extras);
            }

            public override void OnWrite(PageRange[] pages, ParcelFileDescriptor destination, CancellationSignal cancellationSignal, WriteResultCallback callback)
            {
                _originalPrintDocumentAdapter.OnWrite(pages, destination, cancellationSignal, callback);
            }

            public override void OnFinish()
            {
                _originalPrintDocumentAdapter.OnFinish();
            }
        }
    }
}
