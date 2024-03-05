
using Guester.Resources;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.ListView;
using System.Net.Sockets;
using static Guester.ViewModels.HomePageViewModel;
using ArcaCashRegister;
using Newtonsoft.Json;
using MvvmHelpers;
using Microsoft.AppCenter.Crashes;

namespace Guester.ViewModels;

public partial class ChecksContentViewModel : BaseViewModel
{
    [ObservableProperty]
    IQueryable<Orders> orders;

    private IDisposable subscribeOrders;

    //IQueryable checks;
    private Realm realm;

    [ObservableProperty]
    string timePeriodTitle = AppResources.Today;
    [ObservableProperty]
    bool isAllCheck, isByCash, isByCard, isRefund, isTimePeriud;

    [ObservableProperty]
    private HomePageViewModel homePageViewModel;


    //[ObservableProperty]
    //CriteriaOperator ordersFilter;

    [ObservableProperty]
    bool isAddNewOrder, isTableSelect, isGuestSelect;
    [ObservableProperty]
    Orders selectedOrder = new();


    [ObservableProperty]
    DateTimeOffset startDate = DateTimeOffset.MinValue, endDate = DateTimeOffset.MinValue;
    // private bool firstLoad = true;
    public SfListView ListView { get; set; }

    [ObservableProperty]
    IQueryable<Detail> orderDetails;

    [ObservableProperty]
    ObservableRangeCollection<OrderSale> orderSales;
    private CheckFilter filter
    {
        get
        {
            return this switch
            {
                { IsAllCheck: true } => CheckFilter.Close,
                { IsByCash: true } => CheckFilter.Cash,
                { IsByCard: true } => CheckFilter.Card,
                { IsRefund: true } => CheckFilter.Refund,
                _ => CheckFilter.Close
            };

        }
    }

    public ChecksContentViewModel()
    {
        homePageViewModel ??= HomePageViewModel.getInstance();
        realm ??= GetRealm();
        OrderDetails = realm.All<Detail>();
        Orders = realm.All<Orders>().ToList().Where(x => x.OrderStatus == OrderStatus.Closed).AsQueryable();
        closing_popups();


        loadSavingFilter();

        subscribeOrders = realm.All<Orders>()
                    .SubscribeForNotifications((sender, changes) =>
                    {
                        if (changes == null)
                            return;
                        foreach (var i in changes.NewModifiedIndices)
                        {
                            Orders = realm.All<Orders>().ToList().Where(x => x.OrderStatus == OrderStatus.Closed).AsQueryable();

                            MakeFilter();


                            return;
                        }


                        foreach (var i in changes.DeletedIndices)
                        {
                            Orders = realm.All<Orders>().ToList().Where(x => x.OrderStatus == OrderStatus.Closed).AsQueryable();


                            MakeFilter();


                        }
                    });
    }


    private Orders createOrder;


    [RelayCommand]
    private void SeletTable(Table currentTable)
    {
        if (!IsNotNull(createOrder))
            return;
        IsTableSelect = false;
        IsGuestSelect = true;
        realm.Write(() =>
        {
            createOrder.OrderReceipt.Table =

     IsNotNull(currentTable) ? new()
     {
         Id = currentTable.Id,
         Name = currentTable.Name,
         Height = currentTable.Height,
         PosX = currentTable.PosX,
         Picture = currentTable.Picture,
         PosY = currentTable.PosY,
         IsDeleted = currentTable.IsDeleted,
         Color = currentTable.Color,
         Zindex = currentTable.Zindex,
         BorderRadius = currentTable.BorderRadius,
         Seats = currentTable.Seats,
         Width = currentTable.Width,

     } : null;
        });


    }
    [RelayCommand]
    private async void SeletGuestCount(string guest_count)
    {
        int.TryParse(guest_count, out int num);
        realm.Write(() =>
        {
            if (num > 0)
            {

                createOrder.GuestCount = num + 1;

            }
        });
        await SaveOrder(createOrder);

        // closing_popups();

    }

    [RelayCommand]
    private async Task ChangeOrderStatus(Orders order)
    {
        if (!IsNotNull(order, order.OrderDelivery)) return;

        if (!IsNotNull(order.OrderDelivery.Courier) && order.OrderDelivery.DeliveryStatus == DeliveryStatus.Success) return;

        await realm.WriteAsync(() =>
        {

            switch (order.OrderDelivery.DeliveryStatus)
            {
                case DeliveryStatus.New:
                    order.OrderStatus = OrderStatus.Delivery;
                    order.OrderDelivery.DeliveryStatus = DeliveryStatus.Success;
                    //  order.OrderDelivery.DeliveryOrderState.Add(new() { Duration = 0, OrderStateEnum = DeliveryStatus.Success });
                    break;
                case DeliveryStatus.Success:

                    order.OrderDelivery.DeliveryStatus = DeliveryStatus.OnWay;
                    //  order.OrderDelivery.DeliveryOrderState.Add(new() { Duration = 0, OrderStateEnum = DeliveryStatus.OnWay });
                    break;
                case DeliveryStatus.OnWay:

                    order.OrderDelivery.DeliveryStatus = DeliveryStatus.Delivered;
                    //  order.OrderDelivery.DeliveryOrderState.Add(new() { Duration = 0, OrderStateEnum = DeliveryStatus.Delivered });
                    break;
                case DeliveryStatus.Delivered:
                    order.OrderStatus = OrderStatus.Closed;
                    order.OrderDelivery.DeliveryStatus = DeliveryStatus.Closed;
                    //_ = RealmService.OnSale(order.Id);
                    //  order.OrderDelivery.DeliveryOrderState.Add(new() { Duration = 0, OrderStateEnum = DeliveryStatus.Closed });
                    break;

            }
        });

    }


    [RelayCommand]
    private async Task CreateOrder(Detail orderDetail)
    {
        try
        {
            if (orderDetail == null)
            {
                IsAddNewOrder = !IsAddNewOrder;
                IsTableSelect = false;
                IsGuestSelect = false;
                createOrder = null;
                return;
            }
            var order_type = orderDetail.OrderType;

            var deliveryZone = realm.All<DeliveryZone>().FirstOrDefault(x => x.salesPointId == CurrentSalePointId);
            await realm.WriteAsync(() =>
             {
                 var order_delivery = order_type is OrderType.Delivery ?
            new OrderDelivery()
            {

                CashierId = CurrentEmpId,
                DeliveryStatus = DeliveryStatus.New,
                DeliveryZone = deliveryZone,

            } : null;
                 createOrder = new Orders
                 {

                     Detail = orderDetail,
                     ShiftId = CurrentCashShiftId,
                     BrandId = CurrentBrandId,
                     OrderStatus = OrderStatus.New,
                     OrderReceipt = new()
                     {

                         CashierId = CurrentEmpId,
                         Table = null,

                     },
                     Name = order_type switch
                     {
                         OrderType.InTheInstitution => AppResources.InEnvy,
                         OrderType.TakeAway => Resources.AppResources.TakeAway,
                         OrderType.Delivery => AppResources.DeliveryLabel,
                         _ => Resources.AppResources.Order
                     },
                     SalePointId = CurrentSalePointId,
                     OrderDelivery = order_delivery

                 };
             });
            if (order_type == OrderType.InTheInstitution)
            {
                IsTableSelect = true;
                return;
            }





            await SaveOrder(createOrder);
        }
        catch (Exception ex) { Crashes.TrackError(ex); }




    }
    private async Task SaveOrder(Orders order)
    {
        try
        {
            if (order is null)
            {
                closing_popups();
                return;
            }
            await realm.WriteAsync(() =>
            {

                order.Logs.Add(new Logs { CreaterDate = DateTimeOffset.Now, Title = AppResources.WasCreateNewOrder, DeviceName = CurrentDivaceId });
                for (int i = 1; i < order.GuestCount; i++)
                {
                    order.OrderSales.Add(new OrderSale { GuestIndex = 1 + i });

                    order.Logs.Add(new Logs { CreaterDate = DateTimeOffset.Now, Title = $"{AppResources.GuestAdded} {i + 1}", DeviceName = CurrentDivaceId });
                }


                realm.Add(order);

            });


            var homePage = HomePageViewModel.getInstance();
            homePage.CurrentState = StateKey.OrderDetail;
            var orderPage = HomePageViewModel.getInstanceOrderDetailPage();

            closing_popups();


            orderPage.Order = order;

            orderPage.OnAppearing();
            IsAddNewOrder = false;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private void closing_popups()
    {
        IsAddNewOrder = false;

        IsTableSelect = false;
        IsGuestSelect = false;
    }



    internal void OnAppearing()
    {
        MakeFilter();
        IsTimePeriud = false;
    }


    [RelayCommand]
    async void SelectOrder(Orders order)
    {
        try
        {

            if (!IsNotNull(order))
                return;

            SelectedOrder = order;

            await Task.Delay(200);
            OnPropertyChanged(nameof(SelectedOrder));
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    // To do correct
    public void AddOrderSale(OrderSale orderSale)
    {
        OrderSales.Add(orderSale);
    }

    [RelayCommand]
    async Task RefundOrder()
    {

        try
        {
            if (!IsNotNull(SelectedOrder))
                return;


            var confirm = await DialogService.ShowWarningAsync("Внимание!", "Нужно ли списать количество со склада? ", true);


            await realm.WriteAsync(() =>
            {
                SelectedOrder.OrderStatus = OrderStatus.Closed;

                SelectedOrder.OrderReceipt.IsRefund = true;
                SelectedOrder.OrderReceipt.IsWriteOff = !confirm;

                SelectedOrder.OrderReceipt.UserId = CurrentEmpId;



                _ = SaveLog($"{AppResources.TheOrderWasReturned} {SelectedOrder.Name}");
                realm.Add(SelectedOrder);



            });

            await DialogService.ShowToast($"{AppResources.TheOrderWasReturned}");
        }
        catch (Exception ex) { Crashes.TrackError(ex); }




    }

    private async Task SaveLog(string title)
    {
        try
        {
            await realm.WriteAsync(() =>
            {
                SelectedOrder.Logs.Add(new Logs { CreaterDate = DateTimeOffset.Now, Title = title, DeviceName = CurrentDivaceId });
            });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    public void FilterChanges(string parm)
    {
        try
        {
            IsAllCheck = parm == nameof(CheckFilter.Close);
            IsByCash = parm == nameof(CheckFilter.Cash);
            IsByCard = parm == nameof(CheckFilter.Card);
            IsRefund = parm == nameof(CheckFilter.Refund);
            Preferences.Set(nameof(IsAllCheck), IsAllCheck);
            Preferences.Set(nameof(IsByCash), IsByCash);
            Preferences.Set(nameof(IsByCard), IsByCard);
            Preferences.Set(nameof(IsRefund), IsRefund);
            MakeFilter();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    private bool FilterContacts(object obj)
    {
        try
        {
            var item = obj as Orders;

            var filterByPeriod = StartDate != DateTimeOffset.MinValue && EndDate != DateTimeOffset.MaxValue;

            var start_date = DateTimeOffset.Now;
            var end_date = DateTimeOffset.Now;
            if (filterByPeriod)
            {
                start_date = new DateTimeOffset(StartDate.Year, StartDate.Month, StartDate.Day, 0, 0, 0, StartDate.Offset);
                end_date = new DateTimeOffset(EndDate.Year, EndDate.Month, EndDate.Day, 23, 59, 59, EndDate.Offset);
            }
            else
            {
                var ts = new TimeSpan(0, 0, 0);
                DateTimeOffset now = DateTimeOffset.Now;
                start_date = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, ts);
                end_date = new DateTimeOffset(now.Year, now.Month, now.Day, 23, 59, 59, ts);
            }
            if (IsAllCheck && item.OrderStatus is OrderStatus.Closed
                    && item.ModifiedDate >= start_date
                    && item.ModifiedDate <= end_date)
                return true;
            else if (IsByCash && item.OrderReceipt.OrderReceiptPayments.Any(y => y.PaymentMethod.PaymentType == PaymentType.Cash)
                    && item.ModifiedDate >= start_date
                    && item.ModifiedDate <= end_date)
                return true;
            else if (IsByCard && item.OrderReceipt.OrderReceiptPayments.Any(y => y.PaymentMethod.PaymentType == PaymentType.Card)
                    && item.ModifiedDate >= start_date
                    && item.ModifiedDate <= end_date)
                return true;
            else if (IsRefund && item.OrderReceipt.IsRefund
                    && item.ModifiedDate >= start_date
                    && item.ModifiedDate <= end_date)
                return true;
            else
                return false;


        }
        catch (Exception ex) { Crashes.TrackError(ex); return false; }

    }

    public void MakeFilter()
    {
        try
        {

            if (IsNotNull(ListView))
            {
                if (ListView.DataSource != null)
                {
                    ListView.DataSource.Filter = FilterContacts;
                    ListView.DataSource.RefreshFilter();
                }
            }
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private void getArchiveChecks()
    {
        try
        {

            if (realm is null)
                realm = GetRealm();
            #region realm filtring
            /*   var _sort = "";
               var filterByPeriod= StartDate!=DateTimeOffset.MinValue && EndDate!=DateTimeOffset.MaxValue;
               var _filter = filter switch
               {
                   *//*	CheckFilter.Close => $"check_state=='{nameof(CheckFilter.Close)}' AND check_type =='{nameof(CheckType.Sale)}'",
                       CheckFilter.Card=> $"check_type =='{nameof(CheckType.Sale)}' AND check_state=='{nameof(CheckFilter.Close)}' AND ANY payments.stateRaw == '{nameof(PaymentState.Card)}'",
                       CheckFilter.Cash=> $"check_type =='{nameof(CheckType.Sale)}' AND check_state=='{nameof(CheckFilter.Close)}' AND ANY payments.stateRaw == '{nameof(PaymentState.Cash)}'",
                       CheckFilter.Refund => $"check_type =='{nameof(CheckType.Refund)}'",
                       _ => $"check_state=='{nameof(CheckFilter.Close)}' AND check_type =='{nameof(CheckType.Sale)}'"*//*
                       CheckFilter.Close => $"order_state=='{nameof(OrderState.Closed)}' OR order_state=='{nameof(OrderState.Returned)}'",
                       CheckFilter.Card=> $"order_state =='{nameof(OrderState.Closed)}' AND order_pay_state=='{nameof(OrderPayState.Card)}'",
                       CheckFilter.Cash=> $"order_state =='{nameof(OrderState.Closed)}' AND order_pay_state=='{nameof(OrderPayState.Cash)}'",
                       CheckFilter.Refund => $"order_state =='{nameof(OrderState.Returned)}'",
                       _ => $"order_state=='{nameof(OrderState.Closed)}'"
               };


           //	Checks = realm.All<Check>().Filter($"{_filter} {_sort}");


               if (filterByPeriod)
               {
                   var start_date = new DateTimeOffset(StartDate.Year, StartDate.Month, StartDate.Day, 0, 0, 0, StartDate.Offset);
                   var end_date = new DateTimeOffset(EndDate.Year, EndDate.Month, EndDate.Day, 23, 59, 59, EndDate.Offset);
                   // _filter += $"close_date >={start_date} AND close_date<={end_date}";
                   // _filter += $"AND created_date >={start_date} AND end_date<={end_date}";
                   Orders = realm.All<Order>().Filter($"{_filter} {_sort}").Where(x => x.CreateDate >= start_date && x.EndDate <= end_date);
               }
               else
               {
                   var ts = new TimeSpan(0, 0, 0);
                   DateTimeOffset now = DateTimeOffset.Now;
                   var start_date = new DateTimeOffset(now.Year, now.Month, now.Day, 0, 0, 0, ts);
                   var end_date = new DateTimeOffset(now.Year, now.Month, now.Day, 23, 59, 59, ts);
                   // _filter += $" AND created_date >= {start_date} AND end_date <= {end_date} ";

                   Orders = realm.All<Order>().Filter($"{_filter} {_sort}").Where(x=>x.CreateDate>=start_date&&x.EndDate<=end_date);
               }
    */
            #endregion
            Orders = realm.All<Orders>();
            MakeFilter();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    public async Task PartialRefund()
    {
        try
        {
            if (selectedOrder.OrderReceipt.OrderReceiptPayments.Count > 1)
            {
                // To do add warning
                return;
            }

            var printer = realm.All<CashRegister>().FirstOrDefault(doc => doc.IsActive
                && doc.CashRegisterTypeRaw == (int)CashRegisterType.Printer
                && !doc.IsDeleted);


            DialogService.ShowToast($"Отправленно на печать на принтер {printer.CashRegisterSetting.IpAddress}");

            string GS = Convert.ToString((char)29);
            string ESC = Convert.ToString((char)27);

            string CUTCOMMAND = "";
            CUTCOMMAND = ESC + "@";
            CUTCOMMAND += GS + "V" + (char)48;
            var salePoint = realm.All<SalesPoint>().FirstOrDefault();

            var payments = selectedOrder.OrderReceipt.OrderReceiptPayments.Where(x => x.Sum > 0m);

            var discount = selectedOrder.OrderDiscountTotal;
            var discountText = discount > 0m ? $"Скидка{new string('.', 33)} {selectedOrder.OrderDiscountTotal,-10}\r\n" : "";

            var plain_text = $"{salePoint.Name,28}\r\n\r\n\r\nЧек № {selectedOrder.Number,20}\r\n{new string('-', 48)}\r\nКассир {AuthorName,20}\r\n{new string('-', 48)}\r\nНапечатан {DateTime.Now.ToString("dd MMMM yyyy HH:mm"),27}\r\n{new string('-', 48)}\r\nЗаказ Открыт {selectedOrder.CreationDate.LocalDateTime.ToString("dd MMMM yyyy HH:mm"),24}\r\nВозвратный чек\r\n\r\n" +
               $"{GeneratePlainTextTable(orderSales.Where(x => IsNotNull(x.Name)).ToList())}\r\n\r\nИтого{new string('.', 33)} {(discount > 0m ? selectedOrder.OrderReceipt.ResultSum + discount : selectedOrder.OrderReceipt.ResultSum),-10}\r\n{discountText}" +
               $"Итого к оплате{new string('.', 24)}{selectedOrder.OrderReceipt.ResultSum,-10}" + $"{(payments.Count() > 0 ? $"\r\n\r\n" : "")}"
               +
               $"{GeneratePaymentsText(payments: payments.ToList())}"
               +
               $"\r\n\r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n  {CUTCOMMAND} ";

            if (printer != null)
            {
                PrintSevice.PrintText(plain_text, printer);
            }

            if (!IsNotNull(SelectedOrder))
                return;



            await realm.WriteAsync(() =>
            {
                SelectedOrder.OrderStatus = OrderStatus.Closed;

                SelectedOrder.OrderReceipt.IsRefund = true;

                SelectedOrder.OrderReceipt.UserId = CurrentEmpId;

                _ = SaveLog($"Частичный возврат заказа: {SelectedOrder.Name}");

                foreach (var orderSale in orderSales)
                {
                    _ = SaveLog($"Возврат товара: {orderSale.Name}");
                }
                realm.Add(SelectedOrder);
            });

            await DialogService.ShowToast("Вернули заказ");
        }
        catch (Exception ex)
        {
            var err = ex.Message;
        }
    }

    public async Task FiscalRefund(Orders refundOrder) // To do correct
    {
        try
        {
            var cashRegister = realm.All<CashRegister>().FirstOrDefault(doc => doc.IsActive
                && doc.CashRegisterTypeRaw == (int)CashRegisterType.CashRegister);

            CreateWebsocket CreateWebsocket = new CreateWebsocket(
                    new CreateWebsocket.Settings
                    {
                        Host = cashRegister.CashRegisterSetting.IpAddress,
                        Login = cashRegister.CashRegisterSetting.Login,
                        Password = cashRegister.CashRegisterSetting.Password,
                        Port = "",
                        Sub = "websocket",
                        Protocol = "ws://"
                    });
            var response = JsonConvert.DeserializeObject<dynamic>(CreateWebsocket.Send(refundOrder, 0));

            if (!string.IsNullOrEmpty(response.Error))
                CheckResponceStatus(response);
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    public async Task ChangeTimePeriod(string parm)
    {
        try
        {

            IsTimePeriud = !IsTimePeriud;
            await Task.Delay(200);

            if (TimePeriodTitle == parm)
                return;

            if (!IsTimePeriud)
            {

                switch (parm)
                {
                    case "Сегодня":
                        EndDate = DateTimeOffset.MinValue;
                        StartDate = DateTimeOffset.MinValue;
                        TimePeriodTitle = AppResources.Today;
                        break;
                    case "Неделя":
                        EndDate = DateTimeOffset.Now;
                        StartDate = DateTimeOffset.Now.AddDays(-7);
                        TimePeriodTitle = AppResources.Week;

                        break;
                    case "Месяц":
                        EndDate = DateTimeOffset.Now;
                        StartDate = DateTimeOffset.Now.AddDays(-30);
                        TimePeriodTitle = AppResources.Month;
                        break;
                    default:
                        EndDate = DateTimeOffset.MinValue;
                        StartDate = DateTimeOffset.MinValue;
                        TimePeriodTitle = AppResources.Today;
                        break;

                }

            }


            MakeFilter();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    private void loadSavingFilter()
    {
        IsAllCheck = Preferences.Get(nameof(IsAllCheck), false);
        IsByCash = Preferences.Get(nameof(IsByCash), false);
        IsByCard = Preferences.Get(nameof(IsByCard), false);
        IsRefund = Preferences.Get(nameof(IsRefund), false);
        // getArchiveChecks();

        IsAllCheck = !IsByCash && !IsByCard && !IsRefund;

        MakeFilter();
    }

    [RelayCommand]
    public async void ViewCheck()
    {
        try
        {
            var printer = realm.All<CashRegister>().FirstOrDefault(doc => doc.IsActive
            && doc.CashRegisterTypeRaw == (int)CashRegisterType.Printer
            && !doc.IsDeleted);

            await DialogService.ShowToast($"{AppResources.SentForPrintingToThePrinter} {printer.CashRegisterSetting.IpAddress}");

            string GS = Convert.ToString((char)29);
            string ESC = Convert.ToString((char)27);

            string CUTCOMMAND = "";
            CUTCOMMAND = ESC + "@";
            CUTCOMMAND += GS + "V" + (char)48;

            var payments = SelectedOrder.OrderReceipt?.OrderReceiptPayments?.Where(x => x.PaymentMethod?.PaymentSum > 0m).Select(x => x.PaymentMethod);
            var salePoint = realm.All<SalesPoint>().FirstOrDefault();

            var discount = SelectedOrder.OrderDiscountTotal;
            var discountText = discount > 0m ? $"{AppResources.Discount}{new string('.', 33)} {SelectedOrder.OrderDiscountTotal,-10}\r\n" : "";
            var plain_text = $"{salePoint.Name,28}\r\n\r\n\r\n{AppResources.Receipt} № {SelectedOrder.Number,20}\r\n{new string('-', 48)}\r\n{AppResources.Cashier} {AuthorName,20}\r\n{new string('-', 48)}\r\n{AppResources.Printed} {DateTime.Now.ToString("dd MMMM yyyy HH:mm"),27}\r\n{new string('-', 48)}\r\n{AppResources.TheOrderIsOpen} {SelectedOrder.CreationDate.LocalDateTime.ToString("dd MMMM yyyy HH:mm"),24}\r\n\r\n" +
                              $"{GeneratePlainTextTable(SelectedOrder.OrderSales.Where(x => IsNotNull(x.Name)).ToList())}\r\n\r\n{AppResources.Total}{new string('.', 33)} {(discount > 0m ? SelectedOrder.OrderReceipt.ResultSum + discount : SelectedOrder.OrderReceipt.ResultSum),-10}\r\n{discountText}" +
                              $"{AppResources.TotalAmountToBePaid}{new string('.', 24)}{SelectedOrder.OrderReceipt.ResultSum,-10}" + $"{(payments.Count() > 0 ? $"\r\n\r\n" : "")}"
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
    static string GeneratePaymentsText(List<OrderReceiptPayment> payments)
    {
        try
        {
            if (payments is null) return "";
            if (payments.Count < 1) return "";

            StringBuilder paymentText = new();
            paymentText.AppendLine(new string('-', 48));
            payments.ForEach(x =>
            {
                var name = x.PaymentMethod.Name;
                if (x.PaymentMethod.Name.Length > 20)
                {
                    name = name.Substring(0, 19);
                    name += ".";
                }

                var size = 48 - (name.Length + 10);

                paymentText.AppendLine($"{name}{new string('.', size)}{(int)x.Sum,-10}");

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


            plainTextTable.AppendLine($"{$"{AppResources.Name}",-18} {$"{AppResources.Quantity}",-8} {$"{AppResources.Price}",-9} {$"{AppResources.Total}",-13}");


            // plainTextTable.AppendLine(new string('-', 48));


            foreach (var item in data)
            {
                plainTextTable.AppendLine($"{item.Name,-18} {item.Amount,-8} {item.Price,-9} {item.TotalPrice,-13}");
            }


            plainTextTable.AppendLine(new string('-', 48));

            return plainTextTable.ToString();
        }
        catch (Exception ex) { Crashes.TrackError(ex); return ""; }
    }

    internal void onDissapearing()
    {
        subscribeOrders?.Dispose();
    }
}



