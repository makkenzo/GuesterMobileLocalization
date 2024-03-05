using Newtonsoft.Json;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using static ArcaCashRegister.CreateWebsocket;

namespace ArcaCashRegister
{
    public class CreateWebsocket
    {
        private ClientWebSocket clientWebSocket = null;
        private Settings settings = null;
        public bool isSendingData { get; set; } = false;


        public CreateWebsocket()
        {

        }
        public CreateWebsocket(Settings _settings)
        {
            this.settings = _settings;
        }
        /// <summary>
        /// Start new connection
        /// </summary>
        public bool Start()
        {
            if (this.settings != null)
            {
                using (var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30)))
                {
                    this.clientWebSocket?.Dispose();
                    this.clientWebSocket = new ClientWebSocket();
                    this.clientWebSocket.Options.KeepAliveInterval = TimeSpan.FromSeconds(7);
                    this.clientWebSocket.ConnectAsync(this.settings.ConnectionString, cancellationTokenSource.Token).Wait();

                    //Task.Run(() => StartSendingPing(this));

                    return true;
                }
            }
            return false;
        }
        public string Send(dynamic data, int type)
        {
            try
            {
                if (Start())
                {
                    if (type == 0) // Sale
                    {
                        var loginResponse = SendData(JsonConvert.SerializeObject(
                            new Request<LoginData>
                            {
                                id = Guid.NewGuid().ToString(),
                                command = "Login",
                                data = new LoginData
                                {
                                    login = settings.Login,
                                    password = settings.Password
                                }
                            }));
                        var r = JsonConvert.DeserializeObject<WrongResponce>(loginResponse);
                        if (r.status == "Fail")
                        {
                            return loginResponse;
                        }
                        else
                        {
                            var order = data;
                            var sale = new Request<SaleData>();
                            sale.id = Guid.NewGuid().ToString();
                            sale.command = "Sale";

                            var saleData = new SaleData
                            {
                                Amount = (long)order.OrderReceipt.ResultSum * 100,
                                DiscountAmount = (long)order.OrderReceipt.DiscountSum * 100,
                                TotalAmount = (long)order.OrderReceipt.ResultSum * 100,
                                SaleType = "Sale",
                                ShouldPrintReceipt = true,
                                PrintOptions = null
                            };

                            foreach (var item in order.OrderReceipt.OrderReceiptPayments)
                            {
                                saleData.Payments.Add(new PaymentData { Amount = (long)item.Sum * 100, PaymentType = item.PaymentMethodType });
                            }

                            foreach (var item in order.OrderSales)
                            {
                                saleData.Products.Add(new ProductData
                                {
                                    Id = item.ProductId,
                                    Amount = (long)item.Amount,
                                    Name = item.Name,
                                    Barcode = "",
                                    Price = (long)item.Price * 100,
                                    Units = item.ProductUnit,
                                    Psid = item.Psu,
                                    Labels = new List<string>(),
                                    Vat = (decimal)item.Vat,
                                    DiscountAmount = (long)item.DiscountSum * 100,
                                    IsDecimalUnits = false,
                                    UnitCode = null,
                                    PackageCode = null,
                                    CommissionPINFL = null,
                                    CommissionTIN = null
                                });
                            }
                            sale.data = saleData;
                            return SendData(JsonConvert.SerializeObject(sale));
                        }
                    }
                    else if (type == 1) // Refund
                    {
                        var loginResponse = SendData(JsonConvert.SerializeObject(
                            new Request<LoginData>
                            {
                                id = Guid.NewGuid().ToString(),
                                command = "Login",
                                data = new LoginData
                                {
                                    login = settings.Login,
                                    password = settings.Password
                                }
                            }));
                        var r = JsonConvert.DeserializeObject<WrongResponce>(loginResponse);
                        if (r.status == "Fail")
                        {
                            return loginResponse;
                        }
                        else
                        {
                            var refund = new Request<Refund>();
                            refund.id = Guid.NewGuid().ToString();
                            refund.command = "Refund";

                            var refundData = new Refund
                            {
                                saleId = data.saleId,
                                amount = (long)data.OrderReceipt.ResultSum * 100,
                                shouldPrintReceipt = true,
                                printOptions = null
                            };

                            foreach (var item in data.OrderReceipt.OrderReceiptPayments)
                            {
                                refundData.payments.Add(
                                    new PaymentRefund
                                    {
                                        amount = (long)item.Sum * 100,
                                        paymentType = item.PaymentMethodType,
                                        transactionNumber = null
                                    });
                            }

                            foreach (var item in data.OrderSales)
                            {
                                refundData.products.Add(new ProductData
                                {
                                    Id = item.ProductId,
                                    Amount = (long)item.Amount,
                                    Name = item.Name,
                                    Barcode = "",
                                    Price = (long)item.Price * 100,
                                    Units = item.ProductUnit,
                                    Psid = item.Psu,
                                    Labels = new List<string>(),
                                    Vat = (decimal)item.Vat,
                                    DiscountAmount = (long)item.DiscountSum * 100,
                                    IsDecimalUnits = false,
                                    UnitCode = null,
                                    PackageCode = null,
                                    CommissionPINFL = null,
                                    CommissionTIN = null
                                });
                            }
                            refund.data = refundData;
                            return SendData(JsonConvert.SerializeObject(refund));
                        }
                    }
                    else if (type == 2) // X-Report
                    {
                        var loginResponse = SendData(JsonConvert.SerializeObject(
                           new Request<LoginData>
                           {
                               id = Guid.NewGuid().ToString(),
                               command = "Login",
                               data = new LoginData
                               {
                                   login = settings.Login,
                                   password = settings.Password
                               }
                           }));
                        var r = JsonConvert.DeserializeObject<WrongResponce>(loginResponse);
                        if (r.status == "Fail")
                        {
                            return loginResponse;
                        }
                        else
                        {
                            var printXReport = new Request<LoginData>
                            {
                                id = Guid.NewGuid().ToString(),
                                command = "PrintXReport",
                                data = null
                            };

                            return SendData(JsonConvert.SerializeObject(printXReport));
                        }
                    }
                    else if (type == 3) // Z-Report
                    {
                        var loginResponse = SendData(JsonConvert.SerializeObject(
                             new Request<LoginData>
                             {
                                 id = Guid.NewGuid().ToString(),
                                 command = "Login",
                                 data = new LoginData
                                 {
                                     login = settings.Login,
                                     password = settings.Password
                                 }
                             }));
                        var r = JsonConvert.DeserializeObject<WrongResponce>(loginResponse);
                        if (r.status == "Fail")
                        {
                            return loginResponse;
                        }
                        else
                        {
                            var printZReport = new Request<LoginData>
                            {
                                id = Guid.NewGuid().ToString(),
                                command = "CloseZReport",
                                data = null
                            };

                            return SendData(JsonConvert.SerializeObject(printZReport));
                        }
                    }
                    else if (type == 4) // Logout
                    {
                        var loginResponse = SendData(JsonConvert.SerializeObject(
                            new Request<LoginData>
                            {
                                id = Guid.NewGuid().ToString(),
                                command = "Login",
                                data = new LoginData
                                {
                                    login = settings.Login,
                                    password = settings.Password
                                }
                            }));
                        var r = JsonConvert.DeserializeObject<WrongResponce>(loginResponse);
                        if (r.status == "Fail")
                        {
                            return loginResponse;
                        }
                        else
                        {
                            var logout = new Request<LoginData>
                            {
                                id = Guid.NewGuid().ToString(),
                                command = "Logout",
                                data = null
                            };
                            return SendData(JsonConvert.SerializeObject(logout));
                        }
                    }
                    else
                        return "Invalid command.";
                }
                else
                {
                    return "Connect failed.";
                }
            }
            catch (Exception ex)
            {

                return ex.Message;
            }
        }
        /// <summary>
        /// Abort current connection
        /// </summary>
        public void Stop()
        {
            this.clientWebSocket?.Abort();
            this.clientWebSocket?.Dispose();
        }
        /// <summary>
        /// Send data to websocket chanel
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public string SendData(string data, int iterator = 0)
        {
            Dictionary<string, object> dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(data);
            data = JsonConvert.SerializeObject(dict);

            var receiveBody = new byte[65536];

            try
            {
                byte[] sendBody = Encoding.UTF8.GetBytes(data);
                this.clientWebSocket.SendAsync(new ArraySegment<byte>(sendBody), WebSocketMessageType.Text, true, CancellationToken.None).Wait();

                Task.Delay(1000).Wait();
            }
            catch (Exception e)
            {
                if (iterator < 3)
                {
                    this.Start();
                    Task.Delay(2000).Wait();
                    return this.SendData(data, iterator++);
                }
                return JsonConvert.SerializeObject(new WrongResponce { error = e.Message });
            }

            Task<WebSocketReceiveResult> receive = this.clientWebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBody), CancellationToken.None);
            receive.Wait();
            WebSocketReceiveResult result = receive.Result;

            string responce = string.Empty;
            if (result.EndOfMessage)
            {
                Task.Delay(1000).Wait();
                responce = Encoding.UTF8.GetString(receiveBody, 0, result.Count);
            }

            return responce;
        }

        /// <summary>
        /// Send ping for infinity connection
        /// </summary>
        /// <param name="client"></param>
        /// <returns></returns>
        private static async Task StartSendingPing(CreateWebsocket gwc)
        {
            try
            {
                byte[] sendPingBody = Encoding.UTF8.GetBytes("ping");
                while (true)
                {
                    await Task.Delay(1000);

                    if (gwc.clientWebSocket.State == WebSocketState.Closed || gwc.isSendingData)
                        continue;

                    gwc.clientWebSocket.SendAsync(
                        new ArraySegment<byte>(sendPingBody),
                        WebSocketMessageType.Binary,
                        true,
                        CancellationToken.None)
                    .Wait();
                }
            }
            catch
            {
                gwc.Start();
            }
        }
        /// <summary>
        /// Get connection state
        /// </summary>
        /// <returns></returns>
        public bool Connected()
        {
            return clientWebSocket != null ? this.clientWebSocket.State == WebSocketState.Open : false;
        }

        /// <summary>
        /// Connection parameters
        /// </summary>
        [Serializable]
        public class Settings
        {
            public string Protocol { get; set; }
            public string Host { get; set; }
            public string Port { get; set; }
            public string Sub { get; set; }
            public string Login { get; set; }
            public string Password { get; set; }

            public Uri ConnectionString
            {
                get
                {
                    return new Uri(
                $"{Protocol}{Host}" +
                $"{(string.IsNullOrEmpty(Port) ? "" : ":" + Port)}" +
                $"{(string.IsNullOrWhiteSpace(Sub) ? "" : "/" + Sub)}"
                );
                }
            }
        }
        /// <summary>
        /// Return fail
        /// </summary>
        [Serializable]
        public class WrongResponce
        {
            public string? requestId { get; set; } = Guid.NewGuid().ToString();
            public string? command { get; set; } = "Any";
            public string? status { get; set; } = "Fail";
            public string? error { get; set; }
            public string? payload { get; set; } = null;
        }
        [Serializable]
        public class Request<T>
        {
            public string id { get; set; }
            public string command { get; set; }
            public T data { get; set; }
        }
        [Serializable]
        public class LoginData
        {
            public string login { get; set; }
            public string password { get; set; }
        }
        [Serializable]
        public class SaleData
        {
            [JsonProperty("saleType")]
            public string SaleType { get; set; }
            [JsonProperty("amount")]
            public long Amount { get; set; }
            [JsonProperty("discountAmount")]
            public long DiscountAmount { get; set; }
            [JsonProperty("totalAmount")]
            public long TotalAmount { get; set; }
            [JsonProperty("payments")]
            public List<PaymentData> Payments { get; set; } = new List<PaymentData>();
            [JsonProperty("products")]
            public List<ProductData> Products { get; set; } = new List<ProductData>();
            [JsonProperty("shouldPrintReceipt")]
            public bool ShouldPrintReceipt { get; set; }
            [JsonProperty("printOptions")]
            public PrintOptionsData PrintOptions { get; set; }
        }
        [Serializable]
        public class Refund
        {
            public string saleId { get; set; }
            public long amount { get; set; }
            public List<PaymentRefund> payments { get; set; } = new List<PaymentRefund>();
            public List<ProductData> products { get; set; } = new List<ProductData>();
            public bool shouldPrintReceipt { get; set; }
            public PrintOptionsData printOptions { get; set; }
        }
        [Serializable]
        public class PaymentRefund
        {
            public long amount { get; set; }
            public string paymentType { get; set; }
            public string transactionNumber { get; set; }
        }
        [Serializable]
        public class PaymentData
        {
            [JsonProperty("amount")]
            public long Amount { get; set; }
            [JsonProperty("paymentType")]
            public string PaymentType { get; set; }
        }
        [Serializable]
        public class ProductData
        {
            [JsonProperty("id")]
            public string Id { get; set; }
            [JsonProperty("name")]
            public string Name { get; set; }
            [JsonProperty("barcode")]
            public string Barcode { get; set; }
            [JsonProperty("units")]
            public string Units { get; set; }
            [JsonProperty("price")]
            public long Price { get; set; }
            [JsonProperty("vat")]
            public decimal? Vat { get; set; }
            [JsonProperty("amount")]
            public decimal? Amount { get; set; }
            [JsonProperty("isDecimalUnits")]
            public bool IsDecimalUnits { get; set; }
            [JsonProperty("labels")]
            public List<string> Labels { get; set; }
            [JsonProperty("psid")]
            public string Psid { get; set; }
            [JsonProperty("discountAmount")]
            public long DiscountAmount { get; set; }
            [JsonProperty("commissionTIN")]
            public string CommissionTIN { get; set; }
            [JsonProperty("commissionPINFL")]
            public string CommissionPINFL { get; set; }
            [JsonProperty("packageCode")]
            public decimal? PackageCode { get; set; }
            [JsonProperty("unitCode")]
            public decimal? UnitCode { get; set; }
        }
        [Serializable]
        public class PrintOptionsData
        {
            [JsonProperty("header")]
            public PrintView[] Header { get; set; }
            [JsonProperty("footer")]
            public PrintView[] Footer { get; set; }
        }
        [Serializable]
        public class PrintView
        {
            [JsonProperty("type")]
            public string Type { get; set; }
            [JsonProperty("content")]
            public string Content { get; set; }
            [JsonProperty("fontSize")]
            public int FontSize { get; set; }
        }
        [Serializable]
        public class Orders
        {
            public List<OrderSale> OrderSales { get; set; }
            public OrderReceipt OrderReceipt { get; set; } = new();
        }
        [Serializable]
        public class OrderSale
        {
            public string Name { get; set; } = string.Empty;
            public string ProductId { get; set; } = string.Empty;
            public string Code { get; set; } = string.Empty;
            public decimal Price { get; set; }
            public double Amount { get; set; }
            public double Vat { get; set; }
            public decimal DiscountSum { get; set; }
            public decimal IncreaseSum { get; set; }
            public double DiscountPercent { get; set; }
            public double Increase { get; set; }
            public decimal Sum { get; set; }
            public double TaxPercent { get; set; }
            public decimal Cost { get; set; }
            public string BarCode { get; set; } = string.Empty;
            public string ProductUnit { get; set; }
            public string Psu { get; set; } = string.Empty;
        }
        [Serializable]
        public class OrderReceipt
        {
            public decimal Sum { get; set; }
            public decimal DiscountSum { get; set; }
            public decimal ResultSum { get; set; }
            public List<OrderReceiptPayment> OrderReceiptPayments { get; set; }
        }
        [Serializable]
        public class OrderReceiptPayment
        {
            public string PaymentMethodType { get; set; }
            public decimal Sum { get; set; }
        }
    }
}
