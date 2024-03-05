using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Guester
{
    public static class PrintSevice
    {

        public static void PrintText(string plain_text, CashRegister cashRegister)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(cashRegister.CashRegisterSetting.IpAddress))
                {
                    throw new Exception("No Ip address");
                }

                // Connect to the printer using TCP socket
                using (var client = new TcpClient(cashRegister.CashRegisterSetting.IpAddress, 9100))  // Assuming the printer uses port 9100
                using (var stream = client.GetStream())
                {


                    var s = Encoding.Default;
                    byte[] postData = System.Text.Encoding.GetEncoding(866).GetBytes(plain_text);

                    stream.Write(postData);
                }
            }
            catch (Exception ex)
            {
                DialogService.ShowToast(ex.Message);

                // Handle connection or printing error
                //  Console.WriteLine($"Error printing directly to printer: {ex.Message}");
            }
        }
    }
}
