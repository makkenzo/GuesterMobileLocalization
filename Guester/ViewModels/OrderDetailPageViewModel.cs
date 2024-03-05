using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Security.Cryptography;


//using DevExpress.Maui.CollectionView;
//using DevExpress.Maui.Core.Internal;
using Guester.Models;
using Guester.Resources;

using Microsoft.AppCenter.Crashes;
using Microsoft.Maui.Layouts;
using MongoDB.Bson.IO;
using MvvmHelpers;

using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.ListView;
using static Guester.ViewModels.HomePageViewModel;
using static Microsoft.Maui.ApplicationModel.Permissions;


namespace Guester.ViewModels;


public partial class OrderDetailPageViewModel : BaseViewModel, IQueryAttributable
{
    private Realm realm;
    private IDisposable subscribeOrders;
    private bool isClient, isDelivery, isDiscountClientBonus;

    private string searchTextProducts, searchTextClients;
    private string parentItemId;

    private Items currentItemWithModif;
    private IQueryable<Client> Clients;
    private bool IsShowAdmin = true;

    private Promotion CurrentBonusPromotion;

    private OrderSale guest { get; set; }
    private OrderSale CurrentGuest
    {
        get { return guest; }
        set
        {
            guest = value;

            if (IsNotNull(value))
            {
                try
                {
                    if (IsNotNull(value.GuestIndex))
                    {
                        var selectedG = GuestGroups.FirstOrDefault(x => x.IsSelected);

                        if (IsNotNull(selectedG))
                        {
                            selectedG.IsSelected = false;
                        }
                        var group = GuestGroups.FirstOrDefault(x => x.GroupIndex == value.GuestIndex);
                        if (IsNotNull(group))
                            group.IsSelected = true;
                    }
                }
                catch { }
            }
        }
    }

    //private Client CurrentClient;


    [ObservableProperty]
    bool isAddNewClient, isOpenOrderDetail;

    [ObservableProperty]
    Orders order = new();

    [ObservableProperty]
    private Menu menu;

    [ObservableProperty]
    IQueryable<Orders> allOrders;


    [ObservableProperty]
    private int f;

    //[ObservableProperty]
    //IEnumerable<Items> visualItems ;

    [ObservableProperty]
    ObservableRangeCollection<Client> visualClients = new();

    [ObservableProperty]
    OrderSale currentOrderSale;


    [ObservableProperty]
    ObservableRangeCollection<Modifiers> modifiers = new();



    [ObservableProperty]
    ObservableRangeCollection<Goods> selectedModifiers = new();

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
    private string commentText;

    [ObservableProperty]
    ObservableRangeCollection<Modifiers> currentModifiersInBonusItems = new();

    [ObservableProperty]
    private bool isPromotionBonusOn, isModifiersBonusOn;

    // public SfListView ListView { get; set; }
    public SfListView ItemsListView { get; set; }
    public SfListView ModifiersListView { get; set; }
    public SfListView AllOrdersListView { get; set; }
    public SfListView PromotionBonusTempateListView { get; set; }
    [ObservableProperty]
    bool isOrderComment, isOpenPromotion;

    public bool IsPromotionCount { get => PromotionCount > 0; set { } }

    [ObservableProperty]
    int promotionCount;

    [ObservableProperty]
    string courier;


    [ObservableProperty]
    bool isMoDifReady;

    [ObservableProperty]
    string orderComment = string.Empty;


    [ObservableProperty]
    bool isPromotion, isSplitOrder;


    [ObservableProperty]
    IQueryable<ClientGroup> clientGruops;


    [ObservableProperty]
    IQueryable<DeliveryZone> deliveryZones;

    //[ObservableProperty]
    //IQueryable<Employer> courers;
    [ObservableProperty]
    ObservableRangeCollection<Items> itemsInPromotion = new();


    [ObservableProperty]
    ObservableRangeCollection<GuestGroup> guestGroups = new();


    private Expression<Func<Items, bool>> ItemsCondition { get { return itemsCondition; } set { itemsCondition = value; MakeFilter(); } }
    private Expression<Func<Items, bool>> itemsCondition { get; set; }


    private CustomObservableCollection<Promotion> promotion_in_order = new();


    private ObservableCollection<OrderSale> OrderSalesInSplitOrder = new();

    public int PromotionBonusSpanCount { get { if (IsModifiersBonusOn) return 1; return 4; } }

    public bool IsClient { get => isClient; set { isClient = value; OnPropertyChanged(nameof(IsClient)); OnPropertyChanged(nameof(IsReceipt)); } }
    public bool IsDelivery { get => isDelivery; set { isDelivery = value; OnPropertyChanged(nameof(IsDelivery)); OnPropertyChanged(nameof(IsDeliveryOrder)); OnPropertyChanged(nameof(IsReceipt)); } }
    //public bool IsTakeaway { get => isTakeaway; set { isTakeaway = value; OnPropertyChanged(nameof(isTakeaway)); OnPropertyChanged(nameof(IsTakeAwayOrder)); OnPropertyChanged(nameof(IsReceipt)); } }
    public bool IsReceipt { get => !IsClient && !IsDelivery; }
    public bool IsItems { get => !IsPromotion; }
    public bool IsOrderPayed
    {
        get => Order?.OrderStatus == OrderStatus.Closed || Order?.OrderDelivery?.DeliveryStatus == DeliveryStatus.OnWay
                                         || Order.OrderStatus == OrderStatus.Deleted
                                        || Order.OrderDelivery?.DeliveryStatus == DeliveryStatus.Delivered || Order.IsOnline;
    }





    // public bool IsPredCheck { get => Order.OrderStatus == OrderStatus.Precheck; }

    public string SearchTextProducts { get { return searchTextProducts; } set { searchTextProducts = value; OnPropertyChanged(nameof(SearchTextProducts)); OnSearchTextChanged(); } }
    public string SearchTextClient { get { return searchTextClients; } set { searchTextClients = value; OnPropertyChanged(nameof(searchTextClients)); OnSearchClientTextChanged(); } }


    public decimal PriceModifications
    {

        // get => Modifiers.Sum(getGoodsPrice);

        get => SelectedModifiers.OfType<Goods>().Sum(x =>
        {
            var count = x.Count <= 0 ? 1 : x.Count;
            return x.Price * count;
        });
    }




    private decimal getGoodsPrice(Modifiers modifiers)
    {
        if (!IsNotNull(modifiers)) return 0;
        else
        {
            if (modifiers.IsOnlyOne)
            {
                if (!IsNotNull(modifiers.SelectedGods)) return 0;
                return ((Goods)modifiers.SelectedGods).Price;
            }
            else
            {
                var totalSum = SelectedModifiers
                                 .OfType<Goods>() // Filter only objects of type Goods
                                 .Sum(modifier =>
                                 {
                                     if (IsNotNull(modifier))
                                         return modifier.Price * modifier.Count;
                                     return 0; // Assuming Count is an integer property, return 0 if modifier is null
                                 });


                return totalSum;
            }
        }
    }

    public void PriceModificationsUpdate() => OnPropertyChanged(nameof(PriceModifications));


    //public bool IsDeliveryOrTakeAway { get => Order.OrderType != OrderType.InTheInstitution && IsDelivery; }
    public bool IsDeliveryOrder { get => Order.Detail?.OrderType == OrderType.Delivery && IsDelivery; }


    public bool IsTakeAwayOrder { get => Order.Detail?.OrderType == OrderType.TakeAway; }



    public string Title { get => Order.Detail?.OrderType switch { OrderType.TakeAway => AppResources.TakeAway, OrderType.Delivery => AppResources.DeliveryLabel, _ => "" }; }


    public OrderDetailPageViewModel()

    {
        try
        {
            realm ??= GetRealm();
            Clients = realm.All<Client>();
            ClientGruops = realm.All<ClientGroup>();
            VisualClients.ReplaceRange(Clients.Where(doc => !doc.IsDeleted));
            DeliveryZones = realm.All<DeliveryZone>();
            AllOrders = realm.All<Orders>().Where(x => (x.Id != Order.Id));

            Menu = realm.All<Menu>().ToList().FirstOrDefault(x => x.SalePoints.FirstOrDefault(y => y.Id == CurrentSalePointId) != null && !x.IsDeleted) ?? realm.All<Menu>().FirstOrDefault(x => !x.IsDeleted);

            if (Menu.Items.Count() <= 0)
            {
                Menu = realm.All<Menu>().ToList().FirstOrDefault(x => x.SalePoints.FirstOrDefault(y => y.Id == CurrentSalePointId) != null && x.Items.Count() > 0 && !x.IsDeleted);
            }
            //if (Menu.IsTime)
            //{
            //    Menu = realm.All<Menu>().ToList().FirstOrDefault(x => x.SalePoints.FirstOrDefault(y => y?.Id == CurrentSalePointId) != null && (x.StartTime>=DateTimeOffset.Now  && x.EndTime<=DateTimeOffset.Now)) ?? realm.All<Menu>().ToList().FirstOrDefault();

            //}
            Promotions ??= new();
            promotion_in_order ??= new();

            Promotions.ItemAdded += Promotions_ItemAdded;
            promotion_in_order.ItemAdded += Promotions_InOrder_ItemAdded;
            //    realm ??= GetRealm();





            ItemsCondition = x => x.IsMain;
            Homepage = HomePageViewModel.getInstance();
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
        }


    }

    [RelayCommand]
    public void Increment(Goods g)
    {


        try
        {
            Modifiers s = new();
            if (IsModifiersBonusOn)
            {
                s = CurrentModifiersInBonusItems.FirstOrDefault(x => x.ModifiersList.Contains(g));

            }
            else
            {
                s = Modifiers.FirstOrDefault(x => x.ModifiersList.Contains(g));

            }

            if (s.MaxNumberOfModifiers <= g.Count && s.MaxNumberOfModifiers > 0)
                return;
            g.Count++;

            if (!SelectedModifiers.Contains(g))
                SelectedModifiers.Add(g);

            // g.RaisePropertyChanged(nameof(g.Count));
            PriceModificationsUpdate();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    [RelayCommand]
    public async Task IncrementDetail(Goods g)
    {
        try
        {
            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}
            await realm.WriteAsync(() =>
            {
                g.Amount++;
                var goodsList = CurrentOrderSale.ModifierGoods.SelectMany(x => x.SelectedGoodsId);

                var orderSale = Order.OrderSales
                .FirstOrDefault(x =>
                x.GuestIndex == CurrentGuest.GuestIndex &&
                CurrentOrderSale.Id != x.Id &&
                x.ModifierGoods.Count != 0 &&
                    x.ModifierGoods
                    .All(modifierGood =>
                        modifierGood.SelectedGoodsId
                        .All(selectedGood =>
                             goodsList
                             .FirstOrDefault(y => y.Id == selectedGood.Id &&
                             y.Amount == selectedGood.Amount) != null

                        )));




                if (IsNotNull(orderSale))
                {
                    Order.OrderSales.Remove(CurrentOrderSale);
                    CurrentOrderSale = null;
                    IsOpenOrderDetail = false;
                    orderSale.Amount += 1d;
                    PriceModificationsUpdate();
                    return;

                }
                CurrentOrderSale.Price += g.Price;
                CurrentOrderSale.Cost += g.CostPrice;
            });



            PriceModificationsUpdate();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    public void Cleare(Goods g)
    {
        try
        {
            Modifiers s = new();
            if (IsModifiersBonusOn)
            {
                s = CurrentModifiersInBonusItems.FirstOrDefault(x => x.ModifiersList.Contains(g));

            }
            else
            {
                s = Modifiers.FirstOrDefault(x => x.ModifiersList.Contains(g));

            }

            g.Count = s.MinNumberOfModifiers;// 0;

            if (g.Count <= 0)
            {
                if (SelectedModifiers.Contains(g))
                    SelectedModifiers.Remove(g);
            }
            // RaisePropertyChanged(nameof(Count));
            PriceModificationsUpdate();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }


    [RelayCommand]
    public async Task ClearDetail(Goods g)
    {
        try
        {
            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}
            await realm.WriteAsync(() =>
            {


                CurrentOrderSale.ModifierGoods.ForEach(x =>
                {
                    if (x.SelectedGoodsId.Contains(g))
                    {
                        CurrentOrderSale.Price -= g.Price * g.Amount;
                        CurrentOrderSale.Cost -= g.CostPrice * g.Amount;

                        //{modif.Name}-{modifier.Name}, 

                        var fl = CurrentOrderSale.PositionDetail;

                        if (fl.Contains($"{x.ModifierId.Name}-{g.Name},"))
                            CurrentOrderSale.PositionDetail = fl.Replace($"{x.ModifierId.Name}-{g.Name},", "");

                        else if (fl.Contains($", {x.ModifierId.Name}-{g.Name}"))
                            CurrentOrderSale.PositionDetail = fl.Replace($", {x.ModifierId.Name}-{g.Name}", "");

                        else if (fl.Contains($"{x.ModifierId.Name}-{g.Name}"))
                            CurrentOrderSale.PositionDetail = fl.Replace($"{x.ModifierId.Name}-{g.Name}", "");


                        var h1 = CurrentOrderSale.PositionDetail;
                        x.SelectedGoodsId.Remove(g);
                        if (x.SelectedGoodsId.Count < 1)
                            CurrentOrderSale.ModifierGoods.Remove(x);

                        if (CurrentOrderSale.ModifierGoods.Count < 1)
                        {
                            Order.OrderSales.Remove(CurrentOrderSale);
                            IsOpenOrderDetail = false;
                            CurrentOrderSale = null;
                        }
                        return;
                    }
                });

                // RaisePropertyChanged(nameof(Count));
                PriceModificationsUpdate();
            });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private void Promotions_ItemAdded(object sender, Promotion promotion)
    {
        try
        {
            if (IsNotNull(promotion))
                PromotionCount = Promotions.Count();


            //Визуальное отображение количество новых акций
            //ListView.DataSource.RefreshFilter();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    [RelayCommand]
    private void OpenOrClosePromotion()
    {
        try
        {
            if (Promotions.Count() < 1)
                return;


            //PromotionCount = 0;

            IsOpenPromotion = !IsOpenPromotion;
            //ListView.DataSource.RefreshFilter();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }


    private async void Promotions_InOrder_ItemAdded(object sender, Promotion promotion)
    {

        try
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

                var product = Order.OrderSales.FirstOrDefault(x => x.Product.Id == condition.ProductId);

                if (!IsNotNull(product)) continue;

                if (condition.ForHowLong != 0 && condition.ForHowLong <= product.TotalPrice)
                {
                    await RecalculateDiscount(product, promotion);
                    break;
                }
                if (condition.IsCategory)
                {

                    if (product.Category?.Id == condition?.CategoryId)
                    {
                        await RecalculateDiscount(product, promotion);

                        break;
                    }
                    continue;
                }

                if (condition.IsEquels)
                {
                    // await RecalculateDiscount(product, promotion);
                    if (product.Amount == condition.Count)
                    {
                        await RecalculateDiscount(product, promotion);
                        break;
                    }
                    continue;
                }
                if (product.Amount >= condition.Count)
                {
                    await RecalculateDiscount(product, promotion);
                    break;
                }







            }

            //ListView.DataSource.RefreshFilter();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    private async Task RecalculateDiscount(OrderSale product, Promotion promotion)
    {
        try
        {
            if (
            IsModifiersBonusOn)

                if (!IsNotNull(product, promotion)) return;
            await realm.WriteAsync(() =>
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

                        if (Order.OrderSales.ToList().Any(x => x.Promotion?.Id == promotion.Id))
                        {
                            return;

                        }
                        var res =
                        promotion.ResultBonus.ResultBonusGoods.FirstOrDefault(x => x.IsAll);
                        if (IsNotNull(res))
                        {
                            Menu.Items.ForEach(x =>
                            {
                                if (!ItemsInPromotion.Contains(x))
                                {
                                    var item = new Items()
                                    {
                                        Name = x.Name,
                                        Id = x.Id,
                                        CreationDate = x.CreationDate,
                                        IsActive = x.IsActive,
                                        Description = x.Description,
                                        IsCategory = x.IsCategory,
                                        IsSelfPrice = x.IsSelfPrice,
                                        Picture = x.Picture,
                                        IsMain = x.IsMain,
                                        Product = x.Product,
                                        IsDiscount = x.IsDiscount,
                                        UserId = x.UserId,
                                    };
                                    item.Price = promotion.ResultBonus.ConditionType switch
                                    {
                                        ConditionType.FixedPriceOnBonusGoods => (decimal)promotion.ResultBonus.ConditionValue,
                                        ConditionType.FixedDiscountAmountOnBonusGoods => x.Price - (decimal)promotion.ResultBonus.ConditionValue,
                                        ConditionType.DiscountPercentOnBonusGoods => x.Price * (decimal)promotion.ResultBonus.ConditionValue / 100,
                                        _ => 0
                                    };

                                    ItemsInPromotion.Add(item);
                                }
                            });
                            //  ItemsInPromotion.ReplaceRange(Menu.Items);
                        }
                        else
                        {
                            ItemsInPromotion.Clear();
                            foreach (var goods in promotion.ResultBonus.ResultBonusGoods)
                            {
                                if (goods.IsCategory)
                                {
                                    var produtcs = realm.All<Product>().ToList().Where(x => x.Category?.Id == goods.CategoryId?.Id).ToList();
                                    var itemsProducts = produtcs.Select(x => new Items() { Price = x.TotalSum, Product = x, Picture = x.Picture, }).ToList();

                                    itemsProducts.ForEach(x =>
                                    {
                                        if (!ItemsInPromotion.Contains(x))
                                        {
                                            var item = new Items()
                                            {
                                                Name = x.Name,
                                                Id = x.Id,
                                                CreationDate = x.CreationDate,
                                                IsActive = x.IsActive,
                                                Description = x.Description,
                                                IsCategory = x.IsCategory,
                                                IsSelfPrice = x.IsSelfPrice,
                                                Picture = x.Picture,
                                                IsMain = x.IsMain,
                                                Product = x.Product,
                                                IsDiscount = x.IsDiscount,
                                                UserId = x.UserId,
                                            };
                                            item.Price = promotion.ResultBonus.ConditionType switch
                                            {
                                                ConditionType.FixedPriceOnBonusGoods => (decimal)promotion.ResultBonus.ConditionValue,
                                                ConditionType.FixedDiscountAmountOnBonusGoods => x.Price - (decimal)promotion.ResultBonus.ConditionValue,
                                                ConditionType.DiscountPercentOnBonusGoods => x.Price * (decimal)promotion.ResultBonus.ConditionValue / 100,
                                            };

                                            ItemsInPromotion.Add(item);

                                        }
                                    });


                                }
                                else
                                {

                                    var item = new Items() { Product = goods.Product, Picture = goods.Product.Picture, Name = goods.Product.Name, };
                                    item.Price = promotion.ResultBonus.ConditionType switch
                                    {
                                        ConditionType.FixedPriceOnBonusGoods => (decimal)promotion.ResultBonus.ConditionValue,
                                        ConditionType.FixedDiscountAmountOnBonusGoods => goods.Product.TotalSum - (decimal)promotion.ResultBonus.ConditionValue,
                                        ConditionType.DiscountPercentOnBonusGoods => goods.Product.TotalSum * (decimal)promotion.ResultBonus.ConditionValue / 100,
                                        _ => 0
                                    };
                                    if (!ItemsInPromotion.Contains(item))
                                    {
                                        ItemsInPromotion.Add(item);
                                    }

                                }

                            }
                        }
                        CurrentBonusPromotion = promotion;

                        IsPromotionBonusOn = true;
                        break;
                    case ResultType.FixedDiscountAmount:
                        break;
                }


                product.Promotion = promotion;

            });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    public void OnAppearing()
    {
        //subscribeOrders = Order.OrderSales
        //     .SubscribeForNotifications(async (sender, changes) =>
        //     {
        //         if (changes == null)
        //             return;

        //         foreach (var i in changes.DeletedIndices)
        //         {
        //            await RecalculationGuest();

        //             return;


        //           }
        //              });
        _ = LoadDependencies();


        //await Task.Delay(150);
        //Homepage.CurrentState = StateKey.OrderDetail;
    }
    public async Task SaveOrderDiscountSum(OrderSale os)
    {
        try
        {
            await realm.WriteAsync(() =>
        {
            os.DiscountSum = (os.Price * (decimal)os.Amount * (decimal)(os.DiscountPercent / 100));
            if (os.DiscountSum < 0.0m)
                os.DiscountSum = 0m;
        });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }
    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        try
        {
            if (query.Count > 0 && query.ContainsKey("Order"))
            {
                Order = query["Order"] as Orders;
            }
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }



    partial void OnSelectedModifiersChanged(ObservableRangeCollection<Goods> value) => OnPropertyChanged(nameof(PriceModifications));
    partial void OnIsModifiersBonusOnChanged(bool value) => OnPropertyChanged(nameof(PromotionBonusSpanCount));



    partial void OnIsPromotionChanged(bool value) => OnPropertyChanged(nameof(IsItems));
    partial void OnPromotionCountChanged(int value) => OnPropertyChanged(nameof(IsPromotionCount));

    [RelayCommand]
    private void SelectedBonusGoods(Items G)
    {
        if (CurrentBonusPromotion == null)
        {
            IsPromotionBonusOn = false;
            return;
        }
        if (G.IsCategory) return;
        var p = CurrentBonusPromotion.ResultBonus.ResultBonusGoods.FirstOrDefault(x => x.Product?.Id == G.Product?.Id);
        if (p?.Modifiers.Count() > 0)
        {
            IsModifiersBonusOn = true;
            currentItemWithModif = G;
            CurrentModifiersInBonusItems.ReplaceRange(p.Modifiers.ToList());

            return;
        }


        var count = ItemsInPromotion.Sum(x => x.Count);

        if (count >= CurrentBonusPromotion.ResultBonus.BonusProductAmount) { return; }
        G.Count++;


    }


    [RelayCommand]
    private async void CloseBonusPromotion()
    {
        ItemsInPromotion.Clear();
        IsPromotionBonusOn = false;
        IsShowAdmin = false;
        await DeletePromotion(Order.OrderSales.FirstOrDefault(x => x.Promotion.Id == CurrentBonusPromotion.Id));

        IsModifiersBonusOn = false;
        CurrentBonusPromotion = null;

    }




    [RelayCommand]
    private async void SaveAddingBonusItem()
    {


        try
        {

            if (IsModifiersBonusOn)
            {


                if (!IsNotNull(currentItemWithModif) || SelectedModifiers.Count() <= 0)
                {
                    throw new Exception("");


                }


                await AddItemToOrderSale(currentItemWithModif, SelectedModifiers.OfType<Goods>().ToList());


                currentItemWithModif = null;

                var os = Order.OrderSales.ToList().FirstOrDefault(x => x.Promotion?.Id == CurrentBonusPromotion.Id && x.Id != currentItemWithModif.Id);

                os.Price = CurrentBonusPromotion.ResultBonus.ConditionType switch
                {
                    ConditionType.FixedPriceOnBonusGoods => (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue,
                    ConditionType.FixedDiscountAmountOnBonusGoods => os.Price - (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue,
                    ConditionType.DiscountPercentOnBonusGoods => os.Price * (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue / 100,
                    _ => 0
                };


            }
            else
            {

                await realm.WriteAsync(() =>
                {


                    if (CurrentBonusPromotion is null)
                    {
                        throw new Exception("");

                    }
                    CurrentGuest ??= Order.OrderSales.FirstOrDefault();

                    var os = Order.OrderSales.ToList().FirstOrDefault(x => x.Promotion?.Id == CurrentBonusPromotion.Id && x.Id != currentItemWithModif.Id);

                    os.Price = CurrentBonusPromotion.ResultBonus.ConditionType switch
                    {
                        ConditionType.FixedPriceOnBonusGoods => (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue,
                        ConditionType.FixedDiscountAmountOnBonusGoods => os.Price - (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue,
                        ConditionType.DiscountPercentOnBonusGoods => os.Price * (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue / 100,
                        _ => 0
                    };

                    ItemsInPromotion.Where(x => x.Count > 0).ForEach(item =>
                    {
                        var os = Order.OrderSales.FirstOrDefault(x => x.Product?.Id == item.Product?.Id);
                        if (IsNotNull(os))
                        {
                            os.Amount += item.Count > 0 ? item.Count : 1d;
                        }
                        else
                        {


                            Order.OrderSales.Add(new OrderSale()
                            {
                                Price = item.Price,
                                GuestIndex = CurrentGuest.GuestIndex,
                                Code = item.Product?.Code,
                                DiscountPercent = 0d,
                                Product = item.Product,
                                Cost = (decimal)item.Product?.CostPrice,
                                DiscountSum = 0m,
                                BarCode = item.Product?.BarCode,
                                Promotion = CurrentBonusPromotion,
                                Sum = 0m,
                                Category = item.Product?.Category,
                                Name = item.Product?.Name,
                                Vat = 0d,
                                IsMainPromotionItem = false,
                                Tax = item.Product?.Tax,
                                Psu = item?.Product?.ForeignSpecialCode,
                                TransferType = TransferType.Card,
                                Section = "",
                                IsTaxable = false,
                                Amount = item.Count > 0 ? item.Count : 1d
                            });
                        }

                    });

                });

            }
        }
        catch (Exception e)
        {

            var i = e;

        }
        finally
        {
            ItemsInPromotion.Clear();
            IsPromotionBonusOn = false;
            var ls = Order.OrderSales
             .GroupBy(x => x.GuestIndex)

             .ToList()
             .Select(x => new GuestGroup(groupIndex: x.Key, orders: x.ToList()));

            GuestGroups.ReplaceRange(ls);
            IsModifiersBonusOn = false;
            CurrentBonusPromotion = null;
            CurrentGuest = CurrentGuest;
        }
    }

    [RelayCommand]
    private void ClearBonusGoods(Items G)
    {
        G.Count = 0;


    }

    partial void OnCurrentClientChanged(Client value)
    {
        try
        {
            if (!IsNotNull(value))
                return;

            CurrentClient = value;

            if (isDiscountClientBonus)
            {
                realm.Write(() =>
               {

                   Order.OrderSales.ToList().ForEach(x =>
                   {
                       x.DiscountPercent = 0d;
                       // x.DiscountSum = client.ClientGroup.BonusOnBirthday;


                   });
                   isDiscountClientBonus = false;

               });
            }

            _ = AddOrderDeliveryFromClient(value);
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }



    partial void OnOrderChanged(Orders value)
    {
        OnPropertyChanged(nameof(Title));
        OnPropertyChanged(nameof(IsTakeAwayOrder));
        OnPropertyChanged(nameof(IsOrderPayed));
        //    OnPropertyChanged(nameof(IsPredCheck));
        OnPropertyChanged(nameof(IsDeliveryOrder));




    }


    [RelayCommand]
    private void AddDeliveryToOrder()
    {
        try
        {
            if (Order is null || Order.Detail == null)
                return;

            Order.Detail.OrderType = OrderType.Delivery;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }


    [RelayCommand]
    private async Task DeletePromotion(OrderSale product)
    {
        if (Homepage.CurrentBrand.OnCancelAction && IsShowAdmin)
        {
            var res = await DialogService.ShowAdminView();
            if (!res)
            {
                IsShowAdmin = true;
                return;
            }
            IsShowAdmin = true;
        }
        IsShowAdmin = true;
        try
        {
            promotion_in_order.Remove(product.Promotion);

            var os = new OrderSale();
            await realm.WriteAsync(() =>
            {
                Order.OrderSales.Where(x => x.Promotion?.Id == product.Promotion?.Id)
   
                 .ForEach( x => 
            {



                switch (x.Promotion.ResultType)
                {

                    case ResultType.DiscountPercent:
                        x.DiscountPercent = 0d;
                        break;
                    case ResultType.FixedPrice:
                        x.DiscountSum = 0m;
                        break;
                    case ResultType.BonusGoods://доп товар
                        x.DiscountSum = 0m;
                        break;
                    case ResultType.FixedDiscountAmount://? добавляется n кол во товаров
                        break;
                }
                x.Promotion = null;

                    if (!x.IsMainPromotionItem)
                    {
                        Order.OrderSales.Remove(x);
                }
                else
                {
                    os = x;
                }

            });


            });

          

          
            var ls = Order.OrderSales
                  .GroupBy(x => x.GuestIndex)

                  .ToList()
                  .Select(x => new GuestGroup(groupIndex: x.Key, orders: x.ToList()));

            GuestGroups.ReplaceRange(ls);


            CurrentGuest = CurrentGuest;
            await CheckPromotion(os);
            //ListView.DataSource.RefreshFilter();
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
        }

    }


    [RelayCommand]
    private async Task OpenCommentOrder(string parm)
    {
        try
        {
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}
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
        catch (Exception ex) { Crashes.TrackError(ex); }
    }




    [RelayCommand]
    private async Task AddGuest()
    {
        try
        {

            if (!IsNotNull(Order))
                return;

            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}

            var count = Order.OrderSales.OrderBy(x => x.GuestIndex).LastOrDefault()?.GuestIndex ?? 0;

            OrderSale os = null;
            await realm.WriteAsync(() =>
            {
                os = new OrderSale
                {
                    GuestIndex = ++count
                };
                Order.OrderSales.Add(os);
            });



            //GuestGroups.Add(new GuestGroup(os.GuestName,new()));


            var ls = Order.OrderSales
                 .GroupBy(x => x.GuestIndex)

                 .ToList()
                 .Select(x => new GuestGroup(groupIndex: x.Key, orders: x.ToList()));

            GuestGroups.ReplaceRange(ls);


            CurrentGuest = CurrentGuest;



        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);

        }

    }

    internal async Task SaveOrderTotalSumm()
    {
        try
        {
            if (!IsNotNull(Order) || !IsNotNull(Order.OrderReceipt))
                return;

            await realm.WriteAsync(() =>
            {

                Order.OrderReceipt.ResultSum = Order.OrderSales.Sum(x => x.TotalPrice);
            }
            );
            //   await Task.Delay(200);
            OnPropertyChanged(nameof(Order));
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private void SelectedGuestOrClient(GuestGroup groupResult)
    {
        try
        {


            CurrentGuest = groupResult.FirstOrDefault();

            //   OnPropertyChanged(nameof(GuestGroup))

            ////TODO : Переделать на нормальный выбор гостя сейчас проскакивает гость на другом языке, по этому гость не выбирается
            //var guest = Order.OrderSales.FirstOrDefault(x => x.GuestName.Equals(groupResult.Key));
            //CurrentGuest ??= Order.OrderSales.FirstOrDefault();
            //if (IsNotNull(CurrentGuest) && Order.OrderSales.Count > 0)
            //    CurrentGuest.IsActive = false;
            //CurrentGuest = guest;
            //if (CurrentGuest == null)
            //{
            //    DialogService.ShowToast($"{groupResult.Key} {AppResources.NotFound}");
            //    return;
            //}
            //CurrentGuest.IsActive = true;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }
    [RelayCommand]
    private async Task GoToPayPage()
    {
        try
        {
            if (Order.OrderStatus is OrderStatus.Closed || Order.OrderStatus == OrderStatus.Deleted)
                return;


            Homepage.CurrentState = StateKey.PayPage;
            PayPageViewModel payPage = HomePageViewModel.getInstancePayPageInstance();


            payPage.Order = Order;


            payPage.PaymentChange();
            await Task.Delay(150);
            payPage.IsPaymentVisible = true;

        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }



    /// <summary>
    /// Возможно перенести в popup
    /// </summary>
    ///

    [RelayCommand]
    private void AddNewAddresNewClien()
    {
        try
        {
            if (!IsNotNull(NewClient) || !IsNotNull(ClientAddress.LastOrDefault()?.Address))
                return;



            ClientAddress.Add(new());
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private void AddNewCardNewClien()
    {
        try
        {

            if (!IsNotNull(NewClient) || !IsNotNull(ClientCards.LastOrDefault()?.CardNumber))
                return;

            ClientCards.Add(new());
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private async Task AddNewClient(string parm)
    {
        try
        {
            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}
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
                else
                {
                    ClientAddress = new();
                    ClientCards = new();
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
                    VisualClients.ReplaceRange(Clients.Where(doc => !doc.IsDeleted));
                    if (Order.Detail?.OrderType is OrderType.Delivery)
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
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    [RelayCommand]
    private async Task AddClientToOrder(object sender)
    {
        try
        {
            try
            {
                var client = sender as Client;
                if (!IsNotNull(client, Order))
                    return;


                await realm.WriteAsync(() =>
                {
                    Order.Client = client;
                });
            }
            catch (Exception ex) { Crashes.TrackError(ex); }

        }
        catch (Exception ex) { Crashes.TrackError(ex); }

        // IsClient = false;
    }

    [RelayCommand]
    public async void SelectedOrderType(string parm)
    {
        try
        {
            if (Homepage.CurrentBrand.OnAddClient && parm == nameof(IsClient))
            {
                var res = await DialogService.ShowAdminView();
                if (!res)
                {
                    IsClient = false;
                    return;
                }
                else
                {
                    IsClient = true;
                }
            }
            IsDelivery = parm == nameof(IsDelivery);
            IsTakeaway = (IsDelivery && Order.Detail?.OrderType == OrderType.TakeAway);
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private async void SelectOrder(Orders order)
    {
        try
        {
            if (!IsNotNull(order))
                return;
            IsOrderListVisible = false;

            _ = SaveOrderTotalSumm();
            Order = order;
            _ = LoadDependencies();









            _ = getOrders();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private void GoToPreviousCategory()
    {
        try
        {
            if (CategoryTitle.Equals(AppResources.AllItems))
                return;

            if (CategoryTitle.Equals("..."))
            {

                goto FillAllItems;

            }



            if (IsNotNull(parentItemId))
            {
                var currentItem = Menu?.Items.FirstOrDefault(x => x?.Id == parentItemId);

                if (!IsNotNull(currentItem)) goto FillAllItems;

                CategoryTitle = currentItem.Name;
                //       VisualItems.ReplaceRange(Menu?.Items.Where(x => x?.ParentItemId == currentItem.Id));
                // VisualItems = Menu?.Items.Where(x => x?.ParentItemId == currentItem.Id);

                ItemsCondition = x => x.ParentItemId == currentItem.Id;
                parentItemId = currentItem.ParentItemId;
                return;
            }
        FillAllItems:
            CategoryTitle = AppResources.AllItems;
            /// VisualItems.ReplaceRange(Menu?.Items.Where(x => x?.IsMain == true));
            // VisualItems =(Menu?.Items.Where(x => x?.IsMain == true));
            ItemsCondition = x => x.IsMain;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    [RelayCommand]
    private async Task MinusProduct(OrderSale orderProduct)
    {
        await Task.Delay(100);
        try
        {
            if (!IsNotNull(orderProduct, CurrentGuest))
                return;

            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }

            if (Homepage.CurrentBrand.OnProductDelete)
            {

                var res = await DialogService.ShowAdminView();
                if (!res)
                {
                    return;
                }
            }
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}
            if (IsSplitOrder)
            {
                OperationToSplitOrder(os: orderProduct);
                return;
            }
            if (orderProduct.Amount <= 1)
            {
                //if (Order.OrderSales.Count <= 1)
                //    return;

                //  GuestGroups.FirstOrDefault(x => x.GroupTitle == orderProduct.GuestName)?.Remove(orderProduct);
                await realm.WriteAsync(() =>
                {
                    _ = SaveLog($"{Resources.AppResources.ProductWasRemoved} - {orderProduct.Name}");
                    var guestIndex = orderProduct.GuestIndex;
                    Order.OrderSales.Remove(orderProduct);



                    if (Order.OrderSales.Count(x => x.GuestIndex == guestIndex) == 0)
                    {
                        var tempGuest = new OrderSale { GuestIndex = guestIndex };
                        Order.OrderSales.Add(tempGuest);

                        // GuestGroups.Add(new GuestGroup(tempGuest.Name, new()));
                        CurrentGuest = tempGuest;
                    }

                });
                var ls = Order.OrderSales
                    .GroupBy(x => x.GuestIndex)

                    .ToList()
                    .Select(x => new GuestGroup(groupIndex: x.Key, orders: x.ToList()));

                GuestGroups.ReplaceRange(ls);
                CurrentGuest = CurrentGuest;
                _ = SaveOrderTotalSumm();
                //ListView.DataSource.RefreshFilter();

                return;
            }
            await realm.WriteAsync(() =>
            {

                orderProduct.Amount--;

            });
            //ListView.DataSource.RefreshFilter();

            _ = SaveLog($"{Resources.AppResources.TookAwayQuantityProduct} - {orderProduct.Name}");

        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private async Task PlusProduct(OrderSale orderProduct)
    {
        await Task.Delay(100);

        try
        {
            if (!IsNotNull(orderProduct, CurrentGuest))
                return;
            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}

            if (IsSplitOrder)
            {
                OperationToSplitOrder(os: orderProduct, isPlus: true);
                return;
            }


            await realm.WriteAsync(async () =>
            {
                try
                {

                    if (!IsNotNull(orderProduct))
                        return;

                    orderProduct.Amount++;
                    _ = CheckPromotion(orderProduct);

                    _ = SaveLog($"{Resources.AppResources.AddedProductQuantity} - {orderProduct.Name}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"{Resources.AppResources.ErrorAddingProduct} : {ex.Message}");
                }



            });
            //ListView.DataSource.RefreshFilter();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }



    private void OperationToSplitOrder(OrderSale os, bool isPlus = false)
    {
        try
        {
            if (isPlus)
            {
                if (os.Count >= os.Amount - 1)
                {
                    os.Count = (int)os.Amount;

                    return;

                }
                os.Count++;

                return;
            }

            if (os.Count <= 1)
            {
                os.Count = 1;

                return;
            }
            os.Count--;

        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }


    private void AddInSplitOrderSale(OrderSale os)
    {
        try
        {
            if (os is null) return;
            if (OrderSalesInSplitOrder.Contains(os))
            {
                OrderSalesInSplitOrder.Remove(os);
            }
            OrderSalesInSplitOrder.Add(os);
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }






    private async Task CheckPromotion(OrderSale product)
    {
        try
        {

            if (!IsNotNull(product, Order)) return;

            realm ??= GetRealm();
            if (realm.IsClosed) realm = GetRealm();


            var promotions = realm.All<Promotion>().ToList().Where(x => x.PromotionConditions.ToList().Any(y => y.ProductId == product.Product?.Id||y.CategoryId==product.Category?.Id) && !x.IsDeleted).AsQueryable();
            if (promotions.Count() < 1)
                return;
            var total_promotions = ContainsPromotionConditions(product, promotions);
            if (total_promotions.Count() < 1)
                return;

            total_promotions.Where(x => Promotions.ToList().Any(y => y.Id != x.Id) || Promotions.Count() < 1).ToList().ForEach(Promotions.Add);
        }
        catch (Exception ex)
        {


            Crashes.TrackError(ex);
        }
    }

    private bool CheckPromotionOnMinus(OrderSale product)
    {
        try
        {

            if (!IsNotNull(product, Order)) return false;

            realm ??= GetRealm();
            if (realm.IsClosed) realm = GetRealm();


            var promotions = realm.All<Promotion>().ToList().Where(x => x.PromotionConditions.ToList().Any(y => y.ProductId == product.Product.Id || y.CategoryId == product.Category.Id) && !x.IsDeleted).AsQueryable();
            if (promotions.Count() < 1)
                return false;
            var total_promotions = ContainsPromotionConditions(product, promotions);
            if (total_promotions.Count() < 1)
                return false;

            return true;
        }
        catch (Exception ex)
        {


            Crashes.TrackError(ex);
            return false;
        }
    }

    private List<Promotion> ContainsPromotionConditions(OrderSale product, IQueryable<Promotion> promotions)
    {
        var totaly = new List<Promotion>();
        try
        {


            foreach (var promotion in promotions)//чтобы какое ни будь условие совпадало с акцией
            {
                if (promotion_in_order.Contains(promotion)) continue;
                foreach (var promotionCondition in promotion.PromotionConditions)
                {
                    if (promotionCondition.ProductId != product.Product.Id && promotionCondition.CategoryId != product.Category.Id || promotionCondition.IsDeleted) continue;

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
                        if (product.Amount == promotionCondition.Count)
                        {
                            totaly.Add(promotion);
                            break;
                        }
                    }
                    else
                    {
                        
                        if (product.Amount >= promotionCondition.Count)
                        {
                            totaly.Add(promotion);
                            break;
                        }
                    }
                }
            }

        }
        catch (Exception ex)
        {

            Crashes.TrackError(ex);


        }
        return totaly;
    }

    [RelayCommand]
    private async Task SelectedPromotion(Promotion promotion)
    {
        try
        {
            if (!IsNotNull(promotion) || Promotions.Count < 1) return;
            if (!Promotions.Any(x => x.Id == promotion.Id)) return;
            Promotions.Clear();
            PromotionCount = 0;
            IsOpenPromotion = false;
            promotion_in_order.Add(promotion);
            //ListView.DataSource.RefreshFilter();

            await Task.CompletedTask;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }
    private Expression<Func<Modifiers, bool>> condition;

    private bool FiltersM(object obj)
    {
        //order.OrderDelivery.DeliveryStatus = DeliveryStatus.Success;
        try
        {
            var item = obj as Modifiers;




            bool result = condition.Compile()(item);
            return result;
        }
        catch (Exception ex) { Crashes.TrackError(ex); return false; }

    }


    private bool FilterOrder(object obj)
    {
        //order.OrderDelivery.DeliveryStatus = DeliveryStatus.Success;
        try
        {
            var item = obj as Orders;




            bool result = Order.OrderReceipt?.Table?.Id == item.OrderReceipt?.Table?.Id && item.OrderReceipt?.Table?.Id != null && item.OrderStatus != OrderStatus.Closed && item.Detail.OrderType == OrderType.InTheInstitution;
            return result;
        }
        catch (Exception ex) { Crashes.TrackError(ex); return false; }

    }

    [RelayCommand]
    private async Task SelectProductOrCategory(Items item)
    {
        try
        {
            await Task.Delay(100);
            if (!IsNotNull(item))
                return;

            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}
            realm ??= GetRealm();
            /*  using (var transaction = realm.BeginWrite())
              {
      */
            if (item.IsCategory)
            {
                //  VisualItems.ReplaceRange(Menu?.Items.Where(x => x?.ParentItemId == item.Id));
                // VisualItems = (Menu?.Items.Where(x => x?.ParentItemId == item.Id));
                ItemsCondition = x => x.ParentItemId == item.Id;
                CategoryTitle = item.Name;
                parentItemId = item.ParentItemId;

                return;
            }

            CurrentGuest ??= Order.OrderSales.FirstOrDefault();


            if (!IsNotNull(CurrentGuest))
                return;




            if (item.Product.Modifiers.Count() > 0 && item.Product.IsModify)
            {
                //.Clear();




                var tempModifiers = item.Product.Modifiers.ToList();

                currentItemWithModif = item;

                ModifiersListView.DataSource.BeginInit();
                Modifiers.ReplaceRange(tempModifiers);
                ModifiersListView.DataSource.EndInit();
                OpenOrCloseModifiersList();
                Modifiers.ForEach(x =>
                {
                    if ((x.IsOnlyOne))
                    {
                        x.SelectedGods = x.ModifiersList.FirstOrDefault();
                    }

                    else if (x.MinNumberOfModifiers > 0)
                    {
                        x.ModifiersList.ForEach(y => y.Count = x.MinNumberOfModifiers);
                    }
                });



                return;
            }




            var orderProduct = Order.OrderSales.FirstOrDefault(x => x.Product?.Id == item.Product.Id && x.GuestIndex == CurrentGuest.GuestIndex);

            if (IsNotNull(orderProduct))
            {
                _ = PlusProduct(orderProduct);
                //ListView.DataSource.RefreshFilter();
                return;
            }



            await AddItemToOrderSale(item);
            _ = CheckPromotion(orderProduct);



            //if (ListView.DataSource != null)
            //{
            //    //ListView.DataSource.RefreshFilter();
            //}
        }

        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private async Task AddItemToOrderSale(Items item)
    {

        try
        {

            if (item is null)
                return;
            if (IsOrderPayed)
            {

                return;
            }
            CurrentGuest ??= Order.OrderSales.FirstOrDefault();
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}
            await realm.WriteAsync(async () =>
            {

                var product = Order.OrderSales.FirstOrDefault(x => x.GuestIndex == CurrentGuest.GuestIndex);



                if (!IsNotNull(product?.Product) && Order.OrderSales.Count(x => x.GuestIndex == CurrentGuest.GuestIndex) == 1

                    )
                {

                    var guest = Order.OrderSales.FirstOrDefault(x => x.GuestIndex == CurrentGuest.GuestIndex);
                    guest.Price = item.IsSelfPrice ? item.Product.TotalSum : item.Price;
                    guest.GuestIndex = CurrentGuest.GuestIndex;
                    guest.Code = item.Product?.Code;
                    guest.DiscountPercent = 0;
                    guest.Product = item.Product;
                    guest.DiscountSum = 0;
                    guest.BarCode = item.Product?.BarCode;
                    guest.Cost = (decimal)item.Product?.CostPrice;
                    guest.Amount = 1d;


                    guest.Sum = 0m;
                    guest.Category = item.Product?.Category;
                    guest.Product = item?.Product;
                    guest.Name = $"{item.Name}";
                    guest.Vat = 0;
                    guest.Tax = item?.Product?.Tax;
                    guest.Psu = item?.Product?.ForeignSpecialCode;
                    guest.TransferType = TransferType.Card;
                    guest.Section = "";
                    guest.IsTaxable = false;

                    //GuestGroups.FirstOrDefault(x => x.GroupTitle == CurrentGuest.GuestName)?.Add(guest);
                    // guest.ModifierGoods.Add(modifier);

                    _ = SaveOrderTotalSumm();
                    //
                    var l = Order.OrderSales
                   .GroupBy(x => x.GuestIndex)

                   .ToList()
                   .Select(x => new GuestGroup(groupIndex: x.Key, orders: x.ToList()));

                    GuestGroups.ReplaceRange(l);
                    CurrentGuest = CurrentGuest;
                    CurrentGuest = CurrentGuest;
                    _ = SaveOrderTotalSumm();
                    return;
                }


                var os = new OrderSale
                {
                    Price = item.IsSelfPrice ? item.Product.TotalSum : item.Price,
                    GuestIndex = CurrentGuest.GuestIndex,
                    Code = item.Product?.Code,
                    DiscountPercent = 0d,
                    Product = item.Product,
                    Cost = (decimal)item.Product?.CostPrice,
                    DiscountSum = 0m,
                    BarCode = item.Product?.BarCode,

                    Sum = 0m,
                    Category = item.Product?.Category,
                    Name = $"{item.Name}",
                    Vat = 0d,
                    Tax = item.Product?.Tax,
                    Psu = item?.Product?.ForeignSpecialCode,
                    TransferType = TransferType.Card,
                    Section = "",
                    IsTaxable = false,
                };

                os.Amount = 1d;

                //  GuestGroups.FirstOrDefault(x => x.GroupTitle == CurrentGuest.GuestName)?.Add(os);
                Order.OrderSales.Add(os);
                // GuestGroups.FirstOrDefault(x => x.GroupTitle == CurrentGuest.GuestName)?.Add(os);
                _ = SaveLog($"{Resources.AppResources.AddProduct} - {product.Name}");
                var ls = Order.OrderSales
                     .GroupBy(x => x.GuestIndex)

                     .ToList()
                     .Select(x => new GuestGroup(groupIndex: x.Key, orders: x.ToList()));

                GuestGroups.ReplaceRange(ls);
                CurrentGuest = CurrentGuest;
                _ = SaveOrderTotalSumm();
            });
            //ListView.DataSource.RefreshFilter();
        }
        catch (Exception ex)
        {

            Crashes.TrackError(ex);
        }
        await Task.Delay(50);
        //ListView.DataSource.RefreshFilter();



    }

    private async Task AddItemToOrderSale(Items item, List<Goods> goodsList)
    {
        await Task.Delay(100);

        try
        {
            CurrentGuest ??= Order.OrderSales.FirstOrDefault();

            if (!IsNotNull(CurrentGuest)) return;
            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }

            await realm.WriteAsync(() =>
            {

                // var goodsId = goodsList.Select(x => x.Id);

                var orderSale = Order.OrderSales
                .FirstOrDefault(x =>
                x.GuestIndex == CurrentGuest.GuestIndex &&
                x.ModifierGoods.Count != 0 &&
                    x.ModifierGoods
                    .All(modifierGood =>
                        modifierGood.SelectedGoodsId
                        .All(selectedGood =>
                             goodsList
                             .FirstOrDefault(y => y.Id == selectedGood.Id &&
                             y.Count == selectedGood.Amount) != null

                        )));




                if (IsNotNull(orderSale))
                {

                    orderSale.Amount += 1d;
                    return;

                }


                var product = Order.OrderSales.FirstOrDefault(x => x.GuestIndex == CurrentGuest.GuestIndex);
                if (!IsNotNull(product?.Product?.Id) && Order.OrderSales.Count(x => x.GuestIndex == CurrentGuest.GuestIndex) == 1

                    )
                {
                    var guest = product;
                    guest.Code = item.Product?.Code;
                    guest.Product = item.Product;
                    guest.Price = item.IsSelfPrice ? item.Product.TotalSum : item.Price;
                    guest.BarCode = item.Product?.BarCode;
                    guest.Cost = item.Product?.CostPrice ?? 0m;

                    guest.Sum = 0m;
                    guest.Category = item.Product?.Category;
                    guest.Product = item.Product;
                    guest.Vat = 0;
                    guest.Tax = item.Product?.Tax;
                    guest.Psu = item?.Product?.ForeignSpecialCode;
                    guest.TransferType = TransferType.Card;
                    guest.Section = "";
                    guest.IsTaxable = false;
                    guest.Name = $"{item.Name}";
                    if (IsModifiersBonusOn)
                    {
                        guest.Price = CurrentBonusPromotion.ResultBonus.ConditionType switch
                        {
                            ConditionType.FixedPriceOnBonusGoods => (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue,
                            ConditionType.FixedDiscountAmountOnBonusGoods => item.Product.TotalSum - (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue,
                            ConditionType.DiscountPercentOnBonusGoods => item.Product.TotalSum * (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue / 100,
                            _ => 0
                        };
                        guest.IsMainPromotionItem = false;
                        guest.Promotion = CurrentBonusPromotion;
                    }
                    goodsList.ForEach(modifier =>
                    {



                        guest.Price += modifier.Price * (modifier.Count == 0 ? 1 : modifier.Count);
                        guest.GuestIndex = CurrentGuest.GuestIndex;

                        guest.DiscountPercent = 0;

                        guest.DiscountSum = 0;

                        guest.Cost += modifier.CostPrice * (modifier.Count == 0 ? 1 : modifier.Count);

                        guest.Amount = 1d;

                        var modif = Modifiers.FirstOrDefault(x => x.ModifiersList.Contains(modifier));
                        if (modif is not null)
                        {
                            var smodif = guest.ModifierGoods.FirstOrDefault(x => x.ModifierId == modif);
                            var goods = new Goods { CanBeWrittenOff = modifier.CanBeWrittenOff, CostPrice = modifier.CostPrice, Amount = (modifier.Count == 0 ? 1 : modifier.Count), CreationDate = modifier.CreationDate, Gros = modifier.Gros, Id = modifier.Id, IsDeleted = modifier.IsDeleted, ModifyDate = modifier.ModifyDate, Name = modifier.Name, Picture = modifier.Picture, Price = modifier.Price, Product = modifier.Product, SortIndex = modifier.SortIndex, UserId = modifier.UserId };
                            if (!IsNotNull(smodif))
                            {
                                smodif = new SelectedModifiers() { ModifierId = modif };
                                smodif.SelectedGoodsId.Add(goods);
                                guest.ModifierGoods.Add(smodif);
                            }
                            else
                                smodif.SelectedGoodsId.Add(goods);
                        }


                        guest.PositionDetail += $"{modif.Name}-{modifier.Name}, ";

                    });

                    guest.PositionDetail = guest.PositionDetail.Remove(guest.PositionDetail.Length - 2, 2);
                    _ = SaveOrderTotalSumm();
                    return;
                }


                var os = new OrderSale
                {
                    Price = item.IsSelfPrice ? item.Product.TotalSum : item.Price,
                    GuestIndex = CurrentGuest.GuestIndex,
                    Code = item.Product?.Code,
                    DiscountPercent = 0d,
                    Product = item.Product,
                    Cost = item.Product?.CostPrice ?? 0m,
                    DiscountSum = 0m,
                    BarCode = item.Product?.BarCode,
                    Amount = 1d,
                    Sum = 0m,
                    Category = item.Product?.Category,
                    Name = $"{item.Name}",
                    Vat = 0d,
                    Tax = item.Product?.Tax,
                    Psu = item?.Product?.ForeignSpecialCode,
                    TransferType = TransferType.Card,
                    Section = "",
                    IsTaxable = false,
                };

                if (IsModifiersBonusOn)
                {
                    os.Price = CurrentBonusPromotion.ResultBonus.ConditionType switch
                    {
                        ConditionType.FixedPriceOnBonusGoods => (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue,
                        ConditionType.FixedDiscountAmountOnBonusGoods => item.Product.TotalSum - (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue,
                        ConditionType.DiscountPercentOnBonusGoods => item.Product.TotalSum * (decimal)CurrentBonusPromotion.ResultBonus.ConditionValue / 100,
                        _ => 0
                    };
                    os.Promotion = CurrentBonusPromotion;
                    os.IsMainPromotionItem = false;

                }



                goodsList.ForEach(modifier =>
                {
                    os.Price += modifier.Price * (modifier.Count == 0 ? 1 : modifier.Count);
                    os.Cost += modifier.CostPrice * (modifier.Count == 0 ? 1 : modifier.Count);


                    Modifiers modif = Modifiers.FirstOrDefault(x => x.ModifiersList.Contains(modifier));
                    modif ??= CurrentModifiersInBonusItems.FirstOrDefault(x => x.ModifiersList.Contains(modifier));


                    os.PositionDetail += $"{modif.Name}-{modifier.Name}, ";
                    if (modif is not null)
                    {
                        var smodif = os.ModifierGoods.FirstOrDefault(x => x.ModifierId == modif);

                        var goods = new Goods { CanBeWrittenOff = modifier.CanBeWrittenOff, CostPrice = modifier.CostPrice, Amount = (modifier.Count == 0 ? 1 : modifier.Count), CreationDate = modifier.CreationDate, Gros = modifier.Gros, Id = modifier.Id, IsDeleted = modifier.IsDeleted, ModifyDate = modifier.ModifyDate, Name = modifier.Name, Picture = modifier.Picture, Price = modifier.Price, Product = modifier.Product, SortIndex = modifier.SortIndex, UserId = modifier.UserId };
                        if (!IsNotNull(smodif))
                        {
                            smodif = new SelectedModifiers() { ModifierId = modif };
                            smodif.SelectedGoodsId.Add(goods);
                            os.ModifierGoods.Add(smodif);
                        }
                        else
                            smodif.SelectedGoodsId.Add(goods);


                    }


                });

                os.PositionDetail = os.PositionDetail.Remove(os.PositionDetail.Length - 2, 2);
                Order.OrderSales.Add(os);

            });
            var ls = Order.OrderSales
                 .GroupBy(x => x.GuestIndex)

                 .ToList()
                 .Select(x => new GuestGroup(groupIndex: x.Key, orders: x.ToList()));

            GuestGroups.ReplaceRange(ls);
            CurrentGuest = CurrentGuest;
            //ListView.DataSource.RefreshFilter();
            _ = SaveOrderTotalSumm();
        }
        catch (Exception ex)
        {


            Crashes.TrackError(ex);
        }
    }

    [RelayCommand]
    private async Task SelectItemCommand(Goods g) { }
    [RelayCommand]
    private async Task OpenOrderDetail(object ob)
    {
        //if (ob.GetType() != typeof(OrderSale))
        //{ return; }
        try
        {
            await realm.WriteAsync(() =>
            {



                CommentText = CurrentOrderSale.Comment;



                IsOpenOrderDetail = true;
                CurrentOrderSale.ModifierGoods.ForEach(x =>
                {
                    if (x.ModifierId?.IsOnlyOne == true)
                        x.ModifierId.SelectedGods = x.ModifierId.ModifiersList.FirstOrDefault();
                });
                //ListView.DataSource.RefreshFilter();

            });

        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private void CloseOrderDetail()
    {
        IsOpenOrderDetail = false;
        CurrentOrderSale = null;
    }

    [RelayCommand]
    private async Task SelectedModifications()
    {
        try
        {

            var s = Modifiers;

            if (!IsNotNull(currentItemWithModif) || SelectedModifiers.Count() <= 0)
            {
                OpenOrCloseModifiersList();
                return;
            }

            /*  SelectedModifiers.ToList().ForEach(async x =>
              {
                  var modifier = x as Goods;
                  if(IsNotNull(modifier))
                      await AddItemToOrderSale(currentItemWithModif, modifier); 
              });*/

            await AddItemToOrderSale(currentItemWithModif, SelectedModifiers.OfType<Goods>().ToList());


            currentItemWithModif = null;
            OpenOrCloseModifiersList();
            await Task.Delay(50);
            //ListView.DataSource.RefreshFilter();

            await Task.CompletedTask;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    async void ClosePromotionItem()
    {
        IsPromotionBonusOn = false;
    }


    [RelayCommand]
    async void OpenOrCloseModifiersList()
    {
        try
        {
            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}
            SelectedModifiers.Clear();


            /*     if(!IsModificationOn)
                     Modifiers.Clear();*/

            //  //ListView.DataSource.RefreshFilter();

            //await Task.Delay(150);
            IsModificationOn = !IsModificationOn;
            IsMoDifReady = !IsMoDifReady;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    [RelayCommand]
    private async Task SplitOrder()
    {
        try
        {

            //   var results = OrderSalesInSplitOrder
            //.Select(x => x.CountOrder)
            //.GroupJoin(Order.OrderSales, count => count, orderSale => orderSale.CountOrder, (count, orderSaleList) => new { count, orderSaleList })
            //.Where(x => x.count != 1)
            //.ToList();

            if (OrderSalesInSplitOrder.Count() >= 1)
            {




                var minNumber = OrderSalesInSplitOrder.Min(x => x.CountOrder);

                if (OrderSalesInSplitOrder.Count() == 1)
                    minNumber = -1;


                OrderSalesInSplitOrder
                    .Where(x => x.CountOrder != minNumber)
                    .GroupBy(x => x.CountOrder)
                    .ForEach(async x => await SplitOrderCl(x));

            }
            //foreach (var result in results)
            //{
            //  await  SplitOrderCl(result.orderSaleList);
            //}


            //Select(x => SplitOrderCl(x.orderSaleList));



            _ = OpenExtraFunctions("split");

        }
        catch (Exception ex) { Crashes.TrackError(ex); }


    }

    [RelayCommand]
    private void SplitPlusOrder(OrderSale os)
    {
        try
        {

            if (os is null) return;
            if (os.CountOrder >= 6)
            {
                os.CountOrder = 1;
                return;
            }
            os.CountOrder++;


            AddInSplitOrderSale(os);
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }



    [RelayCommand]
    private async Task OpenExtraFunctions(string parm)
    {
        try
        {
            if (IsOrderPayed)
            {

                return;
            }
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}
            IsExtraOn = !IsExtraOn;

            switch (parm)
            {
                case "Border":
                    return;
                case "cancelOrder":
                    _ = CancelOrder();


                    break;
                case "cancelPredCheck":
                    realm.Write(() =>
                    {
                        Order.OrderStatus = OrderStatus.New;
                    });


                    break;

                case "split":

                    if (Homepage.CurrentBrand.OnReceiptSeparation)
                    {
                        var res = await DialogService.ShowAdminView();
                        if (!res)
                        {
                            IsExtraOn = false;
                            return;
                        }
                    }
                    OrderSalesInSplitOrder ??= new();
                    OrderSalesInSplitOrder.Clear();

                    await Task.Delay(50);
                    IsSplitOrder = !IsSplitOrder;
                    break;
            }


            if (IsExtraOn)
                IsExtraOn = false;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
        //// //ListView.DataSource.RefreshFilter();

    }

    [RelayCommand]
    private async Task CancelOrder()
    {


        try
        {
            if (!IsNotNull(Order))
                return;
            IsBusy = true;
            _ = SaveLog($"{AppResources.Order}  {AppResources.WasCancelled}");

            await realm.WriteAsync(() =>
            {
                Order.OrderStatus = OrderStatus.Deleted;
                Order.OrderSales.Clear();
            });
            Back();
            IsBusy = false;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private async Task DeleteGuest(GuestGroup groupResult)
    {
        try
        {
            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }
            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}
            if (!IsNotNull(Order, groupResult))
                return;
            var guests = Order.OrderSales.Where(x => x.GuestIndex.Equals(groupResult.GroupIndex)).ToList();


            //var guests = Order.OrderSales.Where(x => x.GuestName.Equals(groupInfo.GroupValueText)).ToList();

            //if (guests.Count <= 0 || guests.Count == Order.OrderSales.Count())
            //    return;

            _ = SaveLog($"{AppResources.WasGuestDelete} {AppResources.Guest} {guests[0]?.GuestIndex}");

            await realm.WriteAsync(() =>
            {
                guests.ForEach(x => Order.OrderSales.Remove(x));


            });
            //   GuestGroups.Remove(groupResult);
            _ = SaveOrderTotalSumm();

            //ListView.DataSource.RefreshFilter();
            //  _ = RecalGuest();

            var ls = Order.OrderSales
                          .GroupBy(x => x.GuestIndex)

                          .ToList()
                          .Select(x => new GuestGroup(groupIndex: x.Key, orders: x.ToList()));

            GuestGroups.ReplaceRange(ls);
            CurrentGuest = Order.OrderSales.FirstOrDefault();

        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private async Task SplitOrderCl(IEnumerable<OrderSale> sales, string? clientId = null) // Do it right
    {
        try
        {

            using (var transaction = realm.BeginWrite())
            {
                try
                {

                    var splitOrder = new Orders();

                    splitOrder.BrandId = CurrentBrandId;
                    splitOrder.Employer = realm.All<Employer>().FirstOrDefault(doc => doc.Id == CurrentEmpId);
                    splitOrder.SalePointId = CurrentSalePointId;
                    splitOrder.ShiftId = CurrentCashShiftId;
                    splitOrder.Detail = Order.Detail;
                    Order.Detail?.SalesPointsId.ForEach(splitOrder.Detail.SalesPointsId.Add);
                    splitOrder.OrderStatus = OrderStatus.New;

                    splitOrder.OrderReceipt = new OrderReceipt
                    {

                        CashierId = CurrentEmpId,
                        CreationDate = DateTime.Now,
                        ModifyDate = DateTime.Now
                    };
                    splitOrder.OrderReceipt.Table = new Table()
                    {
                        IsDeleted = Order.OrderReceipt.Table.IsDeleted,
                        Id = Order.OrderReceipt.Table.Id,
                        BorderRadius = Order.OrderReceipt.Table.BorderRadius,
                        Color = Order.OrderReceipt.Table.Color,
                        Height = Order.OrderReceipt.Table.Height,
                        Name = Order.OrderReceipt.Table.Name,
                        Picture = Order.OrderReceipt.Table.Picture,
                        PosX = Order.OrderReceipt.Table.PosX,
                        PosY = Order.OrderReceipt.Table.PosY,
                        Zindex = Order.OrderReceipt.Table.Zindex,
                        Width = Order.OrderReceipt.Table.Width,
                        Seats = Order.OrderReceipt.Table.Seats,

                    };


                    splitOrder.Client = clientId == null ? null : realm.All<Client>().FirstOrDefault(doc => doc.Id == clientId);

                    foreach (var sale in sales)
                    {
                        var promotions = realm.All<Promotion>().ToList().Where(x => x.PromotionConditions.ToList().Any(y => y.ProductId == sale.Product.Id) && !x.IsDeleted).AsQueryable();
                        if (promotions.Count() > 0)
                            return;
                        var total_promotions = ContainsPromotionConditions(sale, promotions);
                        if (total_promotions.Count() > 0)
                            return;

                        sale.DiscountPercent = splitOrder.Client?.ClientGroup?.Value ?? 0d;

                        sale.DiscountSum = (sale.Price * (decimal)sale.Amount * (decimal)(sale.DiscountPercent / 100));
                        if (sale.DiscountSum < 0.0m)
                            sale.DiscountSum = 0m;
                        sale.Sum = sale.Price * (decimal)sale.Amount - sale.DiscountSum;


                        var orderSaleSplit = new OrderSale()
                        {
                            Amount = sale.Count,
                            Cost = sale.Cost,
                            Category = sale.Category,
                            BarCode = sale.BarCode,
                            DiscountSum = sale.DiscountSum,
                            Product = sale.Product,
                            Comment = sale.Comment,
                            Code = sale.Code,
                            GuestIndex = sale.GuestIndex,
                            DiscountPercent = sale.DiscountPercent,
                            IsActive = sale.IsActive,
                            Name = sale.Name,
                            Vat = sale.Vat,
                            TransferType = sale.TransferType,
                            TaxPercent = sale.TaxPercent,
                            Tax = sale.Tax,
                            Sum = sale.Sum,
                            Increase = sale.Increase,
                            IncreaseSum = sale.IncreaseSum,

                            IsTaxable = sale.IsTaxable,
                            PositionDetail = sale.PositionDetail,
                            ProductId = sale.ProductId,
                            Psu = sale.Psu,
                            Promotion = sale.Promotion,
                            Price = sale.Price,
                            ProductUnit = sale.ProductUnit,
                            Section = sale.Section,
                        };
                        sale.ModifierGoods.ForEach(orderSaleSplit.ModifierGoods.Add);

                        //orderSaleSplit.Amount = sale.Count;
                        //var orderSaleSplit = new OrderSale()
                        //{
                        //    Id = sale.Id,
                        //}sale;

                        splitOrder.OrderSales.Add(orderSaleSplit);

                        Order.OrderSales.Remove(sale);

                    }


                    splitOrder.OrderStates.Add(new OrderStatuses()
                    {
                        Duration = 0,
                        OrderStateEnum = OrderStatus.New,
                    });


                    splitOrder.OrderReceipt.ResultSum = splitOrder.OrderSales.Sum(s => s.Sum);



                    await RecalculationGuest(splitOrder);

                    realm.Add(splitOrder);
                    realm.Add(Order);

                    if (transaction.State == TransactionState.Running)
                    {
                        await transaction.CommitAsync();
                    }
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


        }
        catch { }
    }

    private async Task CombineOrders(IEnumerable<Orders> orders)
    {
        try
        {
            var combinedOrder = new Orders();

            combinedOrder.BrandId = CurrentBrandId;
            combinedOrder.Employer = realm.All<Employer>().FirstOrDefault(doc => doc.Id == CurrentEmpId);
            combinedOrder.SalePointId = CurrentSalePointId;
            combinedOrder.ShiftId = CurrentCashShiftId;
            combinedOrder.OrderReceipt.CashierId = CurrentEmpId;
            combinedOrder.OrderReceipt.CreationDate = DateTime.Now;
            combinedOrder.OrderReceipt.ModifyDate = DateTime.Now;
            combinedOrder.OrderReceipt.Table = Order.OrderReceipt.Table;
            combinedOrder.Client = Order.Client; // I don't know where to get currentClient

            foreach (var order in orders)
            {
                combinedOrder.GuestCount += order.GuestCount;
                foreach (var sale in order.OrderSales)
                {
                    var promotions = realm.All<Promotion>().ToList().Where(x => x.PromotionConditions.ToList().Any(y => y.ProductId == sale.Product.Id) && !x.IsDeleted).AsQueryable();
                    if (promotions.Count() < 1)
                        return;
                    var total_promotions = ContainsPromotionConditions(sale, promotions);
                    if (total_promotions.Count() < 1)
                        return;

                    sale.DiscountPercent = (double)(combinedOrder.Client?.ClientGroup.Value);

                    sale.DiscountSum = (sale.Price * (decimal)sale.Amount * (decimal)(sale.DiscountPercent / 100));
                    if (sale.DiscountSum < 0.0m)
                        sale.DiscountSum = 0m;
                    sale.Sum = sale.Price * (decimal)sale.Amount - sale.DiscountSum;
                    combinedOrder.OrderSales.Add(sale);
                }

                foreach (var status in order.OrderStates)
                {
                    var combineOrderStatus = combinedOrder.OrderStates.FirstOrDefault(doc => doc.OrderStateRaw == status.OrderStateRaw);
                    if (combineOrderStatus == null)
                    {
                        combinedOrder.OrderStates.Add(status);
                    }
                    else
                    {
                        combineOrderStatus.Duration += status.Duration;
                    }
                }

                foreach (var payment in order.OrderReceipt.OrderReceiptPayments)
                {
                    var combineOrderPayment = combinedOrder.OrderReceipt.OrderReceiptPayments
                            .FirstOrDefault(doc => doc.PaymentMethod == payment.PaymentMethod);
                    if (combineOrderPayment == null)
                    {
                        combinedOrder.OrderReceipt.OrderReceiptPayments.Add(payment);
                    }
                    else
                    {
                        combineOrderPayment.Sum += payment.Sum;
                    }
                }
            }
            combinedOrder.OrderReceipt.ResultSum = combinedOrder.OrderSales.Sum(s => s.Sum);
            RecalculationGuest(combinedOrder);

            await realm.WriteAsync(() =>
            {
                realm.Add(combinedOrder);
                foreach (var order in orders)
                {
                    realm.Remove(order);
                }
            });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private async Task getOrders()
    {
        //try
        //{


        //    if (Order is null)
        //        return;

        //    AllOrders = (
        //        realm.All<Orders>().Where(x =>
        //        (x.Id != Order.Id)
        //        && (Order.OrderReceipt.Table == x.OrderReceipt.Table )
        //        && x.OrderStatus !=OrderStatus.Closed
        //          //&&
        //          //(x.OrderStatus == OrderStatus.New || x.IsOnline || x.OrderDelivery?.DeliveryStatus == DeliveryStatus.OnWay ||
        //          //  x.OrderStatus == OrderStatus.Success || x.OrderDelivery?.DeliveryStatus == DeliveryStatus.Delivered)

        //          ));

        //    OnPropertyChanged(nameof(Order));

        //}
        //catch (Exception ex) { Crashes.TrackError(ex); }
    }



    public async Task LoadDependencies()
    {
        //IsOpenOrderDetail = false;


        try
        {


            Promotions.ItemAdded += Promotions_ItemAdded;
            promotion_in_order.ItemAdded += Promotions_InOrder_ItemAdded;
            if (!IsNotNull(Order)) return;
            _ = SaveLog($"{AppResources.OpenOrder} {Order.Name}");
            var ls = Order.OrderSales
                 .GroupBy(x => x.GuestIndex)

                 .ToList()
                 .Select(x => new GuestGroup(groupIndex: x.Key, orders: x.ToList()));

            GuestGroups.ReplaceRange(ls);
            CurrentGuest = CurrentGuest;

            Order.OrderSales.Where(x => IsNotNull(x.Promotion)).Select(x => x.Promotion).ToList().ForEach(x =>
            {
                if (!promotion_in_order.Contains(x))
                    promotion_in_order.Add(x);
            });



            CurrentGuest = Order.OrderSales.FirstOrDefault();

            CurrentClient = Order.Client;
            //isDiscountClientBonus = Order.OrderSales.All(x => x.DiscountPercent == CurrentClient?.ClientGroup?.Value && x.DiscountSum == CurrentClient?.ClientGroup?.BonusOnBirthday) && CurrentClient?.ClientGroup?.IsDeleted == false;
            // await Task.Delay(50);
            isDiscountClientBonus = (IsNotNull(CurrentClient?.ClientGroup) && CurrentClient?.ClientGroup?.IsDeleted == false && CurrentClient?.ClientGroup?.Value > 0 && Order.OrderSales.All(x => x.DiscountPercent == CurrentClient?.ClientGroup?.Value));
            ItemsCondition = x => x.IsMain;
            VisualClients.ReplaceRange(Clients.Where(doc => !doc.IsDeleted));
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
        }

        //try
        //{

        //    if (!IsNotNull(Order)) return;




        //    //VisualItems.Clear();
        //    // VisualItems.AddRange(Menu?.Items.Where(x => x?.IsMain == true));





        //    if (Order.OrderSales.Count < 1)
        //        await AddGuest();
        //    CurrentGuest = Order.OrderSales[0];





        //    // SelectedGuestOrClient(guest: Order.OrderSales[0]);

        //    //_=  getOrders();



        _ = SaveOrderTotalSumm();
        //    _ = RecalculationGuest();
        //    DeliveryZones?.Clear();
        //    DeliveryZones?.AddRange(realm.All<DeliveryZone>().ToList());
        //    CurrentClient = Order.Client;
        //    Order.OrderSales.Where(x => IsNotNull(x.Promotion)).Select(x => x.Promotion).ToList().ForEach(x =>
        //    {
        //        if (!promotion_in_order.Contains(x))
        //            promotion_in_order.Add(x);
        //    });



        //    ItemsCondition = x => x.IsMain;

        //}
        //catch (Exception ex) { Crashes.TrackError(ex); }
        //finally
        //{


        //    //  isOpenOrderDetail = false;

        //}


        ////ListView.DataSource.RefreshFilter();


    }

    [RelayCommand]
    async Task UnlinkClient()
    {
        try
        {
            if (IsOrderPayed)//|| IsPredCheck)
            {

                return;
            }

            //if (!CheckAccessEditOrder)
            //{
            //    await DialogService.ShowAlertAsync("Ошибка", "У вас нет прав взаимодействовать с заказом", "OK");
            //    return;
            //}


            if (isDiscountClientBonus)
            {
                await realm.WriteAsync(() =>
                {

                    Order.OrderSales.ToList().ForEach(x =>
                    {
                        x.DiscountPercent = 0d;
                        // x.DiscountSum = client.ClientGroup.BonusOnBirthday;


                    });
                    isDiscountClientBonus = false;

                });
            }


            CurrentClient = null;

            await realm.WriteAsync(() =>
             {
                 Order.Client = CurrentClient;
             });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private void OnSearchTextChanged()
    {
        try
        {
            if (!IsNotNull(Order, Menu?.Items))
                return;
            if (Menu?.Items.Count() < 1)
                return;
            if (IsNotNull(SearchTextProducts))
            {
                CategoryTitle = "...";



                //VisualItems.ReplaceRange(
                //    Menu?.Items.Where(x => x?.Name.Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase) == true
                //|| x?.Product?.Name.Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase) == true)
                //    );
                //VisualItems =(
                //    Menu?.Items.Where(x => x?.Name.Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase) == true
                //|| x?.Product?.Name.Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase) == true)
                //    );
                ItemsCondition = (
                x =>
                        x.Name.Contains(SearchTextProducts.ToLower(),
                        StringComparison.CurrentCultureIgnoreCase) == true
                        ||
                        x.Product.Name.Contains(SearchTextProducts.ToLower(),
                        StringComparison.CurrentCultureIgnoreCase) == true
                );
                return;
            }
            GoToPreviousCategory();

        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
        }

    }

    private void OnSearchClientTextChanged()
    {
        try
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
                VisualClients.ReplaceRange(Clients.Where(doc => !doc.IsDeleted));

        }
        catch (Exception ex) { Crashes.TrackError(ex); }


    }
    //device name - fullname member
    //при добавлении/удаление продукта логи не ведутся
    //убрать №
    //
    private async Task SaveLog(string title)
    {
        try
        {
            await realm.WriteAsync(() =>
            {
                Order.Logs.Add(new Logs { CreaterDate = DateTimeOffset.Now, Title = title, DeviceName = HomePageViewModel.getInstance().CurrentUser.FullName });
            });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private async Task RecalGuest()
    {
        //try
        //{
        //    if (!IsNotNull(Order))
        //        return;
        //    int count = 1;

        //    await realm.WriteAsync(() =>
        //    {
        //        foreach (var guest in Order.OrderSales
        //        .GroupBy(x => x.GuestName))
        //        {
        //            foreach(var g in guest)
        //            {

        //                    g.GuestName = $"Guest{count}";
        //            }
        //            count++;

        //        }
        //    });
        //    CurrentGuest = Order.OrderSales.FirstOrDefault();
        //    //ListView.DataSource.RefreshFilter();
        //}
        //catch (Exception ex) { 
        //    Crashes.TrackError(ex);
        //}
        //await Task.Delay(100);



    }
    private async Task RecalculationGuest(Orders order)
    {
        //try
        //{
        //    if (!IsNotNull(order))
        //        return;
        //    int count = 1;


        //    var guestList = order.OrderSales.ToList().OrderBy(x => x.GuestName);

        //    var guestTempName = "";

        //    foreach (var guest in guestList)
        //    {
        //        if (guestTempName != guest.GuestName)
        //        {
        //            guest.GuestName = $"{Resources.AppResources.Guest} {count}";
        //            count++;
        //            guestTempName = guest.GuestName;
        //        }
        //    }
        //}
        //catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    public void OrderList()
    {
        try
        {
            if (Order.Detail?.OrderType != OrderType.InTheInstitution)
                return;
            if (!IsOrderListVisible)
                _ = getOrders();
            try
            {

                IsOrderListVisible = !IsOrderListVisible;

                if (IsOrderListVisible)
                {

                    AllOrdersListView.DataSource.Filter = FilterOrder;
                    AllOrdersListView.DataSource.RefreshFilter();
                }

            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex);
                IsOrderListVisible = false;
            }
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private void ClosingPopups()
    {
        IsOrderListVisible = false;
        IsClient = false;
        IsExtraOn = false;
        IsOpenPromotion = false;
        IsOrderComment = false;
        IsDelivery = false;
        IsTakeaway = false;
    }

    [RelayCommand]
    public async void Back()
    {
        try
        {
            Homepage.CurrentState = StateKey.Orders;



            Promotions.Clear();
            promotion_in_order.Clear();
            PromotionCount = 0;
            _ = SaveOrder();
            IsOpenOrderDetail = false;
            ClosingPopups();


            HomePageViewModel.getInstanceOrderContentPage().MakeFilter();
            //         ClosingPopups();




        }
        catch (Exception ex) { Crashes.TrackError(ex); }



        //  await AppShell.Current.GoToAsync("..");
    }
    private async Task AddOrderDeliveryFromClient(Client client)
    {
        try
        {
            if (!IsNotNull(client))
                return;


            realm ??= GetRealm();


            await realm.WriteAsync(async () =>
            {
                var address = client.ClientAddress.FirstOrDefault();
                if (IsNotNull(address))
                {
                    if (IsNotNull(Order.OrderDelivery?.Address))
                        Order.OrderDelivery.Address.Address = $"{address.Country} {address.City} ";
                    Order.Client = client;
                    if (IsNotNull(Order.OrderDelivery))
                    {
                        Order.OrderDelivery.DeliveryStatus = DeliveryStatus.New;
                        Order.OrderDelivery.Address.Description = address.Address;
                        Order.OrderDelivery.Description = address.Description;
                    }
                }


                if (
                (
                Order.OrderSales.ToList().Any(x => x.DiscountPercent == 0 && x.DiscountSum == 0) &&
                IsNotNull(CurrentClient.ClientGroup) &&
                CurrentClient?.ClientGroup?.IsDeleted == false &&
                CurrentClient?.ClientGroup?.Value > 0
                ) ||
                (
                isDiscountClientBonus &&
                CurrentClient?.ClientGroup?.IsDeleted == false &&
                CurrentClient?.ClientGroup?.Value > 0
                )
                )
                    await AddPromotionFromClientGroup(CurrentClient);
            });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private async Task AddPromotionFromClientGroup(Client client)
    {
        if (Homepage.CurrentBrand.OnAddDiscount)
        {
            var res = await DialogService.ShowAdminView();
            if (!res)
            {
                return;
            }
        }
        try
        {
            if (!IsNotNull(client)) return;
            await realm.WriteAsync(() =>
            {

                Order.OrderSales.ToList().ForEach(x =>
                {
                    x.DiscountPercent = client.ClientGroup.Value;
                    // x.DiscountSum = client.ClientGroup.BonusOnBirthday;


                });
                isDiscountClientBonus = true;

            });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private async Task SaveOrder()
    {
        try
        {

            await realm.WriteAsync(() =>
            {
                Order.OrderSales.ForEach(x =>
                {
                    x.Sum = x.TotalPrice;
                });
            });
            await SaveOrderTotalSumm();
            Promotions.ItemAdded -= Promotions_ItemAdded;
            promotion_in_order.ItemAdded -= Promotions_InOrder_ItemAdded;
            IsClient = false;
            IsDelivery = false;
            subscribeOrders?.Dispose();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private async Task Print()
    {
        if (Homepage.CurrentBrand.IsBill)
        {
            await DialogService.ShowToast($"Запрет печати пречека");
            return;
        }
        try
        {

            if (IsOrderPayed)
                return;

            //await realm.WriteAsync(() =>
            //{
            //    Order.OrderStatus = OrderStatus.Precheck;
            //});

            var printer = realm.All<CashRegister>().FirstOrDefault(doc => doc.IsActive
                && doc.CashRegisterTypeRaw == (int)CashRegisterType.Printer
                && !doc.IsDeleted);

            await DialogService.ShowToast($"Отправлено в печать на принтер {printer.CashRegisterSetting.IpAddress}");

            string GS = Convert.ToString((char)29);
            string ESC = Convert.ToString((char)27);

            string CUTCOMMAND = "";
            CUTCOMMAND = ESC + "@";
            CUTCOMMAND += GS + "V" + (char)48;
            var salePoint = realm.All<SalesPoint>().FirstOrDefault();
            var plain_text = $"{salePoint.Name,28}\r\n\r\n\r\n{AppResources.Receipt} № {Order.Number,20}\r\n{new string('-', 48)}\r\n{AppResources.Cashier} {AuthorName,20}\r\n{new string('-', 48)}\r\n{AppResources.Printed} {DateTime.Now.ToString("dd MMMM yyyy HH:mm"),27}\r\n{new string('-', 48)}\r\n{AppResources.TheOrderIsOpen} {Order.CreationDate.LocalDateTime.ToString("dd MMMM yyyy HH:mm"),24}\r\n\r\n" +
                $"{GeneratePlainTextTable(Order.OrderSales.Where(x => IsNotNull(x.Name)).ToList())}\r\n\r\n{AppResources.Total}{new string('.', 33)} {Order.OrderReceipt.ResultSum,-10}\r\n{new string('-', 48)}\r\n{AppResources.TotalAmountToBePaid}{new string('.', 24)}{Order.OrderReceipt.ResultSum,-10}\r\n\r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n  {CUTCOMMAND} ";

            if (printer != null)
            {
                PrintSevice.PrintText(plain_text, printer);
            }
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    //  private bool CheckAccessEditOrder { get => Homepage.CurrentUser?.Post?.IsAdministrator == true || Order.CreaterEmpId == Homepage.CurrentUser.Id; }
    static string GenerateHtmlTable(List<OrderSale> data)
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
        catch (Exception ex) { Crashes.TrackError(ex); return ""; }
    }


    static IEnumerable<string> WrapText(string text, int maxLength)
    {
        for (int i = 0; i < text.Length; i += maxLength)
        {
            yield return text.Substring(i, Math.Min(maxLength, text.Length - i));
        }
    }








    [RelayCommand]
    private async Task SaveComment()
    {
        if (CurrentOrderSale == null) return;
        try
        {
            await realm.WriteAsync(() =>
            {

                CurrentOrderSale.Comment = CommentText;

            });

        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    private bool FiltersItemes(object obj)
    {
        //order.OrderDelivery.DeliveryStatus = DeliveryStatus.Success;
        try
        {
            var item = obj as Items;


            bool result = ItemsCondition.Compile()(item);
            return result;
        }
        catch (Exception ex) { Crashes.TrackError(ex); return false; }


    }

    private async void MakeFilter()
    {
        try
        {
            if (ItemsListView == null) return;
            if (ItemsListView.DataSource != null)
            {
                if (ItemsListView.DataSource.Filter != FiltersItemes)
                {
                    ItemsListView.DataSource.Filter = FiltersItemes;
                }
                ItemsListView.DataSource.RefreshFilter();

            }

            /*else
            {
               
                if (ItemsListView.DataSource != null)
                {   ItemsListView.DataSource.BeginInit();
                    
                    ItemsListView.DataSource.Filter = FiltersItemes;
                    Items//ListView.DataSource.RefreshFilter();
                    ItemsListView.DataSource.EndEdit();
                }

            }*/
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    internal void OnDisappearing()
    {

    }
}
