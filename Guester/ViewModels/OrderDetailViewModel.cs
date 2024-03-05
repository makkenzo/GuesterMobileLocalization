using System.Collections.ObjectModel;
using System.Net;
using System.Security.Cryptography;
//using DevExpress.Maui.CollectionView;
//using DevExpress.Maui.Core.Internal;
using Guester.Models;
using Guester.Resources;
using Microsoft.Maui.Animations;
using Microsoft.Maui.Controls;
using MvvmHelpers;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.ListView;
using static Guester.ViewModels.HomePageViewModel;


namespace Guester.ViewModels;

/// <summary>
/// Это не используется 
/// TODO DELETE THIS 
/// </summary>
public partial class OrderDetailViewModel : BaseViewModel
{
    private Realm realm;
    private IDisposable subscribeOrders;
    private bool isClient, isDelivery, isDiscountClientBonus;
    private Menu Menu;
    private string searchTextProducts, searchTextClients;
    private string parentItemId;
    private IEnumerable<Items> allItems;
    private Items currentItemWithModif;
    private IQueryable<Client> Clients;
    private OrderSale CurrentGuest;


    [ObservableProperty]
    bool isAddNewClient;

    [ObservableProperty]
    Orders order = new();

    [ObservableProperty]
    IQueryable<Orders> allOrders;


    [ObservableProperty]
    ObservableRangeCollection<Items> visualItems = new();

    [ObservableProperty]
    ObservableRangeCollection<Client> visualClients = new();


    [ObservableProperty]
    ObservableRangeCollection<Modifiers> modifiers = new();

    [ObservableProperty]
    ObservableRangeCollection<object> selectedModifiers = new();

    [ObservableProperty]
    Client currentClient, newClient;

    [ObservableProperty]
    string categoryTitle = AppResources.AllProducts;

    [ObservableProperty]
    bool isOrderListVisible, isExtraOn, isModificationOn, isTakeaway;

    [ObservableProperty]
    ObservableRangeCollection<ClientAddress> clientAddress = new();

    [ObservableProperty]
    ObservableRangeCollection<ClientCard> clientCards = new();
    [ObservableProperty]
    CustomObservableCollection<Promotion> promotions = new();
    [ObservableProperty]
    HomePageViewModel homepage;


    [ObservableProperty]
    bool isOrderComment;


    public bool IsPromotionCount { get => PromotionCount > 0; set { } }

    [ObservableProperty]
    int promotionCount;

    [ObservableProperty]
    string courier;

    [ObservableProperty]
    string orderComment = string.Empty;


    [ObservableProperty]
    bool isPromotion;


    [ObservableProperty]
    ObservableRangeCollection<DeliveryZone> deliveryZones = new();

    //[ObservableProperty]
    //IQueryable<Employer> courers;


    private CustomObservableCollection<Promotion> promotion_in_order = new();


    public bool IsClient { get => isClient; set { isClient = value; OnPropertyChanged(nameof(IsClient)); OnPropertyChanged(nameof(IsReceipt)); } }
    public bool IsDelivery { get => isDelivery; set { isDelivery = value; OnPropertyChanged(nameof(IsDelivery)); OnPropertyChanged(nameof(IsDeliveryOrder)); OnPropertyChanged(nameof(IsReceipt)); } }
    //public bool IsTakeaway { get => isTakeaway; set { isTakeaway = value; OnPropertyChanged(nameof(isTakeaway)); OnPropertyChanged(nameof(IsTakeAwayOrder)); OnPropertyChanged(nameof(IsReceipt)); } }
    public bool IsReceipt { get => !IsClient && !IsDelivery; }
    public bool IsItems { get => !IsPromotion; }

    public string SearchTextProducts { get { return searchTextProducts; } set { searchTextProducts = value; OnPropertyChanged(nameof(SearchTextProducts)); OnSearchTextChanged(); } }
    public string SearchTextClient { get { return searchTextClients; } set { searchTextClients = value; OnPropertyChanged(nameof(searchTextClients)); OnSearchClientTextChanged(); } }

    public SfListView ListView { get; set; }


    public decimal PriceModifications { get => SelectedModifiers.OfType<Goods>().Sum(x => x.Price); }




    //public bool IsDeliveryOrTakeAway { get => Order.OrderType != OrderType.InTheInstitution && IsDelivery; }
    public bool IsDeliveryOrder { get => Order.Detail.OrderType == OrderType.Delivery && IsDelivery; }


    public bool IsTakeAwayOrder { get => Order.Detail.OrderType == OrderType.TakeAway; }



    public string Title { get => Order.Detail.OrderType switch { OrderType.TakeAway => AppResources.TakeAway, OrderType.Delivery => AppResources.DeliveryLabel, _ => "" }; }


    public OrderDetailViewModel()

    {
        realm ??= GetRealm();
        Clients = realm.All<Client>();
        VisualClients.ReplaceRange(Clients.ToList());
        Homepage = HomePageViewModel.getInstance();
        //Courers = realm.All<Employer>().ToList().Where(x => x.Post?.IsCourier == true).AsQueryable();

    }




    private void Promotions_ItemAdded(object sender, Promotion promotion)
    {
        if (IsNotNull(promotion))
            PromotionCount++;


        //Визуальное отображение количество новых акций

    }

    private async void Promotions_InOrder_ItemAdded(object sender, Promotion promotion)
    {
        //Перерасчет стоимости при добавлениее новой акции в заказ  ? много не состыковывается  внушительно переделать

        if (!IsNotNull(promotion)) return;

        if (isDiscountClientBonus)
        {
            await realm.WriteAsync(() =>
            {

                Order.OrderSales.ToList().ForEach(x =>
                {

                    x.DiscountPercent = 0;
                    x.DiscountSum = 0;

                });

            });
            isDiscountClientBonus = false;
        }


        foreach (var condition in promotion.PromotionConditions)
        {
            if (condition.IsDeleted) continue;

            var products = Order.OrderSales.Where(x => x.Product.Id == condition.ProductId);

            if (products.Count() < 1) continue;

            if (condition.ForHowLong != 0 && condition.ForHowLong <= products.Sum(x => x.TotalPrice))
            {
                await recalculate_discount(products, promotion);
                break;
            }
            if (condition.IsCategory)
            {

                if (products.FirstOrDefault().Category?.Id == condition?.CategoryId)
                {
                    await recalculate_discount(products, promotion);

                    break;
                }
                continue;
            }

            var amount = products.Sum(x => x.Amount);
            if (condition.IsEquels)
            {
                if (products.Count() == amount)
                {
                    await recalculate_discount(products, promotion);
                    break;
                }
                continue;
            }
            if (products.Count() >= amount)
            {
                await recalculate_discount(products, promotion);
                break;
            }







        }


    }

    private async Task recalculate_discount(IEnumerable<OrderSale> products, Promotion promotion)
    {
        await realm.WriteAsync(() =>
        {

            foreach (var product in products)
            {

                switch (promotion.ResultType)
                {

                    case ResultType.DiscountPercent:
                        product.DiscountPercent = (double)promotion.ResultValue;
                        break;
                    case ResultType.FixedPrice:
                        product.DiscountSum = promotion.ResultValue;
                        break;
                    case ResultType.BonusGoods:
                        break;
                    case ResultType.FixedDiscountAmount:
                        break;
                }


            }

        });
    }

    public async void OnAppearing()
    {
        subscribeOrders = Order.OrderSales
             .SubscribeForNotifications(async (sender, changes) =>
             {
                 if (changes == null)
                     return;

                 foreach (var i in changes.DeletedIndices)
                 {
                     await RecalculationGuest();

                     return;

                 }
             });
        _ = loadDependencies();

        await Task.Delay(150);
        Homepage.CurrentState = StateKey.OrderDetail;
    }

    partial void OnSelectedModifiersChanged(ObservableRangeCollection<object> value) => OnPropertyChanged(nameof(PriceModifications));

    partial void OnIsPromotionChanged(bool value) => OnPropertyChanged(nameof(IsItems));

    partial void OnPromotionCountChanged(int value) => OnPropertyChanged(nameof(IsPromotionCount));

    partial void OnCurrentClientChanged(Client value)
    {
        if (IsNotNull(value))
            _ = AddOrderDeliveryFromClient(value);
    }

    partial void OnOrderChanged(Orders value)
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(IsTakeAwayOrder));

        OnPropertyChanged(nameof(IsDeliveryOrder));

    }

    [RelayCommand]
    private void AddDeliveryToOrder()
    {
        if (Order is null)
            return;

        Order.Detail.OrderType = OrderType.Delivery;


    }



    [RelayCommand]
    private async Task OpenCommentOrder(string parm)
    {
        if (!IsNotNull(Order) || !IsNotNull(parm)) return;

        if (parm.Equals("open") || parm.Equals("close"))
        {
            IsExtraOn = false;
            IsOrderComment = !IsOrderComment;
            OrderComment = Order.OrderReceipt.Description;

            return;
        }
        if (!IsNotNull(OrderComment))
        {
            await DialogService.ShowToast(AppResources.FillAllField);
            return;
        }
        await realm.WriteAsync(() =>
        {
            Order.OrderReceipt.Description = OrderComment;
            IsExtraOn = false;
            IsOrderComment = false;

        });


    }

    [RelayCommand]
    private async Task AddGuest()
    {

        if (!IsNotNull(Order))
            return;

        var count = 0;
        var maxIndex = Order.OrderSales.ToList().Max(x => x.GuestIndex);


        await realm.WriteAsync(() =>
        {
            Order.OrderSales.Add(new OrderSale { GuestIndex = maxIndex, });
        });
    }

    private async Task SaveOrderTotalSumm()
    {
        if (!IsNotNull(Order))
            return;

        await realm.WriteAsync(() =>
        {

            Order.OrderReceipt.ResultSum = Order.OrderSales.Sum(x => x.TotalPrice);
        }
        );
        await Task.Delay(200);
        OnPropertyChanged(nameof(Order));

    }

    [RelayCommand]
    private void SelectedGuestOrClient(GroupResult groupResult)
    {
        var guest = Order.OrderSales.FirstOrDefault(x => x.GuestIndex.Equals((int)groupResult.Key));
        if (IsNotNull(CurrentGuest) && Order.OrderSales.Count > 0)
            CurrentGuest.IsActive = false;
        CurrentGuest = guest;
        CurrentGuest.IsActive = true;
    }

    [RelayCommand]
    private async Task GoToPayPage()
    {
        if (!IsNotNull(Order))
            return;
        if (Order.OrderStatus is OrderStatus.Closed)
            return;

        await save_order();


        PayPageViewModel payPage = HomePageViewModel.getInstancePayPageInstance();
        payPage.Order = Order;
        Homepage.CurrentState = StateKey.PayPage;
        await Task.Delay(50);
        payPage.PaymentChange();

        await Task.Delay(250);
        payPage.IsPaymentVisible = true;
    }

    [RelayCommand]
    private async Task DeleteGuestOrClient(OrderSale guest)
    {
        if (!IsNotNull(guest, Order))
            return;

        if (Order.OrderSales.Count <= 1)
            return;


        await realm.WriteAsync(() =>
        {

            Order.OrderSales.Remove(guest);


            _ = SaveLog($"{Resources.AppResources.Guest} {guest.GuestIndex} {Resources.AppResources.WasDeleted}");

        });



    }


    /// <summary>
    /// Возможно перенести в popup
    /// </summary>
    ///

    [RelayCommand]
    private void AddNewAddresNewClien()
    {
        if (!IsNotNull(NewClient) || !IsNotNull(ClientAddress.LastOrDefault()?.Address))
            return;



        ClientAddress.Add(new());

    }

    [RelayCommand]
    private void AddNewCardNewClien()
    {

        if (!IsNotNull(NewClient) || !IsNotNull(ClientCards.LastOrDefault()?.CardNumber))
            return;

        ClientCards.Add(new());
    }

    [RelayCommand]
    private async Task AddNewClient(string parm)
    {
        if (parm == "open" || parm == "close")
        {
            IsAddNewClient = !IsAddNewClient;
            //await Task.Delay(10); // для быстрой открития модальный окна
            if (IsAddNewClient)
            {
                ClientAddress.Add(new()); // Из-за этих двух строчек окно долго открывается TODO испрвить это нужно
                ClientCards.Add(new());    // в следущий раз перед тем как гнать проверь свой код...
                await Task.Delay(50);
                NewClient = new()
                {
                    BrandId = CurrentBrandId,
                    UserId = CurrentEmpId

                };





            }
            return;
        }
        if (!IsNotNull(NewClient.FullName, NewClient.PhoneNumber, NewClient.Gender))
        {
            await DialogService.ShowToast(AppResources.FillAllField);
            return;
        }
        using (var transaction = realm.BeginWrite())
        {

            try
            {

                ClientAddress.Where(x => IsNotNull(x.Address)).ToList().ForEach(NewClient.ClientAddress.Add);
                ClientCards.Where(x => IsNotNull(x.CardNumber)).ToList().ForEach(NewClient.ClientCard.Add);
                ClientCards?.Clear();
                ClientAddress?.Clear();

                NewClient.PhoneNumber = new string(NewClient.PhoneNumber.Where(char.IsDigit).ToArray());


                realm.Add(NewClient);

                Order.Client = NewClient;



                if (transaction.State == TransactionState.Running)
                {
                    transaction.Commit();
                }
                CurrentClient = Order.Client;
                VisualClients.ReplaceRange(Clients.ToList());
                if (Order.Detail.OrderType is OrderType.Delivery)
                    _ = AddOrderDeliveryFromClient(NewClient);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);

                if (transaction.State != TransactionState.RolledBack &&
                    transaction.State != TransactionState.Committed)
                {
                    transaction.Rollback();
                }
            }
        }



        IsAddNewClient = false;

    }

    [RelayCommand]
    private async Task AddClientToOrder(object sender)
    {

        var client = sender as Client;
        if (!IsNotNull(client, Order))
            return;


        await realm.WriteAsync(() =>
        {
            Order.Client = client;
        });

        _ = SaveOrderTotalSumm();
        // IsClient = false;
    }

    [RelayCommand]
    public void SelectedOrderType(string parm)
    {
        IsClient = parm == nameof(IsClient);
        IsDelivery = parm == nameof(IsDelivery);
        IsTakeaway = (IsDelivery && Order.Detail.OrderType == OrderType.TakeAway);
    }

    [RelayCommand]
    private async void SelectOrder(Orders order)
    {

        if (!IsNotNull(order))
            return;
        IsOrderListVisible = false;
        await Task.Delay(100);
        _ = SaveOrderTotalSumm();
        Order = order;


        CurrentClient = Order.Client;
        _ = getOrders();
    }

    [RelayCommand]
    private void GoToPreviousCategory()
    {
        if (CategoryTitle.Equals(AppResources.AllItems))
            return;

        if (CategoryTitle.Equals("..."))
        {
            CategoryTitle = AppResources.AllItems;
            VisualItems.ReplaceRange(allItems.Where(x => x.IsMain));
            SearchTextProducts = string.Empty;
            return;
        }



        if (IsNotNull(parentItemId))
        {
            var currentItem = allItems.FirstOrDefault(x => x.Id == parentItemId);
            CategoryTitle = currentItem.Name;

            VisualItems.ReplaceRange(allItems.Where(x => x.ParentItemId == currentItem.Id));
            parentItemId = currentItem.ParentItemId;
        }
        else
        {
            CategoryTitle = AppResources.AllItems;
            VisualItems.ReplaceRange(allItems.Where(x => x.IsMain));
        }
    }

    [RelayCommand]
    private async Task MinusProduct(OrderSale orderProduct)
    {
        if (!IsNotNull(orderProduct, CurrentGuest))
            return;

        if (Homepage.CurrentBrand.OnProductDelete)
        {

            var res = await DialogService.ShowAdminView();
            if (!res)
            {
                return;
            }
        }

        if (orderProduct.Amount <= 1)
        {
            if (Order.OrderSales.Count <= 1)
                return;
            await realm.WriteAsync(() =>
            {
                _ = SaveLog($"{Resources.AppResources.ProductWasRemoved} - {orderProduct.Name}");
                var guestIndex = orderProduct.GuestIndex;
                Order.OrderSales.Remove(orderProduct);

                if (Order.OrderSales.Count(x => x.GuestIndex == guestIndex) == 0)
                {
                    var tempGuest = new OrderSale { Amount = guestIndex };
                    Order.OrderSales.Add(tempGuest);
                    CurrentGuest = tempGuest;
                }

            });
            ListView.DataSource.RefreshFilter();
            return;
        }
        await realm.WriteAsync(() =>
        {

            orderProduct.Amount--;
        });
        _ = SaveOrderTotalSumm();

        _ = SaveLog($"{Resources.AppResources.TookAwayQuantityProduct} - {orderProduct.Name}");

    }

    [RelayCommand]
    private async Task PlusProduct(OrderSale orderProduct)
    {
        if (!IsNotNull(orderProduct, CurrentGuest))
            return;

        await realm.WriteAsync(async () =>
        {
            try
            {

                if (!IsNotNull(orderProduct))
                    return;

                orderProduct.Amount++;
                _ = check_promotion(orderProduct);

                _ = SaveLog($"{Resources.AppResources.AddedProductQuantity} - {orderProduct.Name}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{Resources.AppResources.ErrorAddingProduct} : {ex.Message}");
            }

            _ = SaveOrderTotalSumm();

        });

    }

    private async Task check_promotion(OrderSale product)
    {


        if (!IsNotNull(product, Order)) return;

        realm ??= GetRealm();
        if (realm.IsClosed) realm = GetRealm();


        var promotions = realm.All<Promotion>().ToList().Where(x => x.PromotionConditions.ToList().Any(y => y.ProductId == product.Product.Id) && !x.IsDeleted).AsQueryable();
        if (promotions.Count() < 1)
            return;
        var total_promotions = ContainsPromotionConditions(product, promotions);
        if (total_promotions.Count() < 1)
            return;

        total_promotions.Where(x => Promotions.ToList().Any(y => y.Id != x.Id)).ToList().ForEach(Promotions.Add);
    }

    private List<Promotion> ContainsPromotionConditions(OrderSale product, IQueryable<Promotion> promotions)
    {
        var totaly = new List<Promotion>();

        foreach (var promotion in promotions)//чтобы какое ни будь условие совпадало с акцией
        {
            foreach (var promotionCondition in promotion.PromotionConditions)
            {
                if (promotionCondition.ProductId != product.Product.Id || promotionCondition.IsDeleted) continue;

                if (promotionCondition.ForHowLong != 0 && promotionCondition.ForHowLong <= Order.OrderSales.Where(x => x.Product.Id == promotionCondition.ProductId).Sum(x => x.TotalPrice))
                {
                    totaly.Add(promotion);
                    break;
                }
                if (promotionCondition.IsCategory)
                {
                    if (product.Category?.Id == promotionCondition?.CategoryId)
                    {
                        totaly.Add(promotion);
                        break;
                    }

                }
                if (promotionCondition.IsEquels)
                {
                    if (Order.OrderSales.Count(x => x.Product.Id == product.Product.Id) == promotionCondition.Count)
                    {
                        totaly.Add(promotion);
                        break;
                    }
                }
                else
                {
                    if (Order.OrderSales.Count(x => x.Product.Id == product.Product.Id) >= promotionCondition.Count)
                    {
                        totaly.Add(promotion);
                        break;
                    }
                }
            }
        }
        return totaly;
    }

    [RelayCommand]
    private async Task SelectedPromotion(Promotion promotion)
    {
        if (!IsNotNull(promotion) || Promotions.Count < 1) return;
        if (!Promotions.Any(x => x.Id == promotion.Id)) return;

        promotion_in_order.Add(promotion);
        await Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SelectProductOrCategory(Items item)
    {
        if (!IsNotNull(item))
            return;
        //realm ??= GetRealm();
        /*  using (var transaction = realm.BeginWrite())
          {
  */
        if (item.IsCategory)
        {
            VisualItems.ReplaceRange(allItems.Where(x => x.ParentItemId == item.Id));
            CategoryTitle = item.Name;
            parentItemId = item.ParentItemId;

            return;
        }



        if (!IsNotNull(CurrentGuest))
            return;




        if (item.Product.Modifiers.Count() > 0 && item.Product.IsModify)
        {
            Modifiers.Clear();
            Modifiers.AddRange(item.Product.Modifiers.ToList());
            currentItemWithModif = item;
            OpenOrCloseModifiersList();
            return;
        }
        var orderProduct = Order.OrderSales.ToList().FirstOrDefault(x => x.Product.Id == item.Product.Id && x.GuestIndex == CurrentGuest?.GuestIndex);
        //transaction.Commit();

        if (IsNotNull(orderProduct))
        {
            _ = PlusProduct(orderProduct);
            return;
        }



        await add_product_to_orderSale(item, null);
        _ = check_promotion(orderProduct);

        _ = SaveOrderTotalSumm();


        if (ListView.DataSource != null)
        {
            ListView.DataSource.RefreshFilter();
        }

    }

    private async Task add_product_to_orderSale(Items item, Goods modifier)
    {


        await realm.WriteAsync(() =>
        {
            if (IsNotNull(modifier))
            {
                var orderSale = Order.OrderSales.FirstOrDefault(x => x.Name == $"{item.Product.Name}\n{modifier.Name}" && x.Price == ((item.IsSelfPrice ? item.Product.TotalSum : item.Price) + modifier.Price) && x.GuestIndex == CurrentGuest.GuestIndex);
                if (IsNotNull(orderSale))
                {
                    ++orderSale.Amount;
                    return;
                }
            }

            var product = Order.OrderSales.FirstOrDefault(x => x.GuestIndex == CurrentGuest.GuestIndex);
            if (!IsNotNull(product?.Product.Id) && Order.OrderSales.Count(x => x.GuestIndex == CurrentGuest.GuestIndex) == 1

                )
            {

                var guest = Order.OrderSales.FirstOrDefault(x => x.GuestIndex == CurrentGuest.GuestIndex);
                guest.Price = (item.IsSelfPrice ? item.Product.TotalSum : item.Price) + (!IsNotNull(modifier) ? 0m : modifier.Price);
                guest.GuestIndex = CurrentGuest.GuestIndex;
                guest.Code = item.Product.Code;
                guest.DiscountPercent = 0;
                guest.Product = item.Product;
                guest.DiscountSum = 0;
                guest.BarCode = item.Product.BarCode;
                guest.Cost = item.Product.CostPrice + (!IsNotNull(modifier) ? 0m : modifier.CostPrice);
                guest.Amount = 1;
                guest.Sum = 0;
                guest.Category = item.Product.Category;
                guest.Product = item.Product;
                guest.Name = $"{item.Product.Name}{(!IsNotNull(modifier) ? "" : $"\n{modifier.Name}")}";
                guest.Vat = 0;
                guest.Tax = item.Product.Tax;
                guest.Psu = "";
                guest.TransferType = TransferType.Card;
                guest.Section = "";
                guest.IsTaxable = false;




                _ = SaveOrderTotalSumm();
                return;
            }

            Order.OrderSales.Add(new OrderSale
            {
                Price = (item.IsSelfPrice ? item.Product.TotalSum : item.Price) + (!IsNotNull(modifier) ? 0m : modifier.Price),
                GuestIndex = CurrentGuest.GuestIndex,
                Code = item.Product.Code,
                DiscountPercent = 0,
                Product = item.Product,
                Cost = item.Product.CostPrice + (!IsNotNull(modifier) ? 0m : modifier.CostPrice),
                DiscountSum = 0,
                BarCode = item.Product.BarCode,
                Amount = 1,
                Sum = 0,
                Category = item.Product.Category,
                Name = $"{item.Product.Name}{(!IsNotNull(modifier) ? "" : $"\n{modifier.Name}")}",
                Vat = 0,
                Tax = item.Product.Tax,
                Psu = "",
                TransferType = TransferType.Card,
                Section = "",
                IsTaxable = false,
            });
        });

    }

    [RelayCommand]
    private async Task SelectedModifications()
    {


        if (!IsNotNull(currentItemWithModif) || SelectedModifiers.Count() <= 0)
        {
            OpenOrCloseModifiersList();
            return;
        }

        SelectedModifiers.ToList().ForEach(async x =>
        {
            var modifier = x as Goods;
            if (IsNotNull(modifier))
                await add_product_to_orderSale(currentItemWithModif, modifier);
        });

        currentItemWithModif = null;
        OpenOrCloseModifiersList();
        _ = SaveOrderTotalSumm();
        await Task.CompletedTask;

    }

    [RelayCommand]
    void OpenOrCloseModifiersList()
    {
        IsModificationOn = !IsModificationOn;
        SelectedModifiers.Clear();
    }

    [RelayCommand]
    private void OpenExtraFunctions(string parm)
    {
        IsExtraOn = !IsExtraOn;
        if (parm.Equals("Border"))
            return;
        else if (parm.Equals("cancelOrder"))
            _ = CancelOrder();

    }

    [RelayCommand]
    private async Task CancelOrder()
    {
        if (!IsNotNull(Order))
            return;
        IsBusy = true;
        _ = SaveLog($"{AppResources.Order} № {Order.Number} {AppResources.WasCancelled}");

        await realm.WriteAsync(() =>
        {
            Order.OrderStatus = OrderStatus.Deleted;
            Order.OrderSales.Clear();
        });
        Back();
        IsBusy = false;
    }

    [RelayCommand]
    private async Task DeleteGuest(GroupResult groupResult)
    {



        if (!IsNotNull(Order, groupResult))
            return;
        var guests = Order.OrderSales.Where(x => x.GuestIndex.Equals(groupResult.Key)).ToList();


        //var guests = Order.OrderSales.Where(x => x.GuestName.Equals(groupInfo.GroupValueText)).ToList();

        if (guests.Count <= 0 || guests.Count == Order.OrderSales.Count())
            return;

        _ = SaveLog($"{AppResources.WasGuestDelete} {guests[0]?.Name}");

        await realm.WriteAsync(() =>
        {
            guests.ForEach(x => Order.OrderSales.Remove(x));


        });

        await RecalculationGuest();


    }

    private async Task getOrders()
    {
        if (Order is null)
            return;

        AllOrders = (
            realm.All<Orders>().ToList().Where(x =>
            (x.Id != Order.Id)
            && Order.OrderReceipt?.Table?.Id == x.OrderReceipt?.Table?.Id
            //&&
            //(x.OrderStatus == OrderStatus.New || x.IsOnline || x.OrderDelivery?.DeliveryStatus == DeliveryStatus.OnWay ||
            //  x.OrderStatus == OrderStatus.Success || x.OrderDelivery?.DeliveryStatus == DeliveryStatus.Delivered)

              )).AsQueryable();

        OnPropertyChanged(nameof(Order));

    }

    public async Task loadDependencies()
    {
        try
        {
            Menu = realm.All<Menu>().ToList().FirstOrDefault(x => x.SalePoints.ToList().Any(y => y.Id == CurrentSalePointId)) ?? realm.All<Menu>().ToList().FirstOrDefault() ?? new();
            //AllOrders = (
            //realm.All<Orders>().
            //Where(x =>
            //(x.Id != Order.Id)
            //&& Order.OrderReceipt.Table.Id == x.OrderReceipt.Table.Id
            //  ));
        }
        catch
        {

        }

        try
        {

            // await Task.Delay(50);
            if (!IsNotNull(Order))
                return;
            Promotions ??= new();
            promotion_in_order ??= new();
            Promotions.Clear();
            promotion_in_order.Clear();
            PromotionCount = 0;
            Promotions.ItemAdded += Promotions_ItemAdded;


            promotion_in_order.ItemAdded += Promotions_InOrder_ItemAdded;
            realm ??= GetRealm();

            allItems = Menu?.Items;
            VisualItems.Clear();
            VisualItems.AddRange(allItems.Where(x => x.IsMain));

            CurrentClient = Order.Client;

            isDiscountClientBonus = Order.OrderSales.All(x => x.DiscountPercent == CurrentClient?.ClientGroup?.Value && x.DiscountSum == CurrentClient?.ClientGroup?.BonusOnBirthday) && CurrentClient?.ClientGroup?.IsDeleted == false;


            _ = SaveLog($"{AppResources.OpenOrder} {Order.Name}");


            if (Order.OrderSales.Count < 1)
                await AddGuest();
            CurrentGuest = Order.OrderSales[0];
            // SelectedGuestOrClient(guest: Order.OrderSales[0]);

            //_=  getOrders();



            //    _ = SaveOrderTotalSumm();
            _ = RecalculationGuest();
            DeliveryZones?.Clear();
            DeliveryZones?.AddRange(realm.All<DeliveryZone>().ToList());
            /*     await Task.Delay(200);

                 AllOrders = (
                     realm.All<Orders>().ToList().Where(x =>
                     (x.Id != Order.Id)
                     && Order.OrderReceipt?.Table?.Id == x.OrderReceipt?.Table?.Id
                       //&&
                       //(x.OrderStatus == OrderStatus.New || x.IsOnline || x.OrderDelivery?.DeliveryStatus == DeliveryStatus.OnWay ||
                       //  x.OrderStatus == OrderStatus.Success || x.OrderDelivery?.DeliveryStatus == DeliveryStatus.Delivered)

                       )).AsQueryable();*/
        }
        catch (Exception ex)
        {
            var i = ex;
        }
        finally
        {




        }




    }

    [RelayCommand]
    private void UnlinkClient()
    {
        CurrentClient = null;

        realm.Write(() =>
        {
            Order.Client = CurrentClient;
        });

    }

    private void OnSearchTextChanged()
    {
        if (!IsNotNull(Order, allItems))
            return;
        if (allItems.Count() < 1)
            return;
        if (IsNotNull(SearchTextProducts))
        {
            CategoryTitle = "...";



            VisualItems.ReplaceRange(
                allItems.Where(x => x.Name.Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase)
            || x?.Product?.Name.Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase) == true)
                );
            return;
        }
        GoToPreviousCategory();

    }

    private void OnSearchClientTextChanged()
    {
        if (!IsNotNull(Order, Clients))
            return;


        if (IsNotNull(SearchTextClient))
        {
            var tempClients = Clients.ToList();
            VisualClients.ReplaceRange(tempClients.Where
                (
                x => x?.FullName.Contains(SearchTextClient, StringComparison.CurrentCultureIgnoreCase) == true
                )
                );
        }
        else
            if (VisualClients.Count != Clients.Count())
            VisualClients.ReplaceRange(Clients.ToList());




    }

    private async Task SaveLog(string title)
    {
        await realm.WriteAsync(() =>
        {
            Order.Logs.Add(new Logs { CreaterDate = DateTimeOffset.Now, Title = title, DeviceName = CurrentDivaceId });
        });
    }

    private async Task RecalculationGuest()
    {
        if (!IsNotNull(Order))
            return;
        int count = 1;


        var guestList = Order.OrderSales.ToList().OrderBy(x => x.GuestIndex);

        var guestTempName = "";
        //await Task.Delay(100);
        realm.Write(() =>
        {
            foreach (var guest in guestList)
            {
                guest.GuestIndex = count++;
            }
        });
        //await Task.Delay(100);


        await Task.CompletedTask;
    }

    [RelayCommand]
    public void OrderList()
    {
        if (Order.Detail.OrderType != OrderType.InTheInstitution)
            return;
        if (!IsOrderListVisible)
            _ = getOrders();
        try
        {
            IsOrderListVisible = !IsOrderListVisible && AllOrders.Count() > 0;
        }
        catch
        {


        }
    }

    private void closing_popup()
    {
        IsOrderListVisible = false;
    }

    [RelayCommand]
    public async void Back()
    {
        _ = save_order();
        closing_popup();




        // await AppShell.Current.GoToAsync("..");
    }
    private async Task AddOrderDeliveryFromClient(Client client)
    {
        if (!IsNotNull(client))
            return;
        realm ??= GetRealm();
        await realm.WriteAsync(async () =>
        {
            var address = client.ClientAddress.FirstOrDefault();
            if (IsNotNull(address))
            {
                Order.OrderDelivery.Address.Address = $"{address.Country} {address.City} ";
                Order.Client = client;
                Order.OrderDelivery.DeliveryStatus = DeliveryStatus.New;
                Order.OrderDelivery.Address.Description = address.Address;
                Order.OrderDelivery.Description = address.Description;
            }
            if (Order.OrderSales.ToList().Any(x => x.DiscountPercent == 0 && x.DiscountSum == 0) && IsNotNull(CurrentClient.ClientGroup) && CurrentClient.ClientGroup?.IsDeleted == false)
                await add_promotion_from_client_group(CurrentClient);
        });

    }

    private async Task add_promotion_from_client_group(Client client)
    {
        if (!IsNotNull(client)) return;
        await realm.WriteAsync(() =>
        {

            Order.OrderSales.ToList().ForEach(x =>
            {
                x.DiscountPercent = client.ClientGroup.Value;
                x.DiscountSum = client.ClientGroup.BonusOnBirthday;


            });
            isDiscountClientBonus = true;

        });
    }

    private async Task save_order()
    {
        await SaveOrderTotalSumm();

        Promotions.ItemAdded -= Promotions_ItemAdded;
        promotion_in_order.ItemAdded -= Promotions_InOrder_ItemAdded;
        IsClient = false;
        IsDelivery = false;
        subscribeOrders?.Dispose();
    }


    [RelayCommand]
    private async Task Print()
    {
        var html = @$"<!DOCTYPE html>
<html lang=""ru"">
<head>
  <meta charset=""UTF-8"">
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
  <style>
      table {{
          width: 100%;
          border-collapse: collapse;
          margin-bottom: 20px;
      }}

      th, td {{
          border: 1px solid #ddd;
          padding: 8px;
          text-align: left;
      }}

      th {{
          background-color: #f2f2f2;
      }}

      tbody tr:nth-child(even) {{
          background-color: #f9f9f9;
      }}
  </style>

</head>
<body>
  <h1 style=""text-align: start; margin-left: 30px;"">Чек</h1>
  <hr />
    {GenerateHtmlTable(Order.OrderSales.ToArray())}
  <hr />

  <div class=""grid"">
    <div class=""label"">{AppResources.Total}:</div>
    
    <div class=""label bold"" style=""text-align: end;"">{Order.OrderReceipt.ResultSum}</div>
    <hr />
    <div class=""label"" v-if=""selectedOrder.orderReceipt.discountSum > 0"">{AppResources.Discount}:</div>
    <div class=""label bold"" style=""text-align: end;"" v-if=""selectedOrder.orderReceipt.discountSum > 0"">{Order.OrderReceipt.DiscountSum} %</div>
    <hr />
    <div class=""label bold"">{AppResources.TotalAmountToBePaid}:</div>
    <div class=""label bold"" style=""text-align: end;"">{Order.OrderReceipt.ResultSum}</div>
  </div>

</body>
</html>
";
        await DependencyService.Get<IPrintService>().PrintHtml(html, "PrintJobName");
    }
    static string GenerateHtmlTable(dynamic[] data)
    {
        StringBuilder htmlTable = new StringBuilder();

        // Открываем теги таблицы
        htmlTable.AppendLine("<table>");
        htmlTable.AppendLine("    <thead>");
        htmlTable.AppendLine("        <tr>");
        htmlTable.AppendLine($"            <th>{AppResources.Name}</th>");
        htmlTable.AppendLine($"            <th>{AppResources.Quantity}</th>");
        htmlTable.AppendLine($"            <th>{AppResources.Price}</th>");
        htmlTable.AppendLine($"            <th>{AppResources.Total}</th>");
        htmlTable.AppendLine("        </tr>");
        htmlTable.AppendLine("    </thead>");
        htmlTable.AppendLine("    <tbody>");

        foreach (var item in data)
        {
            htmlTable.AppendLine("        <tr>");
            htmlTable.AppendLine($"            <td>{item.Name}</td>");
            htmlTable.AppendLine($"            <td>{item.Amount}</td>");
            htmlTable.AppendLine($"            <td>{item.Price}</td>");
            htmlTable.AppendLine($"            <td>{item.TotalPrice}</td>");
            htmlTable.AppendLine("        </tr>");
        }


        htmlTable.AppendLine("    </tbody>");
        htmlTable.AppendLine("</table>");

        return htmlTable.ToString();
    }


}
