using Guester.Resources;
using MvvmHelpers;
using Newtonsoft.Json.Linq;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Syncfusion.Maui.DataSource.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using static Guester.ViewModels.HomePageViewModel;
using ArcaCashRegister;
using Newtonsoft.Json;
using Microsoft.AppCenter.Crashes;
using MongoDB.Bson.Serialization.Serializers;

namespace Guester.ViewModels
{
    public partial class PayPageViewModel : BaseViewModel/*, IQueryAttributable*/
    {
        private object selectedPayment { get; set; }


        [ObservableProperty]
        bool isCheckPrint = true, isFiscalPrint = false;

        [ObservableProperty]
        decimal sumChange;


        public object SelectedPayment
        {
            get => selectedPayment;
            set
            {
                if (value != null && selectedPayment != value)
                {

                    selectedPayment = value;

                    OnPropertyChanged(nameof(SelectedPayment));
                    PaymentChange((PaymentMethod)SelectedPayment);



                }
            }
        }



        [ObservableProperty]
        Orders order;



        [ObservableProperty]
        ObservableRangeCollection<PaymentMethod> paymentMethods = new();

        private Realm realm;

        [ObservableProperty]
        bool isPaymentVisible;


        [ObservableProperty]
        HomePageViewModel homepage;


        public PayPageViewModel()
        {
            try
            {
                realm ??= GetRealm();
                //PaymentMethods = realm.All<PaymentMethod>().Where(x => x.IsDeleted == false);


                PaymentMethods.AddRange(realm.All<PaymentMethod>().Where(x => x.IsDeleted == false).ToList());

                Homepage = HomePageViewModel.getInstance();
            }
            catch (Exception ex) { Crashes.TrackError(ex); }
        }




        public async void PaymentChange(PaymentMethod p = null)
        {
            try
            {
                if (p == null)
                {
                    PaymentMethods.ForEach(x => x.PaymentSum = 0m);
                    p = PaymentMethods.FirstOrDefault();
                    await Task.Delay(100);
                    SelectedPayment = p;

                }
              
                    var sumPayment = PaymentMethods.Where(x=>x.Id!=p.Id).Sum(x => x.PaymentSum );
                    var sum = Order.OrderReceipt.ResultSum - sumPayment;
                var paymentCount = PaymentMethods.Count(x => x.PaymentSum > 0);
                    if (paymentCount == 1 && Order.OrderReceipt.ResultSum == sumPayment)
                    {
                        PaymentMethods.ForEach(x => x.PaymentSum = 0m);
                        p.PaymentSum = Order.OrderReceipt.ResultSum;
                    }

                    else if(sum>0)
                    {
                        if (PaymentMethods.Any(x=>x.Id==p.Id&& x.PaymentSum>0m)&& paymentCount == 1)
                        {
                            p.PaymentSum= Order.OrderReceipt.ResultSum;
                        }
                        else {
                                (p).PaymentSum = sum > 0m ? sum : 0m;
                            }
                    }
                    ChangeSum();
                

            }
            catch (Exception ex) { Crashes.TrackError(ex); }

        }




        [RelayCommand]
        private async Task Paid()
        {
            try
            {
                if (PaymentMethods.Sum(x => x.PaymentSum) < Order?.OrderReceipt?.ResultSum)
                {
                    await DialogService.ShowToast(AppResources.EnteringSummLessError);
                    return;
                }



                var cashRegisterShift = realm.Find<Shift>(CurrentCashShiftId);

                var payments = PaymentMethods.Where(x => x.PaymentSum > 0m);

                if (SumChange > 0)
                {
                    var card=PaymentMethods.Where(x=>x.PaymentType==PaymentType.Card).Sum(x=>x.PaymentSum);
                    var cash=PaymentMethods.Where(x=>x.PaymentType==PaymentType.Cash).Sum(x=>x.PaymentSum);

                    if(card>Order.OrderReceipt.ResultSum && cash < SumChange)
                    {
                        await DialogService.ShowAlertAsync("Ошибка", "Невозможно дать сдачу по карте", "OK");
                        return;
                    }
                }


                await realm.WriteAsync(() =>
                {



                    Order.OrderReceipt.ModifyDate = DateTimeOffset.Now;



                    payments.ForEach(x =>
                    {


                        Order.OrderReceipt.OrderReceiptPayments.Add(
                              new()
                              {
                                  PaymentMethod = x,
                                  Sum = x.PaymentSum
                              });


                        var shiftPayment = cashRegisterShift.ShiftPayments.FirstOrDefault(doc => doc.PaymentMethodId == x.Id);
                        if (shiftPayment != null)
                        {
                            shiftPayment.Sum = (decimal.Parse(shiftPayment.Sum) + x.PaymentSum).ToString();
                        }
                        else
                        {
                            cashRegisterShift.ShiftPayments.Add(new ShiftPayment()
                            {
                                PaymentMethodId = x.Id,
                                Sum = x.PaymentSum.ToString()
                            });
                        }

                        switch (x.PaymentType)
                        {
                            case PaymentType.Card:
                                _ = SaveLog(AppResources.PaymentInCard);
                                break;
                            case PaymentType.Cash:
                                _ = SaveLog(AppResources.PaymentInCash);
                                break;
                            case PaymentType.Bonus:
                                _ = SaveLog(AppResources.PayedSertificateLabel);
                                break;
                            case PaymentType.Cashless:
                                _ = SaveLog(AppResources.WithoutPayment);
                                break;
                        }








                    });


                    if (Homepage.CurrentBrand.IsRound)
                    {
                        Order.OrderReceipt.ResultSum = Math.Floor(Order.OrderSales.Sum(x => x.TotalPrice));
                    }

                    if (Homepage.CurrentBrand.IsPercentageForService)
                    {
                        Order.OrderReceipt.ResultSum += (Order.OrderReceipt.ResultSum * Homepage.CurrentBrand.PercentageForService / 100);
                    }


                    Order.OrderStatus = OrderStatus.Closed;
                    Order.OrderReceipt.CloseDate = DateTimeOffset.Now;

                    Order.OrderReceipt.UserId = CurrentEmpId;
                    _ = SaveLog($"{Resources.AppResources.OrderWasClosed}");
                    realm.Add(Order);

                });



                _ = Back();

                if (IsCheckPrint)
                {

                    if (!IsFiscalPrint)
                    {




                        if (!payments.Any(x => x.PaymentType == PaymentType.Bonus))
                            PrintFiscalCheque();
                    }



                    _ = Print();


                    if (Homepage.CurrentBrand.IsPrintFiscalDublicate)
                    {
                        _ = Print();
                    }
                }
                //_ = RealmService.OnSale(Order.Id);

            }
            catch (Exception ex) { Crashes.TrackError(ex); }
        }

        [RelayCommand]
        private async Task WithoutPay()
        {
            if (Homepage.CurrentBrand.OnCloseReceipt)
            {

                var res = await DialogService.ShowAdminView();
                if (!res)
                {
                    return;
                }
            }

            try
            {
                await realm.WriteAsync(() =>
                {
                    Order.OrderStatus = OrderStatus.Closed;
                    Order.OrderReceipt.ModifyDate = DateTimeOffset.Now;

                });
                //_ = RealmService.OnSale(Order.Id);
                _ = SaveLog($"{Resources.AppResources.OrderWasClossedWithoutPayment}");

                await Back();
            }
            catch (Exception ex) { Crashes.TrackError(ex); }
        }

        [RelayCommand]
        void PrintCheckEnable(string parm)
        {
            try
            {
                if (parm.Equals(nameof(IsFiscalPrint)))
                    IsFiscalPrint = !IsFiscalPrint;

                else if (parm.Equals(nameof(IsCheckPrint)))
                    IsCheckPrint = !IsCheckPrint;
            }
            catch (Exception ex) { Crashes.TrackError(ex); }
        }

        [RelayCommand]
        async Task Back()
        {
            try
            {

                var homePage = HomePageViewModel.getInstance();
                homePage.CurrentState = StateKey.Orders;
                IsPaymentVisible = false;
                await Task.Delay(50);
                getInstanceOrderContentPage().UpdateOrders();
            }
            catch (Exception ex) { Crashes.TrackError(ex); }


        }
        private async Task SaveLog(string title)
        {
            try
            {
                await realm.WriteAsync(() =>
                {
                    Order.Logs.Add(new Logs { CreaterDate = DateTimeOffset.Now, Title = title });
                });
            }
            catch (Exception ex) { Crashes.TrackError(ex); }
        }


        [RelayCommand]
        public void KeyInput(string parm)
        {

            try
            {

                var p = (PaymentMethod)SelectedPayment;
                if (parm.Equals("back"))
                {

                    if ((Math.Truncate(p.PaymentSum)).ToString().Length <= 1)
                        p.PaymentSum = 0m;
                    var strconvert = (Math.Truncate(p.PaymentSum)).ToString().Substring(0, (Math.Truncate(p.PaymentSum)).ToString().Length - 1);
                    p.PaymentSum = strconvert == string.Empty ? 0 : Convert.ToDecimal(strconvert);
                    ChangeSum();
                    return;
                }
                if (p.PaymentSum > 90000000000)
                    return;

                if (parm.Equals("10000") || parm.Equals("20000"))
                {
                    p.PaymentSum += Convert.ToDecimal(parm);
                    ChangeSum();
                    return;
                }


                p.PaymentSum = Convert.ToDecimal((Math.Truncate(p.PaymentSum)).ToString() + parm);
                ChangeSum();
            }
            catch (Exception ex) { Crashes.TrackError(ex); return; }


        }

        private void ChangeSum()
        {
            try
            {
                var sum = PaymentMethods.Sum(x => x.PaymentSum);
                SumChange = sum - Order.OrderReceipt.ResultSum;
                if (SumChange < 0m)
                    SumChange = 0m;
            }
            catch (Exception ex) { Crashes.TrackError(ex); }
        }



        [RelayCommand]
        private async Task Print()
        {
            try
            {
                var printer = realm.All<CashRegister>().FirstOrDefault(doc => doc.IsActive
                     && doc.CashRegisterTypeRaw == (int)CashRegisterType.Printer
                     && !doc.IsDeleted);

                if (printer == null)
                    return;

                await DialogService.ShowToast($"{AppResources.SentForPrintingToThePrinter} {printer.CashRegisterSetting.IpAddress}");

                string GS = Convert.ToString((char)29);
                string ESC = Convert.ToString((char)27);

                string CUTCOMMAND = "";
                CUTCOMMAND = ESC + "@";
                CUTCOMMAND += GS + "V" + (char)48;
                var salePoint = realm.All<SalesPoint>().FirstOrDefault();

                var payments = PaymentMethods.Where(x => x.PaymentSum > 0m);

                var discount = Order.OrderDiscountTotal;
                var discountText = discount > 0m ? $"{AppResources.Discount}{new string('.', 33)} {Order.OrderDiscountTotal,-10}\r\n" : "";

                var plain_text = $"{salePoint.Name,28}\r\n\r\n\r\n{AppResources.Receipt} № {Order.Number,20}\r\n{new string('-', 48)}\r\n{AppResources.Cashier} {AuthorName,20}\r\n{new string('-', 48)}\r\n{AppResources.Printed} {DateTime.Now.ToString("dd MMMM yyyy HH:mm"),27}\r\n{new string('-', 48)}\r\n{AppResources.TheOrderIsOpen} {Order.CreationDate.LocalDateTime.ToString("dd MMMM yyyy HH:mm"),24}\r\n\r\n" +
                   $"{GeneratePlainTextTable(Order.OrderSales.Where(x => IsNotNull(x.Name)).ToList())}\r\n\r\n{AppResources.Total}{new string('.', 33)} {(discount > 0m ? Order.OrderReceipt.ResultSum + discount : Order.OrderReceipt.ResultSum),-10}\r\n{discountText}" +
                   $"{AppResources.TotalAmountToBePaid}{new string('.', 24)}{Order.OrderReceipt.ResultSum,-10}" + $"{(payments.Count() > 0 ? $"\r\n\r\n" : "")}"
                   +
                   $"{GeneratePaymentsText(payments: payments.ToList())}"
                   +
                   $"\r\n\r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n  {CUTCOMMAND} ";

                if (printer != null)
                {
                    PrintSevice.PrintText(plain_text, printer);
                }
            }
            catch (Exception ex) { Crashes.TrackError(ex); }
            // await DependencyService.Get<IPrintService>().PrintHtml(plain_text, "PrintJobName");




        }
        static string GeneratePaymentsText(List<PaymentMethod> payments)
        {
            try
            {
                if (payments is null) return "";
                if (payments.Count < 1) return "";

                StringBuilder paymentText = new();
                paymentText.AppendLine(new string('-', 48));
                payments.ForEach(x =>
                {
                    var name = x.Name;
                    if (x.Name.Length > 20)
                    {
                        name = name.Substring(0, 19);
                        name += ".";
                    }

                    var size = 48 - (name.Length + 10);

                    paymentText.AppendLine($"{name}{new string('.', size)}{(int)x.PaymentSum,-10}");

                    paymentText.AppendLine();
                });

                paymentText.AppendLine(new string('-', 48));

                return paymentText.ToString();
            }
            catch (Exception ex) { Crashes.TrackError(ex); return ""; }
        }

        static string GeneratePlainTextTable(List<OrderSale> data)
        {
            try
            {
                StringBuilder plainTextTable = new StringBuilder();

                plainTextTable.AppendLine(new string('-', 48));

                plainTextTable.AppendLine(string.Format("{0,-14} {1,-14} {2,-9} {3,-13}",
                                                         AppResources.Name,
                                                         AppResources.Quantity.PadLeft(7 + AppResources.Quantity.Length / 2).PadRight(14),
                                                         AppResources.Price,
                                                         AppResources.Total));
                plainTextTable.AppendLine(new string('-', 48));


                foreach (var item in data)
                {

                    string[] nameLines = WrapText(item.Name, 14).ToArray();
                    string[] quantityLines = WrapText(item.Amount.ToString(), 14).ToArray();
                    string[] priceLines = WrapText(item.Price.ToString(), 9).ToArray();
                    string[] totalPriceLines = WrapText(item.TotalPrice.ToString(), 13).ToArray();

                    int maxLines = Math.Max(nameLines.Length, Math.Max(quantityLines.Length, Math.Max(priceLines.Length, totalPriceLines.Length)));

                    for (int i = 0; i < maxLines; i++)
                    {
                        string name = i < nameLines.Length ? nameLines[i] : string.Empty;
                        string quantity = i < quantityLines.Length ? quantityLines[i].PadLeft(4 + AppResources.Quantity.Length / 2).PadRight(14) : string.Empty;
                        string price = i < priceLines.Length ? priceLines[i] : string.Empty;
                        string totalPrice = i < totalPriceLines.Length ? totalPriceLines[i] : string.Empty;

                        plainTextTable.AppendLine($"{name,-14} {quantity,-14} {price,-9} {totalPrice,-13}");
                    }
                }

                plainTextTable.AppendLine(new string('-', 48));

                return plainTextTable.ToString();
            }
            catch
            {
                return "";
            }
        }


        static IEnumerable<string> WrapText(string text, int maxLength)
        {
            for (int i = 0; i < text.Length; i += maxLength)
            {
                yield return text.Substring(i, Math.Min(maxLength, text.Length - i));
            }
        }
        private async void PrintFiscalCheque()
        {
            try
            {
                var cashRegister = realm.All<CashRegister>().FirstOrDefault(doc => doc.IsActive
                    && doc.CashRegisterTypeRaw == (int)CashRegisterType.CashRegister);

                if (cashRegister == null)
                    return;

                CreateWebsocket CreateWebsocket = new CreateWebsocket(
                        new CreateWebsocket.Settings
                        {
                            Host = cashRegister.CashRegisterSetting.IpAddress,
                            Login = cashRegister.CashRegisterSetting.Login,
                            Password = cashRegister.CashRegisterSetting.Password,
                            Port = "8888",
                            Sub = "",
                            Protocol = "ws://"
                        });
                var response = JsonConvert.DeserializeObject<ReceiptResponce>(CreateWebsocket.Send(Order, 0));

                if (!string.IsNullOrEmpty(response.Error))
                    CheckResponceStatus(response);
                else
                    Order.SaleId = (string)response.Payload.saleId;
            }
            catch (Exception ex) { Crashes.TrackError(ex); }
        }
    }
}
