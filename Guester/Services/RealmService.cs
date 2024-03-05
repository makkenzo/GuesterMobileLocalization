
using Realms.Sync;
using System.Linq.Expressions;
using Transaction = Guester.Models.Transaction;

namespace Guester.Services;

public static class RealmService
{



//    private static readonly string APIKey = "mP1sbzxkm18IDQYx2CFKGz7CgwlUQMF9ceZ5pBFPskdbQ9xiooIWe7HxhAdwZPqt";
   private static readonly string APIKey = "bjZWzlr4v9MOEzEkZraTzcISGkY4gyi7ZkK7Bg0bNBlPw7OOuhkHYOylsEY9VE5A";


    //master

    //private static readonly string APIKey = "AqdF0g4sx2OavlLVBui4182tUzNmztgXgS9DQ9Gly6h4pwYy79qLKbucpvXtNqqo";

    //test

    //private static readonly string APIKey = "bjZWzlr4v9MOEzEkZraTzcISGkY4gyi7ZkK7Bg0bNBlPw7OOuhkHYOylsEY9VE5A";

    public static string CurrentDivaceId { get => Preferences.Get(nameof(CurrentDivaceId), ""); set => Preferences.Set(nameof(CurrentDivaceId), value); }
    public static string CurrentBrandId { get => Preferences.Get(nameof(CurrentBrandId), ""); set => Preferences.Set(nameof(CurrentBrandId), value); }

    public static string CurrentSalePointId { get => Preferences.Get(nameof(CurrentSalePointId), ""); set => Preferences.Set(nameof(CurrentSalePointId), value); }

    private static bool serviceInitialised;

    private static Realms.Sync.App app;

    private static Realm mainThreadRealm;

    public static User CurrentUser => app.CurrentUser;


    // Инициализация сервиса
    public static async Task Init()
    {
  
        if (serviceInitialised)
            return;
       
        var config = new RealmAppConfig
        {



         //   AppId = "guestertestdev-nrgdn",
            AppId = "guesterapp-uxcdb",


            //master

            //AppId = "guester-ecrdl",

            //test

            //AppId = "guesterapp-uxcdb",

            BaseUrl = "https://realm.mongodb.com"
        };

        var appConfiguration = new Realms.Sync.AppConfiguration(config.AppId)
        {
            BaseUri = new Uri(config.BaseUrl)
        };

        app = Realms.Sync.App.Create(appConfiguration);


        serviceInitialised = true;
        await Task.CompletedTask;
    }
    // 
    public static Realm GetMainThreadRealm() => mainThreadRealm ??= GetRealm();
  

    public static Realm GetRealm()
    {

        var config = new FlexibleSyncConfiguration(app.CurrentUser)
        {
            PopulateInitialSubscriptions = SubscribesRealm
        };

        mainThreadRealm = Realm.GetInstance(config);
        return mainThreadRealm;
    }



    public static async Task LoginAsync()
    {
        if (app == null)
            await Init();
        try
        {
            var user = await app.LogInAsync(Credentials.ApiKey(APIKey));

        mainThreadRealm = GetRealm();
        await SetSubscription(mainThreadRealm, SubscriptionType.All);
        await mainThreadRealm.Subscriptions.WaitForSynchronizationAsync();
        }
        catch (Exception ex)
        {
            var i = ex;
        }
    }

    public static async Task LogoutAsync()
    {
        mainThreadRealm.Subscriptions.Update(() =>
        {
            mainThreadRealm.Subscriptions.RemoveAll(true);
        });

        await app.CurrentUser.LogOutAsync();
        mainThreadRealm?.Dispose();
        mainThreadRealm = null;
    }

    public static async Task SetSubscription(Realm realm, SubscriptionType subType)
    {
        try
        {
            if (realm != null && !realm.IsClosed)
                realm.Subscriptions.Update(() =>
                {
                    realm.Subscriptions.RemoveAll(true);
                    SubscribesRealm(realm);
                });
            else
                realm = GetRealm();
            //There is no need to wait for synchronization if we are disconnected
            if (realm.SyncSession.ConnectionState != ConnectionState.Disconnected)
                await realm.Subscriptions.WaitForSynchronizationAsync();
        }catch(Exception ex)
        {
            var i = ex;
        }
    }

    private static (IQueryable<T> Query, string Name) GetQuery<T>(Realm realm, SubscriptionType subType, Expression<Func<T, bool>> filterExpression) where T : RealmObject
    {
        IQueryable<T> query = null;
        string queryName = null;

        if (subType == SubscriptionType.Mine)
        {
            query = realm.All<T>().Where(filterExpression);
            queryName = $"mine{typeof(T).Name}";
        }
        else if (subType == SubscriptionType.All)
        {
            query = realm.All<T>();
            queryName = $"all{typeof(T).Name}s";
        }
        else
        {
            throw new ArgumentException("Unknown subscription type");
        }
        return (query, queryName);
    }


    private static void SubscribesRealm(Realm realm)
    {

        #region Access
        var (queryCashRegister, queryNameCashRegister) = GetQuery<CashRegister>(realm, SubscriptionType.Mine, x => x.SalePointId == CurrentSalePointId);
        var (queryUser, queryNameUser) = GetQuery<Employer>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryPost, queryNamePost) = GetQuery<Post>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryPremises, queryNamePremises) = GetQuery<Premises>(realm, SubscriptionType.Mine, x => x.SalesPointId == CurrentSalePointId);


        var (querySalesPoint, queryNameSalesPoint) = GetQuery(realm, SubscriptionType.Mine, GetLinq<SalesPoint>("Id", CurrentSalePointId));
        #endregion

        #region Business
        var (queryBrand, queryNameBrand) = GetQuery(realm, SubscriptionType.Mine, GetLinq<Brand>("Id", CurrentBrandId));
        #endregion

        #region Finanse

        var (queryAccount, queryNameAccount) = GetQuery<Account>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryShift, queryNameShift) = GetQuery<Shift>(realm, SubscriptionType.Mine, x => x.SalePointId == CurrentSalePointId);
        var (queryTransaction, queryNameTransaction) = GetQuery<Transaction>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (querySalesTransactionCategory, queryNameTransactionCategory) = GetQuery<TransactionCategory>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);

        #endregion

        #region Marketing
        var (queryClient, queryNameClient) = GetQuery<Client>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryClientGroup, queryNameClientGroup) = GetQuery<ClientGroup>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryLoyaltyProgram, queryNameLoyaltyProgram) = GetQuery<LoyaltyProgram>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryLoyaltyProgramsException, queryNameLoyaltyProgramsException) = GetQuery<LoyaltyProgramsException>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryPromotion, queryNamePromotion) = GetQuery<Promotion>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        #endregion

        #region Menu

        var (queryCategory, queryNameCategory) = GetQuery<Category>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);

        var (queryCookingMethod, queryNameCookingMethod) = GetQuery<CookingMethod>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryModifiers, queryNameModifiers) = GetQuery<Modifiers>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
       // var (querySelectedModifiers, queryNameSelectedModifiers) = GetQuery<SelectedModifiers>(realm, SubscriptionType.All,x=>x.ModifierId!=null);
        var (queryProduct, queryNameProduct) = GetQuery<Product>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        #endregion

        #region Settings
        var (queryDeliveryZone, queryNameDeliveryZone) = GetQuery<DeliveryZone>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryPaymentMethod, queryNamePaymentMethod) = GetQuery<PaymentMethod>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryTaxes, queryNameTaxes) = GetQuery<Taxes>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        #endregion

        #region ShowCase
        var (queryMenu, queryNameMenu) = GetQuery<Menu>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryDetail, queryNameDetail) = GetQuery<Detail>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);


        var (queryOrders, queryNameOrders) = GetQuery<Orders>(realm, SubscriptionType.Mine, x => x.SalePointId == CurrentSalePointId);
        var (queryBooking, queryNameBooking) = GetQuery<Booking>(realm, SubscriptionType.Mine, x => x.SalesPointId == CurrentSalePointId);
        var (queryNotification, queryNameNotification) = GetQuery<Notification>(realm, SubscriptionType.Mine, x => x.SalesPointId == CurrentSalePointId);
        #endregion

        #region WareHouse

        var (queryWareHouse, queryNameWareHouse) = GetQuery<WareHouse>(realm, SubscriptionType.Mine, x => x.SalePointId == CurrentSalePointId);

        var (queryReasonsForWriteOff, queryNameReasonsForWriteOff) = GetQuery<ReasonsForWriteOff>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (queryRemainsOfWarehouse, queryNameRemainsOfWarehouse) = GetQuery<RemainsOfWarehouse>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        var (querySupplier, queryNameSupplier) = GetQuery<Supplier>(realm, SubscriptionType.Mine, x => x.BrandId == CurrentBrandId);
        #endregion



        realm.Subscriptions.Add(queryCashRegister, new SubscriptionOptions { Name = queryNameCashRegister });
        realm.Subscriptions.Add(queryUser, new SubscriptionOptions { Name = queryNameUser });
        realm.Subscriptions.Add(queryPost, new SubscriptionOptions { Name = queryNamePost });
        realm.Subscriptions.Add(querySalesPoint, new SubscriptionOptions { Name = queryNameSalesPoint });
        realm.Subscriptions.Add(queryPremises, new SubscriptionOptions { Name = queryNamePremises });

        realm.Subscriptions.Add(queryBrand, new SubscriptionOptions { Name = queryNameBrand });


        realm.Subscriptions.Add(queryAccount, new SubscriptionOptions { Name = queryNameAccount });
        realm.Subscriptions.Add(queryShift, new SubscriptionOptions { Name = queryNameShift });
        realm.Subscriptions.Add(queryTransaction, new SubscriptionOptions { Name = queryNameTransaction });
        realm.Subscriptions.Add(querySalesTransactionCategory, new SubscriptionOptions { Name = queryNameTransactionCategory });

        realm.Subscriptions.Add(queryClient, new SubscriptionOptions { Name = queryNameClient });
        realm.Subscriptions.Add(queryClientGroup, new SubscriptionOptions { Name = queryNameClientGroup });
        realm.Subscriptions.Add(queryLoyaltyProgram, new SubscriptionOptions { Name = queryNameLoyaltyProgram });
        realm.Subscriptions.Add(queryLoyaltyProgramsException, new SubscriptionOptions { Name = queryNameLoyaltyProgramsException });
        realm.Subscriptions.Add(queryPromotion, new SubscriptionOptions { Name = queryNamePromotion });


        realm.Subscriptions.Add(queryCategory, new SubscriptionOptions { Name = queryNameCategory });
        realm.Subscriptions.Add(queryCookingMethod, new SubscriptionOptions { Name = queryNameCookingMethod });
        realm.Subscriptions.Add(queryModifiers, new SubscriptionOptions { Name = queryNameModifiers });
     //   realm.Subscriptions.Add(querySelectedModifiers, new SubscriptionOptions { Name = queryNameSelectedModifiers });
        realm.Subscriptions.Add(queryProduct, new SubscriptionOptions { Name = queryNameProduct });

        realm.Subscriptions.Add(queryDeliveryZone, new SubscriptionOptions { Name = queryNameDeliveryZone });
        realm.Subscriptions.Add(queryPaymentMethod, new SubscriptionOptions { Name = queryNamePaymentMethod });
        realm.Subscriptions.Add(queryTaxes, new SubscriptionOptions { Name = queryNameTaxes });

        realm.Subscriptions.Add(queryMenu, new SubscriptionOptions { Name = queryNameMenu });
        realm.Subscriptions.Add(queryDetail, new SubscriptionOptions { Name = queryNameDetail });

        realm.Subscriptions.Add(queryOrders, new SubscriptionOptions { Name = queryNameOrders });

        realm.Subscriptions.Add(queryBooking, new SubscriptionOptions { Name = queryNameBooking });
        realm.Subscriptions.Add(queryNotification, new SubscriptionOptions { Name = queryNameNotification });

        realm.Subscriptions.Add(queryWareHouse, new SubscriptionOptions { Name = queryNameWareHouse });

        realm.Subscriptions.Add(queryReasonsForWriteOff, new SubscriptionOptions { Name = queryNameReasonsForWriteOff });
        realm.Subscriptions.Add(queryRemainsOfWarehouse, new SubscriptionOptions { Name = queryNameRemainsOfWarehouse });
        realm.Subscriptions.Add(querySupplier, new SubscriptionOptions { Name = queryNameSupplier });
    }

    private static Expression<Func<IRealmObject, bool>> GetLinq<IRealmObject>(string propertyS, string valueS)
    {
        ParameterExpression parameter = Expression.Parameter(typeof(IRealmObject), "x");
        MemberExpression property = Expression.Property(parameter, propertyS);
        ConstantExpression value = Expression.Constant(valueS);
        BinaryExpression equal = Expression.Equal(property, value);

        return Expression.Lambda<Func<IRealmObject, bool>>(equal, parameter);
    }


 
    
    //public static async Task OnSale(string id)
    //{
    //    if (string.IsNullOrWhiteSpace(id))
    //        return;

    //    await CurrentUser.Functions.CallAsync<string>(nameof(OnSale), id);

    //}

    // Закритие заказа
    //public static async Task<bool> CloseOrder(Orders order)
    //{
    //    if (order == null)
    //    {
    //        Console.WriteLine("Error: Order dont close becose order null");
    //        return false;

    //    }

    //    return await CurrentUser.Functions.CallAsync<bool>("OnSale",order);
    //}

}






public enum SubscriptionType
{
    Mine,
    All,
}

internal class RealmAppConfig
{
    internal string AppId { get; set; }

    internal string BaseUrl { get; set; }
}
