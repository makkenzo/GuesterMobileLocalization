
using Syncfusion.Maui.DataSource.Extensions;
//using DevExpress.Data.Filtering;
using static Guester.ViewModels.HomePageViewModel;
using System.Drawing.Text;
using Guester.Resources;
using Syncfusion.Maui.ListView;
using System.Collections.ObjectModel;
using MvvmHelpers;
using Microsoft.AppCenter.Crashes;

namespace Guester.ViewModels;



public partial class OrdersContentViewModel : BaseViewModel
{
    //[ObservableProperty]
    //IQueryable<Orders> orders;

    [ObservableProperty]
    private string ordersType = AppResources.AllOrders;

    [ObservableProperty]
    IQueryable<Orders> orderss;

    [ObservableProperty]
    ObservableRangeCollection<GuestCreate> createGuests = new();

    public SfListView ListView { get; set; }

    [ObservableProperty]
    bool isAllOrders, isNewOrders, isOnlineOrders, isSuccessOrders, isInWayOrders, isDelivered, isInRestoran, isOutRestoran, isInDelivered, isOrderType, isTableSelect, isGuestSelect;


    [ObservableProperty]
    int deliveryCount, allCount, inWayCount, alreadyCount, onlineCount, newCount;

    //private IQueryable<Orders> allOrders;

    //private List<Orders> all_Orders = new();
    [ObservableProperty]
    bool isAddNewOrder;

    [ObservableProperty]
    private HomePageViewModel homePageViewModel;




    [ObservableProperty]
    ObservableRangeCollection<Detail> orderDetails = new();


    public ObservableRangeCollection<Employer> Couriers { get; set; } = new();


    //[ObservableProperty]
    //CriteriaOperator ordersFilter;
    private OrderFilter filter
    {
        get
        {
            if (IsNewOrders)
                return OrderFilter.New;
            else if (IsOnlineOrders)
                return OrderFilter.Online;
            else if (IsSuccessOrders)
                return OrderFilter.Success;
            else if (IsInWayOrders)
                return OrderFilter.InWay;
            else if (IsDelivered)
                return OrderFilter.Delivered;
            return OrderFilter.All;
        }
    }

    private Realm realm;
    private IDisposable subscribeOrders;


    //private int count { get; set; }
    public OrdersContentViewModel()
    {

        try
        {
            realm ??= GetRealm();


            Orderss = realm.All<Orders>().OrderByDescending(x => x.CreationDate);
            for (int i = 1; i < 5; i++)
            {

                CreateGuests.Add(new() { Name = i.ToString() });
            }
            var courers = realm.All<Employer>().ToList().Where(x => x.Post?.IsCourier == true).ToList();
            Couriers.ReplaceRange(courers);
            //allOrders = realm.All<Orders>();
            //count = allOrders.Count();
            //Orders =allOrders;
            homePageViewModel ??= HomePageViewModel.getInstance();
            closing_popups();
            //subscribeOrders = Orderss
            //      .SubscribeForNotifications((sender, changes) =>
            //      {
            //          if (changes == null)
            //              return;
            //          homePageViewModel ??= HomePageViewModel.getInstance();
            //          foreach (var i in changes.NewModifiedIndices)
            //          {

            //              getOrdersCount();

            //              return;
            //          }

            //          foreach (var i in changes.InsertedIndices)
            //          {


            //              getOrdersCount();


            //              return;
            //          }
            //          foreach (var i in changes.DeletedIndices)
            //          {

            //              getOrdersCount();



            //          }
            //      });
            loadSavingFilter();
            var brand = realm.Find<Brand>(CurrentBrandId);
            OrderDetails.ReplaceRange(realm.All<Detail>().ToList().Where(x => !x.IsDeleted
                && x.Name != string.Empty
                && x.OnTerminal
                && (!brand.IsDelivery ? x.OrderType != OrderType.Delivery : true)
                && x.SalesPointsId.ToList().Any(x => x.Id == CurrentSalePointId)));


            //   all_Orders=(allOrders.ToList().Where(x => x.OrderStatus == OrderStatus.New || x.OrderStatus == OrderStatus.Delivery || x.IsOnline || x.OrderDelivery?.DeliveryStatus == DeliveryStatus.OnWay
            //|| x.OrderStatus == OrderStatus.Success || x.OrderDelivery?.DeliveryStatus == DeliveryStatus.Delivered && x.OrderStatus == OrderStatus.Closed)).ToList();

            realm.All<Detail>().SubscribeForNotifications((sender, changes) =>
                  {
                      if (changes == null)
                          return;

                      foreach (var i in changes.NewModifiedIndices)
                      {
                          OrderDetails.ReplaceRange(realm.All<Detail>().ToList().Where(x => !x.IsDeleted
                            && x.Name != string.Empty
                            && x.OnTerminal
                            && (!brand.IsDelivery ? x.OrderType != OrderType.Delivery : true)
                            && x.SalesPointsId.ToList().Any(x => x.Id == CurrentSalePointId)));


                          return;
                      }

                      foreach (var i in changes.InsertedIndices)
                      {


                          OrderDetails.ReplaceRange(realm.All<Detail>().ToList().Where(x => !x.IsDeleted
                            && x.Name != string.Empty
                            && x.OnTerminal
                            && (!brand.IsDelivery ? x.OrderType != OrderType.Delivery : true)
                            && x.SalesPointsId.ToList().Any(x => x.Id == CurrentSalePointId)));


                          return;


                      }
                      foreach (var i in changes.DeletedIndices)
                      {
                          OrderDetails.ReplaceRange(realm.All<Detail>().ToList().Where(x => !x.IsDeleted
                            && x.Name != string.Empty
                            && x.OnTerminal
                            && (!brand.IsDelivery ? x.OrderType != OrderType.Delivery : true)
                            && x.SalesPointsId.ToList().Any(x => x.Id == CurrentSalePointId)));



                      }
                  });

            getOrdersCount();

        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
        }

    }

    private Orders createOrder;

    /// <summary>
    ///  Сделать  popup с выбором стола.... Выбор помещение  сделать в настройках?
    /// </summary>
    /// <param ></param>
    /// <returns></returns>
    [RelayCommand]
    private async Task SeletTable(Table currentTable)
    {
        try
        {
            if (!IsNotNull(createOrder))
                return;
            IsTableSelect = false;


            await realm.WriteAsync(() =>
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
            if (HomePageViewModel.CurrentBrand.IsGuestsCount)
            {
                createOrder.GuestCount = 1;
                await SaveOrder(createOrder);
            }
            else
            {
                IsGuestSelect = true;
            }

        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }
    [RelayCommand]
    private async Task SeletGuestCount(string guest_count)
    {
        try
        {
            int.TryParse(guest_count, out int num);
            await realm.WriteAsync(() =>
             {
                 if (num > 0)
                 {

                     createOrder.GuestCount = num;

                 }
             });
            await SaveOrder(createOrder);
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
        // closing_popups();

    }


    public void UpdateOrders()
    {
        if (ListView.DataSource != null)
        {
            ListView.DataSource.Filter = FilterContacts;
            ListView.DataSource.RefreshFilter();
        }
        getOrdersCount();
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
                if (IsAddNewOrder)
                {
                    if (OrderDetails.Count() <= 0)
                    {

                        IsAddNewOrder = false;
                        return;
                    }
                }
                return;
            }
            var order_type = orderDetail.OrderType;
            //var order_type = orderDetail.OrderType switch
            //{
            //    OrderType.InTheInstitution => OrderType.InTheInstitution,
            //    "takeaway" => OrderType.TakeAway,
            //    "dilivery" => OrderType.Delivery,
            //    _ => OrderType.InTheInstitution
            //};

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
                    OrderDelivery = order_delivery,


                };
            });
            if (order_type == OrderType.InTheInstitution)
            {
                IsTableSelect = true;
                return;
            }



            createOrder.GuestCount = 1;

            await SaveOrder(createOrder);
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
        //var dic = new Dictionary<string, object>();
        //dic.Add("order", order);

        //await AppShell.Current.GoToAsync($"{nameof(OrderDetailPage)}?is_new=true", dic);


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

                for (int i = 0; i < order.GuestCount; i++)
                {
                    order.OrderSales.Add(new OrderSale { GuestIndex = 1 + i });

                    order.Logs.Add(new Logs { CreaterDate = DateTimeOffset.Now, Title = $"{AppResources.GuestAdded} {AppResources.Guest} {i + 1}", DeviceName = CurrentDivaceId });
                }

                order.CreaterEmpId = HomePageViewModel.CurrentUser.Id;
                realm.Add(order);

            });



            //var dic = new Dictionary<string, object>();
            //dic.Add("order", order);

            //await AppShell.Current.GoToAsync($"{nameof(OrderDetailPage)}?is_new=true", dic);

            HomePageViewModel.CurrentState = StateKey.OrderDetail;
            var orderPage = HomePageViewModel.getInstanceOrderDetailPage();
            await Task.Delay(100);
            closing_popups();


            orderPage.Order = order;

            orderPage.OnAppearing();
            IsAddNewOrder = false;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }




    [RelayCommand]
    private async Task ChangeOrderStatus(Orders order)
    {
        try
        {
            if (!IsNotNull(order, order.OrderDelivery)) return;

            //if (!IsNotNull(order.OrderDelivery.Courier) && order.OrderDelivery.DeliveryStatus == DeliveryStatus.Success) return;

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

            if (ListView.DataSource != null)
            {
                ListView.DataSource.Filter = FilterContacts;
                ListView.DataSource.RefreshFilter();
            }
            getOrdersCount();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private void closing_popups()
    {
        IsAddNewOrder = false;
        IsOrderType = false;
        IsTableSelect = false;
        IsGuestSelect = false;
    }

    [RelayCommand]
    public async Task ChangeType(string parm)
    {
        try
        {
            IsOrderType = !IsOrderType;
            if (parm.Equals("Border"))
                return;
            IsInRestoran = parm == "Restoran";
            IsOutRestoran = parm == "OutRestoran";
            IsInDelivered = parm == "Delivered";

            if (IsInRestoran) OrdersType = AppResources.InEnvy;
            else if (IsOutRestoran) OrdersType = AppResources.TakeAway;
            else if (IsInDelivered) OrdersType = AppResources.DeliveryLabel;
            else OrdersType = AppResources.AllOrders;

            /*       Preferences.Set(nameof(IsInRestoran), IsInRestoran);
                   Preferences.Set(nameof(IsOutRestoran), IsOutRestoran);
                   Preferences.Set(nameof(IsInDelivered), IsInDelivered);*/

            MakeFilter();

            // LoadOrders();  
            getOrdersCount();

            await Task.CompletedTask;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }




    #region Filters
    [RelayCommand]
    public void FilterChanges(string parm)
    {
        try
        {

            IsAllOrders = parm == "all";
            IsNewOrders = parm == "new";
            IsOnlineOrders = parm == "online";
            IsSuccessOrders = parm == "succes";
            IsInWayOrders = parm == "inway";
            IsDelivered = parm == "delivered";
            Preferences.Set($"{nameof(IsAllOrders)}{CurrentSalePointId}", IsAllOrders);
            Preferences.Set($"{nameof(IsNewOrders)}{CurrentSalePointId}", IsNewOrders);
            Preferences.Set($"{nameof(IsOnlineOrders)}{CurrentSalePointId}", IsOnlineOrders);
            Preferences.Set($"{nameof(IsSuccessOrders)}{CurrentSalePointId}", IsSuccessOrders);
            Preferences.Set($"{nameof(IsInWayOrders)}{CurrentSalePointId}", IsInWayOrders);
            Preferences.Set($"{nameof(IsDelivered)}{CurrentSalePointId}", IsDelivered);

            if (ListView.DataSource != null)
            {
                ListView.DataSource.Filter = FilterContacts;
                ListView.DataSource.RefreshFilter();
            }
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
        //MakeFilter();
        //LoadOrders();

    }

    private bool FilterContacts(object obj)
    {
        //order.OrderDelivery.DeliveryStatus = DeliveryStatus.Success;
        try
        {
            var item = obj as Orders;

            var check = false;

            if (IsAllOrders && item.OrderStatus != OrderStatus.Closed && item.OrderStatus != OrderStatus.Deleted)
                check = true;
            else if (IsNewOrders && item.OrderStatus == OrderStatus.New && item.OrderStatus != OrderStatus.Closed && item.OrderStatus != OrderStatus.Deleted)
                check = true;
            else if (IsOnlineOrders && item.IsOnline && item.OrderStatus != OrderStatus.Closed && item.OrderStatus != OrderStatus.Deleted)
                check = true;
            else if (IsSuccessOrders && item.OrderStatus != OrderStatus.Closed && item.OrderDelivery?.DeliveryStatus == DeliveryStatus.Success && item.OrderStatus != OrderStatus.Closed && item.OrderStatus != OrderStatus.Deleted)
                check = true;
            else if (IsInWayOrders && item.OrderStatus != OrderStatus.Closed && item.OrderDelivery?.DeliveryStatus == DeliveryStatus.OnWay && item.OrderStatus != OrderStatus.Closed && item.OrderStatus != OrderStatus.Deleted)
                check = true;
            else if (IsDelivered && item.OrderStatus != OrderStatus.Closed && item.OrderDelivery?.DeliveryStatus == DeliveryStatus.Delivered && item.OrderStatus != OrderStatus.Closed && item.OrderStatus != OrderStatus.Deleted)
                check = true;
            else
                check = false;

            if (IsInRestoran)
            {
                if (check && item.Detail?.OrderType == OrderType.InTheInstitution)
                    return true;
                else
                    return false;
            }
            else if (IsOutRestoran)
            {
                if (check && item.Detail?.OrderType == OrderType.TakeAway)
                    return true;
                else
                    return false;
            }
            else if (IsInDelivered)
            {
                if (check && item.Detail?.OrderType == OrderType.Delivery)
                    return true;
                else
                    return false;
            }
            return check;
        }
        catch (Exception ex) { Crashes.TrackError(ex); return false; }

    }

    public async void MakeFilter()
    {
        try
        {
            if (IsNotNull(ListView))
            {
                if (ListView.DataSource != null)
                {
                    if (ListView.DataSource.Filter == null)
                    {
                        ListView.DataSource.Filter = FilterContacts;
                    }
                    ListView.DataSource.RefreshFilter();
                }
            }
            else
            {
                await Task.Delay(200);

                if (ListView.DataSource != null)
                {
                    if (ListView.DataSource.Filter == null)
                    {
                        ListView.DataSource.Filter = FilterContacts;
                    }
                    ListView.DataSource.RefreshFilter();
                }

            }
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

        //OrdersFilter = filter switch
        //{
        //    /*  OrderFilter.New => $"order_state=='{nameof(OrderState.New)}'",
        //      OrderFilter.Online => $"order_state=='{nameof(OrderState.Online)}'",
        //      OrderFilter.Success => $"order_state=='{nameof(OrderState.Success)}'",
        //      OrderFilter.InWay => $"order_state=='{nameof(OrderState.InWay)}'",
        //      OrderFilter.Delivered => $"order_state=='{nameof(OrderState.Delivered)}'",
        //      //_ => "order_state != nil"x=> x.OrderState is OrderState.New || x.OrderState is OrderState.Online || x.OrderState is OrderState.InWay||x.OrderState==OrderState.Success||x.OrderState== OrderState.Delivered
        //      _ => $"(order_state == '{nameof(OrderState.New)}' OR order_state == '{nameof(OrderState.Online)}' OR order_state=='{nameof(OrderState.InWay)}' OR order_state=='{nameof(OrderState.Success)}' OR order_state=='{nameof(OrderState.Delivered)}')"*/
        //    OrderFilter.New => CriteriaOperator.FromLambda<Orders>(x => x.OrderStatus == OrderStatus.New),
        //    OrderFilter.Online => CriteriaOperator.FromLambda<Orders>(x => x.IsOnline),
        //    OrderFilter.Success => CriteriaOperator.FromLambda<Orders>(x => x.OrderDelivery.DeliveryStatus == DeliveryStatus.Success && x.OrderStatus == OrderStatus.Delivery),
        //    OrderFilter.InWay => CriteriaOperator.FromLambda<Orders>(x => x.OrderDelivery.DeliveryStatus == DeliveryStatus.OnWay && x.OrderStatus == OrderStatus.Delivery),
        //    OrderFilter.Delivered => CriteriaOperator.FromLambda<Orders>(x => x.OrderDelivery.DeliveryStatus == DeliveryStatus.Delivered &&x.OrderStatus==OrderStatus.Delivery),


        //    _ => CriteriaOperator.FromLambda<Orders>(x => x.OrderStatus  == OrderStatus.New || x.OrderStatus == OrderStatus.Delivery || x.IsOnline || x.OrderDelivery.DeliveryStatus == DeliveryStatus.OnWay
        //        || x.OrderStatus == OrderStatus.Success || x.OrderDelivery.DeliveryStatus == DeliveryStatus.Delivered)

        //};
        /*


                    _ => CriteriaOperator.FromLambda<Orders>(x => x.OrderStatus == OrderStatus.New || x.OrderStatus == OrderStatus.Delivery || x.IsOnline || x.OrderDelivery.DeliveryStatus == DeliveryStatus.OnWay
                        || x.OrderStatus == OrderStatus.Success || x.OrderDelivery.DeliveryStatus == DeliveryStatus.Delivered)


                    //if (IsOutRestoran)

                    //    OrdersFilter = OrdersFilter & (CriteriaOperator.FromLambda<Orders>(x => x.Detail.OrderTypeRaw == (int)OrderType.TakeAway));

                    //else if (IsInDelivered)

                    //    OrdersFilter = OrdersFilter & (CriteriaOperator.FromLambda<Orders>(x => x.Detail.OrderTypeRaw == (int)OrderType.Delivery));


                    OrdersFilter = OrdersFilter & (CriteriaOperator.FromLambda<Orders>(x => x.Detail.OrderTypeRaw == (int)OrderType.TakeAway));


                    //else if (IsInRestoran)

                    //    OrdersFilter = OrdersFilter & (CriteriaOperator.FromLambda<Orders>(x => x.Detail.OrderTypeRaw == (int)OrderType.InTheInstitution));
                    getOrdersCount();


                *//*    try*//*
                    {
                        var all_orders = filter switch
                    {
                        OrderFilter.New => allOrders.Where(x => x.OrderStatusRaw == (int)OrderStatus.New),
                        OrderFilter.Online => allOrders.Where(x => x.IsOnline),
                        OrderFilter.Success => allOrders.Where(x => x.OrderStatusRaw == (int)OrderStatus.Success),
                        OrderFilter.InWay => allOrders.Where(x => x.OrderDelivery != null && x.OrderDelivery.DeliveryStatusRaw == (int)DeliveryStatus.OnWay),
                        OrderFilter.Delivered => allOrders.Where(x => x.OrderDelivery != null && x.OrderDelivery.DeliveryStatusRaw == (int)DeliveryStatus.Delivered),
                        _ => allOrders.ToList().Where(x => x.OrderStatusRaw == (int)OrderStatus.New || x.IsOnline || x.OrderDelivery.DeliveryStatusRaw == (int)DeliveryStatus.OnWay
                        || x.OrderStatusRaw == (int)OrderStatus.Success || x.OrderDelivery.DeliveryStatusRaw == (int)DeliveryStatus.Delivered).AsQueryable(),


                    };

                    if (IsOutRestoran)
                        all_orders = all_orders.Where(x => x.OrderTypeRaw == (int)OrderType.TakeAway);
                    else if (IsInDelivered)
                        all_orders = all_orders.Where(x => x.OrderTypeRaw == (int)OrderType.Delivery);
                    else if (IsInRestoran)
                        all_orders = all_orders.Where(x => x.OrderTypeRaw == (int)OrderType.InTheInstitution);


                        Order = all_orders.ToList().OrderByDescending(x => x.Number).AsQueryable(); ;

                    }catch(Exception ex)
                    {
                        var i = ex;
                    }

            */
    }

    private void loadSavingFilter()
    {
        IsAllOrders = Preferences.Get($"{nameof(IsAllOrders)}{CurrentSalePointId}", false);
        IsNewOrders = Preferences.Get($"{nameof(IsNewOrders)}{CurrentSalePointId}", false);
        IsOnlineOrders = Preferences.Get($"{nameof(IsOnlineOrders)}{CurrentSalePointId}", false);
        IsSuccessOrders = Preferences.Get($"{nameof(IsSuccessOrders)}{CurrentSalePointId}", false);
        IsInWayOrders = Preferences.Get($"{nameof(IsInWayOrders)}{CurrentSalePointId}", false);
        IsDelivered = Preferences.Get($"{nameof(IsDelivered)}{CurrentSalePointId}", false);

        IsAllOrders = !IsNewOrders && !IsOnlineOrders && !IsSuccessOrders && !IsInWayOrders && !IsDelivered;

        MakeFilter();
    }

    private void getOrdersCount()
    {

        try
        {
            realm ??= GetRealm();
            //var all_orders = filter switch
            //{
            //    OrderFilter.New => allOrders.Where(x => x.OrderStatusRaw == (int)OrderStatus.New).ToList(),
            //    OrderFilter.Online => allOrders.Where(x => x.IsOnline).ToList(),
            //    OrderFilter.Success => allOrders.Where(x => x.OrderStatusRaw == (int)OrderStatus.Success).ToList(),
            //    OrderFilter.InWay => allOrders.ToList().Where(x => x.OrderDelivery != null && x?.OrderDelivery?.DeliveryStatusRaw == (int)DeliveryStatus.OnWay).ToList(),
            //    OrderFilter.Delivered => allOrders.ToList().Where(x => x.OrderDelivery != null && x?.OrderDelivery?.DeliveryStatusRaw == (int)DeliveryStatus.Delivered).ToList(),
            //    _ => allOrders.ToList().Where(x => x.OrderStatusRaw == (int)OrderStatus.New || x.IsOnline || x?.OrderDelivery?.DeliveryStatusRaw == (int)DeliveryStatus.OnWay
            //    || x.OrderStatusRaw == (int)OrderStatus.Success || x?.OrderDelivery?.DeliveryStatusRaw == (int)DeliveryStatus.Delivered).ToList(),


            //};

            //count = allOrders.Count();
            //var all_orders = allOrders.ToList().Where(x => x.OrderStatus == OrderStatus.New ||x.OrderStatus==OrderStatus.Delivery || x.IsOnline || x?.OrderDelivery?.DeliveryStatus == DeliveryStatus.OnWay
            //    || x.OrderStatus == OrderStatus.Success || x?.OrderDelivery?.DeliveryStatus == DeliveryStatus.Delivered).ToList();
            //if (IsOutRestoran)
            //    all_orders = all_orders.Where(x => x.Detail.OrderTypeRaw == (int)OrderType.TakeAway).ToList();
            //else if (IsInDelivered)
            //    all_orders = all_orders.Where(x => x.Detail.OrderTypeRaw == (int)OrderType.Delivery).ToList();
            //else if (IsInRestoran)
            //    all_orders = all_orders.Where(x => x.Detail.OrderTypeRaw == (int)OrderType.InTheInstitution).ToList();





            var all_orders = Orderss.ToList();


            AllCount = all_orders.Count(x => x.OrderStatus != OrderStatus.Closed && x.OrderStatus != OrderStatus.Deleted);
            NewCount = all_orders.Count(x => x.OrderStatus == OrderStatus.New && x.OrderStatus != OrderStatus.Closed && x.OrderStatus != OrderStatus.Deleted);
            OnlineCount = all_orders.Count(x => x.IsOnline && x.OrderStatus != OrderStatus.Closed && x.OrderStatus != OrderStatus.Deleted);
            InWayCount = all_orders.Count(x => x.OrderDelivery?.DeliveryStatus == DeliveryStatus.OnWay && x.OrderStatus == OrderStatus.Delivery && x.OrderStatus != OrderStatus.Closed && x.OrderStatus != OrderStatus.Deleted);
            DeliveryCount = all_orders.Count(x => x.OrderDelivery?.DeliveryStatus == DeliveryStatus.Delivered && x.OrderStatus == OrderStatus.Delivery && x.OrderStatus != OrderStatus.Closed && x.OrderStatus != OrderStatus.Deleted);
            AlreadyCount = all_orders.Count(x => x.OrderDelivery?.DeliveryStatus == DeliveryStatus.Success && x.OrderStatus == OrderStatus.Delivery && x.OrderStatus != OrderStatus.Closed && x.OrderStatus != OrderStatus.Deleted);
            all_orders = null;
            IsBusy = false;

        }
        catch (Exception ex) { Crashes.TrackError(ex); }



    }

    #endregion 


    [RelayCommand]
    public async void GotoDetail(Orders order)
    {
        try
        {
            if (order is null) return;


            closing_popups();
            HomePageViewModel.CurrentState = StateKey.OrderDetail;
            await Task.Delay(100);
            var orderPage = HomePageViewModel.getInstanceOrderDetailPage();

            orderPage.Order = order;

            _ = orderPage.LoadDependencies();
            //  await Task.Delay(150);

        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    [RelayCommand]
    private async Task GoToPayPage(Orders order)
    {

        try
        {
            if (order.OrderStatus is OrderStatus.Closed)
                return;


            HomePageViewModel.CurrentState = StateKey.PayPage;
            var payPage = HomePageViewModel.getInstancePayPageInstance();
            closing_popups();


            payPage.Order = order;


            payPage.PaymentChange();
            await Task.Delay(150);
            payPage.IsPaymentVisible = true;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
        // payPage.OrderChange();

    }

    internal void OnDissapearing()
    {
        subscribeOrders?.Dispose();
    }


}
