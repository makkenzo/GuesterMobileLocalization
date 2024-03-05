using System.Globalization;
using ArcaCashRegister;
using Guester.Resources;
using Microsoft.AppCenter.Crashes;
using MvvmHelpers;
using Microsoft.Maui.Networking;
using Transaction = Guester.Models.Transaction;
using Syncfusion.Maui.DataSource.Extensions;

namespace Guester.ViewModels;

public partial class HomePageViewModel : BaseViewModel, IQueryAttributable
{
    [ObservableProperty]
    private Employer currentUser;
    private Realm realm;

    [ObservableProperty]
    private Premises currentHall;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(ChangeStateCommand))]
    bool canStateChange;

    [ObservableProperty]
    bool isInternet;

    [ObservableProperty]
    int ordersCount, notificationCount;

    [ObservableProperty]
    bool isEquipment, isNotificationPopup, isOpenCashRegister, isNew;

    [ObservableProperty]
    List<Employer> courers = new();



    [ObservableProperty]
    CashRegister currentCash;


    [ObservableProperty]
    string currentType;




    [ObservableProperty]
    IQueryable<Notification> notifications;

    [ObservableProperty]
    IQueryable<CashRegister> cashRegisters;
    public string ipAddress { get; set; }
    public bool IsNotificatedOrder { get => OrdersCount > 0; }
    public bool IsOpenPopup { get; set; }
    private string currentState = StateKey.Loading;

    [ObservableProperty]
    private Shift currentShift;
    public WebView webView { get; set; }
    public string CurrentState
    {
        get => currentState; set
        {
            if (value == currentState) return;

            if (value == nameof(StateKey.Checks))
            {

            }
            closing_popups();
            if (value == nameof(StateKey.Orders))
            {

            }
            currentState = value;






            TapVisible = value switch
            {
                nameof(StateKey.PayPage) => false,
                nameof(StateKey.OrderDetail) => false,
                _ => true
            };
            if (value is StateKey.Orders)
            {
                OrdersCount = 0;

            }

            OnPropertyChanged(nameof(CurrentState));
        }
    }

    [ObservableProperty]
    private Guester.Models.Transaction newTransaction = new();


    [ObservableProperty]
    Booking newBooking;


    [ObservableProperty]
    ObservableRangeCollection<Booking> reservationsList = new();


    [ObservableProperty]
    bool isLanguage, tapVisible, isOpenCurrentHallMap;

    [ObservableProperty]
    bool isMenuOpened = false, isBookingOpened, isReservationsOpened, isNewTransactionOpened, isOpenAllShifts; // Отоброжение модальных окон

    [RelayCommand(CanExecute = nameof(CanStateChange))]
    async void ChangeState(string state)
    {
        if (state == CurrentState) return;

        if (CurrentBrand.OnReceiptArchive && state == nameof(StateKey.Checks))
        {
            var res = await DialogService.ShowAdminView();
            if (!res)
            {
                return;
            }
            else
            {
                CurrentState = StateKey.Checks;
            }

        }
        else
        {
            CurrentState = state;
        }
    }
    [ObservableProperty]
    string currentLang = AppResources.ChoseLang;
    private static HomePageViewModel mainInstance;

    [ObservableProperty]
    private static Brand currentBrand;



    [ObservableProperty]
    IQueryable<Premises> premises;

    private static OrderDetailPageViewModel orderDetailInstance;
    private static PayPageViewModel payPageInstance;
    public static HomePageViewModel getInstance() => ServiceHelper.GetService<HomePageViewModel>();
    public static OrderDetailPageViewModel getInstanceOrderDetailPage() => ServiceHelper.GetService<OrderDetailPageViewModel>();
    public static HallMapContentViewModel getInstanceHallMapPage() => ServiceHelper.GetService<HallMapContentViewModel>();
    public static OrdersContentViewModel getInstanceOrderContentPage() => ServiceHelper.GetService<OrdersContentViewModel>();
    public static PayPageViewModel getInstancePayPageInstance() => ServiceHelper.GetService<PayPageViewModel>();

    partial void OnOrdersCountChanged(int value) => OnPropertyChanged(nameof(IsNotificatedOrder));


    partial void OnCurrentTypeChanged(string value)
    {
        if (IsNotNull(value))
        {

            switch (value)
            {
                case "Принтер":
                    CurrentCash.CashRegisterType = CashRegisterType.Printer;
                    break;
                case "ККМ":
                    CurrentCash.CashRegisterType = CashRegisterType.CashRegister;
                    break;
                case "Сканнер":
                    CurrentCash.CashRegisterType = CashRegisterType.Scanner;
                    break;
                default:
                    CurrentCash.CashRegisterType = CashRegisterType.Printer;
                    break;



            }

            OnPropertyChanged(nameof(CurrentCash));

        }
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.Count > 0 && query.ContainsKey(nameof(IsOpenPopup)) && query[nameof(IsOpenPopup)] != null)
        {
            IsOpenPopup = bool.Parse(query[nameof(IsOpenPopup)].ToString());
            if (IsOpenPopup)
                _ = StartCashRegisterShift();


        }

    }


    public string Currency { get => CurrentBrand.Currency switch { Models.Currency.KZT => "₸", Models.Currency.PUB => "₽", Models.Currency.USD => "$", Models.Currency.SOM => "SOʻM", _ => "$" }; }


    public string Language { get => CurrentBrand.Language switch { Models.Language.RU => $"{AppResources.Rus}", Models.Language.UZ => $"{AppResources.Uzbek}", Models.Language.KZ => $"{AppResources.Kz}", Models.Language.US => $"{AppResources.English}", _ => "" }; }





    public bool IsWareHouseVisible { get => CurrentUser.Post?.AccessDetails.FirstOrDefault(x => x.Page == (PageEnum.Stock))?.PageDetail.LastOrDefault(x => x.Access != PostEnum.NoAccess) != null; }




    public IQueryable<PersonalShift> PersonalShifts { get => CurrentShift.PersonalShifts.Where(x => x?.IsClosed == false && x?.Employer?.Id != CurrentUser?.Id).AsQueryable(); }


    public HomePageViewModel()
    {

        try
        {

            CurrentState = StateKey.Orders;
            CurrentLang = Preferences.Get(nameof(CurrentLang), AppResources.ChoseLang);

            realm ??= GetRealm();

            CurrentUser = realm.Find<Employer>(CurrentEmpId);
            CurrentBrand = realm.Find<Brand>(CurrentBrandId);
            CurrentShift = realm.Find<Shift>(CurrentCashShiftId);
            CurrentHall = realm.Find<Premises>(CurrentHallId);

            if (!IsNotNull(CurrentHall))
            {
                CurrentHall = realm.All<Premises>().ToList().FirstOrDefault(x => x.Tables.ToList().Count() > 0 && !x.IsDeleted && !x.IsWorkShop);
                CurrentHallId = CurrentHall?.Id;
            }



            Premises = realm.All<Premises>().Where(x => x.Id != CurrentHallId && !x.IsDeleted && !x.IsWorkShop);
            Courers = realm.All<Employer>().ToList().Where(x => x.Post?.IsCourier == true).ToList();


            var date = DateTime.Now.AddDays(1);
            var date2 = DateTime.Now.AddDays(-1);


            Notifications = realm.All<Notification>();//.Where(x=>x.CreationDateTime.Date < date.Date &&x.CreationDateTime.Date > date2.Date);





            // Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            closing_popups();
            //  ordersContentViewModel = getInstanceOrderDetailPage();

            realm.All<Shift>().Where(x => x.Id == CurrentCashShiftId)
                    .SubscribeForNotifications((sender, changes) =>
                    {
                        if (changes == null)
                            return;
                        foreach (var i in changes.NewModifiedIndices)
                        {
                            if (CurrentShift.IsClosed)
                            {
                                CurrentEmpId = string.Empty;
                                CurrentDivaceId = string.Empty;
                            }
                            return;
                        }
                    });

            realm.All<Notification>()
                    .SubscribeForNotifications((sender, changes) =>
                    {
                        if (changes == null)
                            return;
                        foreach (var i in changes.InsertedIndices)
                        {
                            NotificationCount = realm.All<Notification>().ToList().Count();
                        }
                    });


            realm.All<Employer>().Where(x => x.Post != null).SubscribeForNotifications((sender, changes) =>
            {
                if (changes == null)
                    return;

                foreach (var i in changes.InsertedIndices)
                {
                    var temp = realm.All<Employer>().ToList().Where(x => x.Post?.IsCourier == true).ToList();

                    if (Courers.Count != temp.Count)
                        Courers = temp;
                    return;
                }
                foreach (var i in changes.ModifiedIndices)
                {
                    CurrentUser = realm.Find<Employer>(CurrentEmpId);
                    return;
                }
            });
            IsBusy = false;
            CashRegisters = realm.All<CashRegister>().Where(x => !x.IsDeleted);
        }
        catch (Exception ex)
        {

            Crashes.TrackError(ex);
        }
    }

    ~HomePageViewModel()
    {
        Connectivity.ConnectivityChanged -= Connectivity_ConnectivityChanged;
    }
    void Connectivity_ConnectivityChanged(object sender, ConnectivityChangedEventArgs e) => IsInternet = e.NetworkAccess == Microsoft.Maui.Networking.NetworkAccess.Internet;



    internal async void OnAppearing()
    {
        try
        {

            //realm ??= GetRealm();
            //await Task.Delay(200);
            //CurrentUser = realm.Find<Employer>(CurrentEmpId);
            //CurrentBrand = realm.Find<Brand>(CurrentBrandId);
            //CurrentShift = realm.Find<Shift>(CurrentCashShiftId);


            //Courers = realm.All<Employer>().ToList().Where(x => x.Post?.IsCourier == true).ToList();
            //Connectivity.ConnectivityChanged += Connectivity_ConnectivityChanged;
            //closing_popups();

            //IsBusy = false;
            realm ??= GetRealm();

            CurrentUser = realm.Find<Employer>(CurrentEmpId);
            CurrentBrand = realm.Find<Brand>(CurrentBrandId);
            CurrentShift = realm.Find<Shift>(CurrentCashShiftId);
            CurrentHall = realm.Find<Premises>(CurrentHallId);
        }
        catch (Exception ex) { Crashes.TrackError(ex); }


    }


    partial void OnCurrentUserChanged(Employer value) => OnPropertyChanged(nameof(IsWareHouseVisible));

    partial void OnCurrentShiftChanged(Shift value) => OnPropertyChanged(nameof(PersonalShifts));
    partial void OnCurrentBrandChanged(Brand value)
    {

        OnPropertyChanged(nameof(Currency));
        OnPropertyChanged(nameof(Language));


    }



    [RelayCommand]
    void OpenNotification()
    {
        try
        {
            realm.Write(() =>
            {
                Notifications.Where(x => x.IsNotify).ForEach(x => x.IsNotify = false);

            });

            IsNotificationPopup = !IsNotificationPopup;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }


    [RelayCommand]
    private async Task DeleteCashRegister()
    {
        var value = realm.Find<CashRegister>(CurrentCash.Id);
        if (value is null) return;
        await realm.WriteAsync(() =>
         {
             value.IsDeleted = true;
             value.IsActive = false;
         });

        CurrentCash = null;
        OpenCashRegisterDetail();

    }

    private async Task StartCashRegisterShift()
    {
        CurrentShift = realm.All<Shift>().FirstOrDefault(x => x.Id == CurrentCashShiftId);

    OpenCashRegister:
        try
        {
            var cashRegistersTemp = await DialogService.ShowCaseRegisterView(AppResources.BalanceOfMoneyAfterCollection);
            if (!IsNotNull(cashRegistersTemp))
                throw new Exception("Error:CashRegisterEmpty don't get total sum on cash register");

            await realm.WriteAsync(() =>
             {
                 var emp = realm.All<Employer>().FirstOrDefault(x => x.Id == CurrentEmpId);
                 var personalShift = CurrentShift.PersonalShifts.FirstOrDefault(x => x.Employer?.Id == CurrentEmpId);
                 var salepoint = realm.Find<SalesPoint>(CurrentSalePointId);
                 Account ac = realm.Find<Account>(salepoint.CashAccountId.ToString());
                 personalShift.Transactions.Add(new()
                 {
                     BrandId = CurrentBrandId,
                     CreationDate = DateTimeOffset.Now,
                     Employer = emp,
                     ModifyDate = DateTimeOffset.Now,
                     UserId = emp.Id,
                     Description = cashRegistersTemp.Comment,
                     TotalSum = cashRegistersTemp.TotalSum,
                     TransactionType = TransactionType.OpeningOfCashShift,
                     ShiftId = CurrentCashShiftId,
                     //Account = ac
                 });


             });

            IsOpenPopup = false;
        }
        catch (Exception ex)
        {

            if (ex.Message.Contains("Error:CashRegisterEmpty"))
                goto OpenCashRegister;

            Crashes.TrackError(ex);
        }
        await Task.CompletedTask;

    }
    [RelayCommand]
    private async Task ChangeType(string res)
    {
        try
        {
            await realm.WriteAsync(() =>
            {
                NewTransaction.TransactionType = res switch { "0" => TransactionType.Income, "1" => TransactionType.Expenditure, "5" => TransactionType.Collection, _ => TransactionType.Income };

            });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }



    [RelayCommand]
    private async Task CreateTransaction()
    {
        try
        {
            await realm.WriteAsync(() =>
            {
                var emp = realm.All<Employer>().FirstOrDefault(x => x.Id == CurrentEmpId);
                NewTransaction.BrandId = CurrentBrandId;
                NewTransaction.Employer = emp;
                NewTransaction.ShiftId = CurrentCashShiftId;
                var salepoint = realm.Find<SalesPoint>(CurrentSalePointId);
                if (newTransaction.TransactionType == TransactionType.Income && newTransaction.TransactionType == TransactionType.Expenditure)
                {
                    NewTransaction.Account = salepoint.CashAccountId;
                }
                else if (newTransaction.TransactionType == TransactionType.Collection)
                {
                    var CollectionAccount = realm.Find<Account>(salepoint.CollectionAccountId);
                    NewTransaction.Account = CollectionAccount;
                }
                realm.Add(NewTransaction);
                NewTransaction = new();
                IsNewTransactionOpened = false;
            });
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }


    [RelayCommand]
    private void OpenCashRegisterDetail()
    {
        try
        {
            IsOpenCashRegister = !IsOpenCashRegister;

            if (IsOpenCashRegister)
            {

                CurrentCash = new();
                CurrentType = "ККМ";
                IsNew = true;
            }
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private async Task ExitPersonalShift()
    {
        try
        {
            IsBusy = true;
            await Task.Delay(150);
            if (!IsNotNull(CurrentShift))
                return;

            var emp = realm.All<Employer>().FirstOrDefault(x => x.Id == CurrentEmpId);
            var personalShift = CurrentShift.PersonalShifts.FirstOrDefault(x => x.Employer?.Id == CurrentEmpId && x.IsClosed == false);

            if (!IsNotNull(personalShift))
                goto GoToLockPage;



            await realm.WriteAsync(() =>
            {
                var salepoint = realm.Find<SalesPoint>(CurrentSalePointId);
                //    Account ac = realm.Find<Account>(salepoint?.CashAccountId.ToString());


                personalShift.Transactions.Add(new()
                {
                    BrandId = CurrentBrandId,
                    CreationDate = DateTimeOffset.Now.AddMinutes(DateTimeOffset.Now.Offset.TotalMinutes),
                    Employer = emp,
                    ModifyDate = DateTimeOffset.Now.AddMinutes(DateTimeOffset.Now.Offset.TotalMinutes),
                    UserId = emp.Id,
                    TransactionType = TransactionType.Translation,
                    ShiftId = CurrentCashShiftId,
                    //Account = ac
                });

                personalShift.EndDateTime = DateTimeOffset.Now;

                var time = personalShift.EndDateTime - personalShift.StartDateTime;
                personalShift.Durations = (long)time.TotalSeconds;
                personalShift.IsClosed = true;

            });


        GoToLockPage:

            closing_popups();
            App.Current.MainPage = new AppShell();

            IsBusy = false;
            await Task.CompletedTask;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
        finally { IsBusy = false; }
    }


    [RelayCommand]
    private async Task ExitFromCashShift()
    {
        if (!IsNotNull(CurrentShift))
            return;
        var count = 0;
    ExitCashRegister:
        try
        {
            if (CurrentBrand.IsNoSessionCloseOnBill)
            {
                var notCloseOrders = realm.All<Orders>()
                    .Where(doc => doc.ShiftId == currentShift.Id)
                    .ToList()
                    .Where(doc => doc.OrderStates.FirstOrDefault(s => s.OrderStateEnum == OrderStatus.Closed) == null);

                if (notCloseOrders.Count() > 0)
                {
                    await DialogService.ShowAlertAsync("Есть не закрытие заказы.", $"У вас не закрыты заказы.", "OK");
                    return;
                }
            }


            count++;
            var cashRegistersTemp = await DialogService.ShowCaseRegisterView(AppResources.BalanceOfMoneyAfterCollection, false);
            var personalShift = CurrentShift.PersonalShifts.FirstOrDefault(x => x.Employer?.Id == CurrentEmpId);
            var _id = personalShift?.Id;
            var emp = realm.All<Employer>().FirstOrDefault(x => x.Id == CurrentEmpId);

            if (!IsNotNull(personalShift))
                return;

            personalShift = CurrentShift.PersonalShifts.FirstOrDefault(x => x.Id == _id);

            var closed_orders = realm
               .All<Orders>()
               .Where(doc => doc.ShiftId == CurrentShift.Id)
               .ToList()
               .Where(x => x.OrderStatus == OrderStatus.Closed);

            var sumCash = closed_orders
                .Where(x => !x.OrderReceipt.IsRefund)
                .ToList()
                .Sum(x => x.OrderReceipt.OrderReceiptPayments
                    .Where(y => y.PaymentMethod.PaymentType == PaymentType.Cash)
                    .ToList()
                    .Sum(z => z.Sum));

            //var salesSum = realm.All<PaymentMethod>().ToList().AsQueryable()
            //    .Join(
            //        currentShift.ShiftPayments.AsQueryable(),
            //        payment => payment.Id,
            //        shiftPayment => shiftPayment.PaymentMethodId,
            //        (payment, shiftPayment) => new { payment, shiftPayment }
            //    )
            //    .Where(doc => doc.payment.PaymentTypeRaw == (int)PaymentType.Cash)
            //    .ToList()
            //    .Sum(s => decimal.Parse(s.shiftPayment.Sum));
            var expenditureTransactions = CurrentShift.PersonalShifts
                .SelectMany(personalShift => personalShift.Transactions, (personalShift, transactions) => new { personalShift, transactions })
                .Where(doc => doc.transactions.TransactionTypeRaw == (int)TransactionType.Expenditure)
                .Sum(s => s.transactions.TotalSum);
            var incomeTransactions = CurrentShift.PersonalShifts
                .SelectMany(personalShift => personalShift.Transactions, (personalShift, transactions) => new { personalShift, transactions })
                .Where(doc => doc.transactions.TransactionTypeRaw == (int)TransactionType.Income)
                .Sum(s => s.transactions.TotalSum);

            var openShiftTransaction = realm.All<Transaction>().FirstOrDefault(doc => doc.ShiftId == CurrentShift.Id
                && doc.TransactionTypeRaw == (int)TransactionType.OpeningOfCashShift);



            if (!IsNotNull(cashRegistersTemp))
                throw new Exception("sum is less then 0");

            using (var transaction = realm.BeginWrite())
            {

                try
                {
                    personalShift.EndDateTime = DateTimeOffset.Now;
                    var time = personalShift.EndDateTime - personalShift.StartDateTime;
                    personalShift.Durations = (long)time.TotalSeconds;
                    personalShift.IsClosed = true;
                    //var salepoint = realm.Find<SalesPoint>(CurrentSalePointId);
                    //Account ac = realm.Find<Account>(salepoint.CashAccountId.Id);
                    personalShift.Transactions.Add(new()
                    {
                        BrandId = CurrentBrandId,
                        CreationDate = DateTimeOffset.Now.AddMinutes(DateTimeOffset.Now.Offset.TotalMinutes),
                        Employer = emp,
                        ModifyDate = DateTimeOffset.Now.AddMinutes(DateTimeOffset.Now.Offset.TotalMinutes),
                        TotalSum = cashRegistersTemp.TotalSum,
                        Description = cashRegistersTemp.Comment,
                        UserId = emp.Id,
                        TransactionType = TransactionType.ClosingOfCashShift,
                        ShiftId = CurrentCashShiftId,
                        //Account = ac
                    });

                    CurrentShift.IsClosed = true;
                    CurrentShift.EndDateTime = DateTimeOffset.Now;
                    CurrentShift.Durations = (long)(CurrentShift.EndDateTime - CurrentShift.StartDateTime).TotalSeconds;

                    if (transaction.State == TransactionState.Running)
                    {
                        transaction.Commit();
                    }

                }
                catch (Exception ex)
                {

                    if (ex.Message.Contains("sum is less"))
                    {
                        await DialogService.ShowToast($"{AppResources.MistakeTheBalance}");
                        await Task.Delay(200);
                        goto ExitCashRegister;

                    }

                    if (transaction.State != TransactionState.RolledBack &&
                        transaction.State != TransactionState.Committed)
                    {

                        await DialogService.ShowToast($"{AppResources.MistakeIsImpossibleToClose}");
                        transaction.Rollback();
                        return;
                    }

                    Crashes.TrackError(ex);
                }
            }




            closing_popups();
            CurrentEmpId = string.Empty;
            /*    CurrentBrandId = string.Empty;
                CurrentSalePointId = string.Empty;
                CurrentDivaceId = string.Empty;*/
            IsBusy = false;
            await AppShell.Current.GoToAsync($"//{nameof(LoginPage)}");


        }
        catch (Exception ex)
        {
            if (count > 2)
            {
                await DialogService.ShowToast(AppResources.CantEndCashRegister);
                return;
            }
            if (ex.Message.Contains("Error:CashRegisterEmpty"))
                goto ExitCashRegister;
        }
    }




    [RelayCommand]
    private void OpenEquipment()
    {
        IsEquipment = !IsEquipment;
    }


    internal void OnDissapearing()
    {

    }

    [RelayCommand]
    public void LogOut()
    {
        CurrentEmpId = string.Empty;
        CurrentBrandId = string.Empty;
        CurrentSalePointId = string.Empty;
        CurrentDivaceId = string.Empty;
        closing_popups();
        App.Current.MainPage = new DeviceAuthPage(ServiceHelper.GetService<DeviceAuthViewModel>());
    }

    [RelayCommand]
    public void ChangeUser()
    {
        AuthorName = string.Empty;
        //App.Current.MainPage = new AppShell();
        AppShell.Current.GoToAsync($"//{nameof(LoginPage)}");
    }





    [RelayCommand]
    public async void LanguageChange(string parm)
    {
        try
        {
            IsLanguage = !IsLanguage;
            if (parm.Equals("Border"))
                return;


            if (parm.Equals("US"))
            {
                if (CurrentLang.Equals(AppResources.EngLang)) return;
                IsBusy = true;

                AppResources.Culture = new CultureInfo("en-US");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");
                SaveCulture = "en-US";
                CurrentLang = AppResources.EngLang;


            }
            else if (parm.Equals("OZ"))
            {
                if (CurrentLang.Equals(AppResources.OzLang)) return;
                IsBusy = true;

                AppResources.Culture = new CultureInfo("uz-Latn");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("uz-Latn");
                SaveCulture = "uz-Latn";
                CurrentLang = AppResources.OzLang;


            }
            else
            {
                if (CurrentLang.Equals(AppResources.RusLang)) return;
                IsBusy = true;

                AppResources.Culture = new CultureInfo("ru-Rus");
                Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-Rus");
                CurrentLang = AppResources.RusLang;
                SaveCulture = "ru-Rus";
            }
            Preferences.Set(nameof(CurrentLang), CurrentLang);
            Application.Current.MainPage = new AppShell();
            await Task.Delay(650);

            App.Current.MainPage = new AppShell();
            await Task.Delay(250);
            await AppShell.Current.GoToAsync($"//{nameof(HomePage)}");

            IsBusy = false;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private void FunctionalMenuOpen()
    {

        IsMenuOpened = !IsMenuOpened;
    }


    [RelayCommand]
    private void BookingOpen()
    {
        try
        {
            closing_popups();


            if (!IsBookingOpened)

                NewBooking = new();

            IsBookingOpened = !IsBookingOpened;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }


    }

    [RelayCommand]
    private void ReservationsOpen()
    {
        try
        {
            closing_popups();

            if (!IsReservationsOpened)
            {
                var tempLs = realm.All<Booking>().ToList().Where(x => x.Premises?.Id == CurrentHall?.Id && DateTime.Now.Date <= x.DateBooking.Date);

                if (ReservationsList.Count() != tempLs.Count())
                    ReservationsList.AddRange(tempLs.OrderByDescending(x => x.DateBooking));
            }


            IsReservationsOpened = !IsReservationsOpened;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private void NewTransactionOpen()
    {
        try
        {
            closing_popups();
            IsNewTransactionOpened = !IsNewTransactionOpened;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private async Task XReport()
    {
        if (CurrentBrand.OnZXReport)
        {
            var res = await DialogService.ShowAdminView();
            if (!res)
            { return; }
        }

        try
        {
            if (!IsNotNull(CurrentShift)) return;

            var personalShift = CurrentShift.PersonalShifts.FirstOrDefault(x => x.Employer?.Id == CurrentEmpId);
            if (!IsNotNull(personalShift)) return;
            var closed_orders = realm
                .All<Orders>()
                .Where(x => x.ShiftId == CurrentShift.Id)
                .ToList()
                .Where(x => x.OrderStatus == OrderStatus.Closed);

            var sumCash = closed_orders
                .Where(x => !x.OrderReceipt.IsRefund)
                .ToList()
                .Sum(x => x.OrderReceipt.OrderReceiptPayments
                    .Where(y => y.PaymentMethod.PaymentType == PaymentType.Cash)
                    .ToList()
                    .Sum(z => z.Sum));

            var sumCard = closed_orders
                .Where(x => !x.OrderReceipt.IsRefund)
                .ToList()
             .Sum(x => x.OrderReceipt.OrderReceiptPayments
                 .Where(y => y.PaymentMethod.PaymentType == PaymentType.Card)
                 .ToList()
                 .Sum(z => z.Sum));

            var totalSum = sumCash + sumCard;

            var refundSum = closed_orders
                .Where(x => x.OrderReceipt.IsRefund)
                .ToList()
                .Sum(x => x.OrderReceipt.OrderReceiptPayments
                    .Sum(z => z.Sum));


            var saleDocumentCount = closed_orders.Where(x => !x.OrderReceipt.IsRefund).Count();
            var refundDocumentCount = closed_orders.Where(x => x.OrderReceipt.IsRefund).Count();

            var moneyInCashRegister = personalShift.Transactions
                .Where(x => x.TransactionType == TransactionType.OpeningOfCashShift || x.TransactionType == TransactionType.ClosingOfCashShift)
                .MaxBy(x => x.ModifyDate).TotalSum;

            var sumPersonalSale = closed_orders
                .Where(x => !x.OrderReceipt.IsRefund && x.OrderReceipt.UserId == CurrentEmpId)
                .ToList()
                .Sum(x => x.OrderReceipt.OrderReceiptPayments
                    .Where(y => y.PaymentMethod.PaymentType == PaymentType.Cash)
                    .ToList()
                    .Sum(z => z.Sum));

            var html = $@"<!DOCTYPE html> <html> 

<head> 
    <meta charset=\""UTF-8\"">  <title>Отчёт о смене</title>  <style>
        body {{
              font-family: Arial, sans-serif;
              text-align: center;
             
        }}

            .report {{
              margin: 20px;
              padding: 20px;
              border: 1px solid #333;
              text-align: left;
            /* Выравнивание содержимого блока влево */
             
        }}

            h1 {{
              font-size: 24px;
             
        }}

            hr {{
              border: 0;
              border-top: 1px solid #333;
              margin: 10px 0;
             
        }}

            .section {{
              margin-top: 20px;
             
        }}

            h2 {{
              font-size: 18px;
              margin-bottom: 10px;
             
        }}

            ul {{
              list-style: none;
              padding: 0;
             
        }}

            li {{
              margin: 5px 0;
             
        }}

            p {{
              font-size: 16px;
             
        }}

           

        /* Выравнивание фигурных скобок влево */
          .report p {{
              display: flex;
              justify-content: space-between;
             
        }}

         
    </style> 
</head> 

<body> 
     <div class=\""report\"">  
        <h1>Guester</h1>  <p>{CurrentDivaceId}</p> 
        <hr>  <p>{AppResources.Zreport}</p> 
        <hr>  <div class=\""section\"">   </div>  <div
            class=\""section\"">  <h2>{AppResources.TheBeginningShift}:</h2>  <p>{personalShift.StartDateTime.ToString("dd MMM yyyy")}</p>  </div>  <div
            class=\""section\"">  <h2>{AppResources.TypesOfPayment}:</h2>  <ul>  <li>{AppResources.CashPayment}: {sumCash}</li>  <li>
                    {AppResources.CashlessPayment}: {sumCard}</li>  </ul>  </div>  <div class=\""section\"">  <h2>{AppResources.ResultsOfOperations}:</h2>  <ul>  <li>{AppResources.Sales}: {totalSum}</li>  <li>{AppResources.Refunds}: {refundSum}</li>  </ul>
              </div>  <div class=\""section\"">  <h2>{AppResources.TotalDocuments}:</h2>  <ul>  <li>{AppResources.Corrupt}: {saleDocumentCount}</li>  <li>{AppResources.Refunds}: {refundDocumentCount}</li>  </ul> 
        </div>  <div class=\""section\"">  <h2>{AppResources.CashOnHand}:</h2>  <p>{moneyInCashRegister}</p>  </div> 
        <hr>  <div class=\""section\"">  <h2>{AppResources.ChangeableTotal}:</h2>  <p>{sumPersonalSale}</p>  </div> 
    </div> </body> 

</html>";
            string GS = Convert.ToString((char)29);
            string ESC = Convert.ToString((char)27);

            string CUTCOMMAND = "";
            CUTCOMMAND = ESC + "@";
            CUTCOMMAND += GS + "V" + (char)48;

            var plain_text = $"Guester\r\n{AppResources.Devices}: {CurrentDivaceId}\r\n\r\n----------------------------\r\n\r\n{AppResources.Xreport}\r\n\r\n----------------------------\r\n\r\n{AppResources.TheBeginningShift}:\r\n{personalShift.StartDateTime.ToString("dd MMM yyyy")}\r\n\r\n----------------------------\r\n\r\n{AppResources.TypesOfPayment}:\r\n  - {AppResources.CashPayment}: {sumCash}\r\n  - {AppResources.CashlessPayment}: {sumCard}\r\n\r\n----------------------------\r\n\r\n{AppResources.ResultsOfOperations}:\r\n  - {AppResources.Sales}: {totalSum}\r\n  - {AppResources.Refunds}: {refundSum}\r\n\r\n----------------------------\r\n\r\n{AppResources.TotalDocuments}:\r\n  - {AppResources.Corrupt}: {saleDocumentCount}\r\n  - {AppResources.Returnable}: {refundDocumentCount}\r\n\r\n----------------------------\r\n\r\n{AppResources.CashOnHand}:\r\n  {moneyInCashRegister}\r\n\r\n----------------------------\r\n\r\n{AppResources.ChangeableTotal}:\r\n  {sumPersonalSale}\r\n\n\n\r\n\r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n  {CUTCOMMAND}";


            var cashRegister = realm.All<CashRegister>().FirstOrDefault(doc => doc.IsActive
                && doc.CashRegisterTypeRaw == (int)CashRegisterType.CashRegister);

            if (cashRegister != null)
            {
                CreateWebsocket createWebsocket = new CreateWebsocket(
                    new CreateWebsocket.Settings
                    {
                        Host = cashRegister.CashRegisterSetting.IpAddress,
                        Login = cashRegister.CashRegisterSetting.Login,
                        Password = cashRegister.CashRegisterSetting.Password,
                        Port = "8888",
                        Protocol = "ws://"
                    });
                createWebsocket.Send(null, 2);
            }
            var printer = realm.All<CashRegister>().FirstOrDefault(doc => doc.IsActive
                && doc.CashRegisterTypeRaw == (int)CashRegisterType.Printer
                && !doc.IsDeleted);

            if (printer != null)
            {
                PrintSevice.PrintText(plain_text, printer);
            }
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private async Task ZReport()
    {
        if (CurrentBrand.OnZXReport)
        {
            var res = await DialogService.ShowAdminView();
            if (!res)
            { return; }
        }
        try
        {

            if (!IsNotNull(CurrentShift)) return;

            if (CurrentBrand.IsNoSessionCloseOnBill)
            {
                var notCloseOrders = realm.All<Orders>()
                    .Where(doc => doc.ShiftId == currentShift.Id)
                    .ToList()
                    .Where(doc => doc.OrderStates.FirstOrDefault(s => s.OrderStateEnum == OrderStatus.Closed) == null);

                if (notCloseOrders.Count() > 0)
                {
                    await DialogService.ShowAlertAsync("Есть не закрытие заказы.", $"У вас не закрыты заказы.", "OK"); 
                    return;
                }
            }

            var personalShift = CurrentShift.PersonalShifts.FirstOrDefault(x => x.Employer?.Id == CurrentEmpId);
            if (!IsNotNull(personalShift)) return;
            var closed_orders = realm
                .All<Orders>()
                .Where(x => x.ShiftId == CurrentShift.Id)
                .ToList()
                .Where(x => x.OrderStatus == OrderStatus.Closed);

            var sumCash = closed_orders
                .Where(x => !x.OrderReceipt.IsRefund)
                .ToList()
                .Sum(x => x.OrderReceipt.OrderReceiptPayments
                    .Where(y => y.PaymentMethod.PaymentType == PaymentType.Cash)
                    .ToList()
                    .Sum(z => z.Sum));

            var sumCard = closed_orders
                .Where(x => !x.OrderReceipt.IsRefund)
                .ToList()
             .Sum(x => x.OrderReceipt.OrderReceiptPayments
                 .Where(y => y.PaymentMethod.PaymentType == PaymentType.Card)
                 .ToList()
                 .Sum(z => z.Sum));

            var totalSum = sumCash + sumCard;

            var refundSum = closed_orders
                .Where(x => x.OrderReceipt.IsRefund)
                .ToList()
                .Sum(x => x.OrderReceipt.OrderReceiptPayments
                    .Sum(z => z.Sum));


            var saleDocumentCount = closed_orders.Where(x => !x.OrderReceipt.IsRefund).Count();
            var refundDocumentCount = closed_orders.Where(x => x.OrderReceipt.IsRefund).Count();

            var moneyInCashRegister = personalShift.Transactions
                .Where(x => x.TransactionType == TransactionType.OpeningOfCashShift || x.TransactionType == TransactionType.ClosingOfCashShift)
                .MaxBy(x => x.ModifyDate)?.TotalSum;

            var sumPersonalSale = closed_orders
                .Where(x => !x.OrderReceipt.IsRefund && x.OrderReceipt.UserId == CurrentEmpId)
                .ToList()
                .Sum(x => x.OrderReceipt.OrderReceiptPayments
                    .Where(y => y.PaymentMethod.PaymentType == PaymentType.Cash)
                    .ToList()
                    .Sum(z => z.Sum));



            var html = @$"<!DOCTYPE html>
<html lang=""en"">

<head>
    <meta charset=""UTF-8"">
    <title>{AppResources.Zreport}</title>
    <style>
        html, body {{
            margin: 0;
            padding: 0;
            background-color: #f4f4f4;
        }}

        body {{
            font-family: Arial, sans-serif;
            text-align: left;
            color: #333;
        }}

        .report {{
            max-width: 600px;
        
            background-color: #fff;
            border-radius: 8px;
            box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
            padding: 20px;
            text-align: left;
            /* Выравнивание содержимого блока влево */
        }}

        h1 {{
            font-size: 28px;
            color: #2c3e50;
            margin-bottom: 10px;
        }}

        hr {{
            border: 0;
            border-top: 1px solid #ddd;
            margin: 15px 0;
        }}

        .section {{
            margin-top: 20px;
        }}

        h2 {{
            font-size: 22px;
            margin-bottom: 10px;
            color: #3498db;
        }}

        ul {{
            list-style: none;
            padding: 0;
        }}

        li {{
            margin: 8px 0;
        }}

        p {{
            font-size: 18px;
            color: #333;
        }}

        /* Выравнивание фигурных скобок влево */
        .report p {{
            display: flex;
            justify-content: space-between;
            margin: 8px 0;
        }}

    </style>
</head>

<body>
    <div class=""report"">
        <h1>{AppResources.ShiftReport}</h1>
        <hr>
        <p>{AppResources.TheReportWasDevice} - {CurrentDivaceId}</p>
        <hr>
        <p>{AppResources.Zreport}</p>
        <hr>
        <div class=""section"">
            <h2>{AppResources.TheBeginningShift}:</h2>
            <p>{personalShift.StartDateTime.ToString("dd MMM yyyy")}</p>
        </div>
        <div class=""section"">
            <h2>{AppResources.TypesOfPayment}:</h2>
            <ul>
                <li>{AppResources.CashPayment}: {sumCash}</li>
                <li>{AppResources.CashlessPayment}: {sumCard}</li>
            </ul>
        </div>
        <div class=""section"">
            <h2>{AppResources.ResultsOfOperations}:</h2>
            <ul>
                <li>{AppResources.Sales}: {totalSum}</li>
                <li>{AppResources.Refunds}: {refundSum}</li>
            </ul>
        </div>
        <div class=""section"">
            <h2>{AppResources.TotalDocuments}:</h2>
            <ul>
                <li>{AppResources.Corrupt}: {saleDocumentCount}</li>
                <li>{AppResources.Returnable}: {refundDocumentCount}</li>
            </ul>
        </div>
        <div class=""section"">
            <h2>{AppResources.CashOnHand}:</h2>
            <p>{moneyInCashRegister}</p>
        </div>
        <hr>
        <div class=""section"">
            <h2>{AppResources.ChangeableTotal}:</h2>
            <p>{sumPersonalSale}</p>
        </div>
    </div>
</body>

</html>
";
            string GS = Convert.ToString((char)29);
            string ESC = Convert.ToString((char)27);

            string CUTCOMMAND = "";
            CUTCOMMAND = ESC + "@";
            CUTCOMMAND += GS + "V" + (char)48;

            var plain_text = $"Z-Отчет\r\n\r\nОтчет составлен с устройства - {CurrentDivaceId}\r\n\r\nZ-отчет\r\n\r\n----------------------------\r\n\r\nНачало смены:\r\n {personalShift.StartDateTime.ToString("dd MMM yyyy")}\r\n\r\n----------------------------\r\n\r\nВиды оплаты:\r\n  - Наличный расчет: {sumCash}\r\n  - Безналичный расчет: {sumCard}\r\n\r\n----------------------------\r\n\r\nИтоги операций:\r\n  - Продажи: {totalSum}\r\n  - Возвраты: {refundSum}\r\n\r\n----------------------------\r\n\r\nВсего документов:\r\n  - Продажных: {saleDocumentCount}\r\n  - Возвратных: {refundDocumentCount}\r\n\r\n----------------------------\r\n\r\nНаличность в кассе:\r\n  {moneyInCashRegister}\r\n\r\n----------------------------\r\n\r\nСменный итог:\r\n  {sumPersonalSale}\r\n\n\n\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n \r\n  {CUTCOMMAND}";


            var cashRegister = realm.All<CashRegister>().FirstOrDefault(doc => doc.IsActive
                && doc.CashRegisterTypeRaw == (int)CashRegisterType.CashRegister);

            if (cashRegister != null)
            {
                CreateWebsocket createWebsocket = new CreateWebsocket(
                    new CreateWebsocket.Settings
                    {
                        Host = cashRegister.CashRegisterSetting.IpAddress,
                        Login = cashRegister.CashRegisterSetting.Login,
                        Password = cashRegister.CashRegisterSetting.Password,
                        Port = "8888",
                        Protocol = "ws://"
                    });
                createWebsocket.Send(null, 3);
            }

            var printer = realm.All<CashRegister>().FirstOrDefault(doc => doc.IsActive
                && doc.CashRegisterTypeRaw == (int)CashRegisterType.Printer
                && !doc.IsDeleted);

            if (printer != null)
            {
                PrintSevice.PrintText(plain_text, printer);
            }
            //закрытие касовой смены
            await ExitFromCashShift();

        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }








    [RelayCommand]
    private void OpenAllShifts(string param)
    {
        try
        {
            if (CurrentUser.IsDeleted) return;///проверка  доступности по роли

            closing_popups();

            IsOpenAllShifts = true;

            if (param.Equals("close"))
                IsOpenAllShifts = false;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }


    [RelayCommand]
    private async Task ClosePersonalShift(PersonalShift p)
    {
        try
        {
            var actualPersonalShift = CurrentShift.PersonalShifts.FirstOrDefault(p);
            if (!IsNotNull(actualPersonalShift))
            {

                return;
            }

            //  if (CurrentUser.IsDeleted) return;///проверка  доступности по роли
            var myShit = CurrentShift.PersonalShifts.FirstOrDefault(x => x.Employer?.Id == CurrentUser?.Id);
            await realm.WriteAsync(() =>
            {
                p.IsClosed = true;
                p.Transactions.Add(new()
                {
                    TransactionType = TransactionType.ClosingOfCashShift,
                    CreationDate = DateTime.Now,
                    BrandId = CurrentBrandId,
                    UserId = CurrentEmpId,
                    Employer = CurrentUser,
                    TotalSum = 0,

                });

            });
            if (myShit?.Id == actualPersonalShift.Id)
            {
                closing_popups();
                App.Current.MainPage = new AppShell();

                IsBusy = false;
            }


            //OnPropertyChanged(nameof(PersonalShifts));
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }


    [RelayCommand]
    private void BackPopup(string parameter = nameof(IsMenuOpened))
    {
        try
        {
            parameter ??= nameof(IsMenuOpened);
            closing_popups();

            switch (parameter)
            {
                case nameof(IsMenuOpened): FunctionalMenuOpen(); break;
                case nameof(IsReservationsOpened): ReservationsOpen(); break;

            };
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    [RelayCommand]
    private async Task Print()
    {
        try
        {
            var html = "<!DOCTYPE html>\n<html>\n<head>\n    <meta charset=\"UTF-8\">\n    <title>Отчёт о смене</title>\n    <style>body {\n        font-family: Arial, sans-serif;\n        text-align: center;\n    }\n    \n    .report {\n        margin: 20px;\n        padding: 20px;\n        border: 1px solid #333;\n        text-align: left; /* Выравнивание содержимого блока влево */\n    }\n    \n    h1 {\n        font-size: 24px;\n    }\n    \n    hr {\n        border: 0;\n        border-top: 1px solid #333;\n        margin: 10px 0;\n    }\n    \n    .section {\n        margin-top: 20px;\n    }\n    \n    h2 {\n        font-size: 18px;\n        margin-bottom: 10px;\n    }\n    \n    ul {\n        list-style: none;\n        padding: 0;\n    }\n    \n    li {\n        margin: 5px 0;\n    }\n    \n    p {\n        font-size: 16px;\n    }\n    \n    /* Выравнивание фигурных скобок влево */\n    .report p {\n        display: flex;\n        justify-content: space-between;\n    }\n    </style>\n</head>\n<body>\n    <div class=\"report\">\n        <h1>Guester</h1>\n        <p>{Devise number}  {Devise name}</p>\n        <hr>\n        <p>Z-отчёт № 0001. [type]</p>\n        <hr>\n        <div class=\"section\">\n            <h2>Закрытие смены:</h2>\n            <p>{Дата и время закрытия смены}</p>\n        </div>\n        <div class=\"section\">\n            <h2>Начало смены:</h2>\n            <p>{Дата и время начала смены}</p>\n        </div>\n        <div class=\"section\">\n            <h2>Виды оплаты:</h2>\n            <ul>\n                <li>Наличный расчёт: {Сумма наличных}</li>\n                <li>Безналичный расчёт: {Сумма безналичных}</li>\n            </ul>\n        </div>\n        <div class=\"section\">\n            <h2>Итоги операций:</h2>\n            <ul>\n                <li>Продажи: {Сумма продаж}</li>\n                <li>Возвраты: {Сумма возвратов}</li>\n            </ul>\n        </div>\n        <div class=\"section\">\n            <h2>Всего документов:</h2>\n            <ul>\n                <li>Продажных: {Количество продажных документов}</li>\n                <li>Возвратных: {Количество возвратных документов}</li>\n            </ul>\n        </div>\n        <div class=\"section\">\n            <h2>Наличность в кассе:</h2>\n            <p>{Сумма наличности в кассе}</p>\n        </div>\n        <hr>\n        <div class=\"section\">\n            <h2>Сменный итог:</h2>\n            <p>{Сумма сменного итога}</p>\n        </div>\n    </div>\n</body>\n</html>";


            await DependencyService.Get<IPrintService>().PrintHtml(html, "PrintJobName");
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private void ConnectedToDevice()
    {

    }


    [RelayCommand]
    private async Task SaveBooking()
    {
        try
        {
            if (!IsNotNull(NewBooking))
                return;

            await realm.WriteAsync(() =>
            {

                realm.Add(new Booking
                {
                    BookingClient = NewBooking.BookingClient,
                    Duration = NewBooking.Duration,
                    Premises = CurrentHall,
                    Table = new()
                    {
                        Name = NewBooking.Table.Name,
                        Id = NewBooking.Table.Id,
                        Picture = NewBooking.Table.Picture,
                        Color = NewBooking.Table.Color,
                        BorderRadius = NewBooking.Table.BorderRadius,
                        Height = NewBooking.Table.Height,
                        IsDeleted = NewBooking.Table.IsDeleted,
                        Zindex = NewBooking.Table.Zindex,
                        PosX = NewBooking.Table.PosX,
                        PosY = NewBooking.Table.PosY,
                        Seats = NewBooking.Table.Seats,
                        Width = NewBooking.Table.Width,
                    },
                    Comment = NewBooking.Comment,
                    SalesPointId = CurrentSalePointId,
                    GuestCount = NewBooking.GuestCount,


                });

            });
        }
        catch (Exception ex)
        {
            return;
        }


        NewBooking = null;
        BackPopup(nameof(IsReservationsOpened));



        await Task.CompletedTask;
    }



    [RelayCommand]
    private void SelectHalMap(Premises p)
    {
        try
        {
            if (!IsNotNull(p)) return;
            CurrentHallId = p.Id;
            CurrentHall = p;

            Premises = realm.All<Premises>().Where(x => x.Id != CurrentHallId && !x.IsDeleted && !x.IsWorkShop);

            IsOpenCurrentHallMap = false;
            getInstanceHallMapPage().Hall = CurrentHall;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    [RelayCommand]
    private async Task SaveCashRegister()
    {
        try
        {

            if (CurrentCash == null) { return; }


            if (!IsNotNull(CurrentCash.Name, CurrentCash.CashRegisterSetting?.IpAddress))
            {
                await DialogService.ShowAlertAsync("Ошибка", "Заполните все поля", "OK");
                return;
            }

            if (CurrentCash.CashRegisterType == CashRegisterType.CashRegister)
            {
                if (!IsNotNull(CurrentCash.CashRegisterSetting?.Login, CurrentCash.CashRegisterSetting?.Password))
                {
                    await DialogService.ShowAlertAsync("Ошибка", "Заполните все поля", "OK");
                    return;
                }
            }
            if (IsNew)
            {

                await realm.WriteAsync(() =>
                {

                    realm.Add(new CashRegister()
                    {

                        BrandId = CurrentBrandId,
                        CashRegisterType = CurrentCash.CashRegisterType,
                        IsActive = false,
                        IsDeleted = false,
                        Name = CurrentCash.Name,
                        SalePointId = CurrentSalePointId,
                        UserId = CurrentEmpId,
                        CashRegisterSetting = new CashRegisterSetting()
                        {
                            IpAddress = CurrentCash.CashRegisterSetting?.IpAddress,
                            Password = CurrentCash.CashRegisterSetting?.Password,
                            Login = CurrentCash.CashRegisterSetting?.Login,
                            UserId = CurrentEmpId,
                        }


                    });



                });

            }
            else
            {
                var cash = realm.Find<CashRegister>(CurrentCash.Id);
                if (cash == null) { return; }
                await realm.WriteAsync(() =>
                {


                    cash.CashRegisterType = CurrentCash.CashRegisterType;
                    cash.IsActive = false;
                    cash.IsDeleted = false;
                    cash.Name = CurrentCash.Name;

                    cash.UserId = CurrentEmpId;
                    cash.CashRegisterSetting ??= new();

                    cash.CashRegisterSetting.IpAddress = CurrentCash.CashRegisterSetting?.IpAddress;
                    cash.CashRegisterSetting.Password = CurrentCash.CashRegisterSetting?.Password;
                    cash.CashRegisterSetting.Login = CurrentCash.CashRegisterSetting?.Login;
                    cash.CashRegisterSetting.UserId = CurrentEmpId;


                });
            }

            IsNew = false;

            CurrentCash = null;
            OpenCashRegisterDetail();


        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex);
        }

    }



    [RelayCommand]
    private void GoToDetailCashRegister(CashRegister cashRegister)
    {
        try
        {
            if (cashRegister == null) return;
            realm.Write(() =>
            {


                CurrentCash = new()
                {
                    Id = cashRegister.Id,
                    BrandId = cashRegister.BrandId,
                    CashRegisterType = cashRegister.CashRegisterType,
                    IsActive = false,
                    IsDeleted = false,
                    Name = cashRegister.Name,
                    SalePointId = cashRegister.SalePointId,
                    UserId = CurrentEmpId,
                    CashRegisterSetting = new CashRegisterSetting()
                    {
                        IpAddress = cashRegister.CashRegisterSetting?.IpAddress,
                        Password = cashRegister.CashRegisterSetting?.Password,
                        Login = cashRegister.CashRegisterSetting?.Login,
                        UserId = cashRegister.UserId,
                    }
                };

                IsNew = false;
                switch (CurrentCash.CashRegisterType)
                {
                    case CashRegisterType.Printer:
                        CurrentType = "Принтер";
                        break;
                    case CashRegisterType.CashRegister:
                        CurrentType = "ККМ";
                        break;
                    case CashRegisterType.Scanner:
                        CurrentType = "Сканнер";
                        break;
                    default:
                        CurrentType = "Принтер";
                        break;



                }


            });
            IsOpenCashRegister = true;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }


    [RelayCommand]
    private async Task SetActiveCashRegister(CashRegister cash)
    {
        try
        {
            await realm.WriteAsync(() =>
            {

                cash.IsActive = !cash.IsActive;

            });

        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }




    [RelayCommand]
    private void closing_popups()
    {
        IsMenuOpened = false;
        IsEquipment = false;
        IsBookingOpened = false;
        IsReservationsOpened = false;
        IsNewTransactionOpened = false;
        IsOpenAllShifts = false;
        IsOpenCurrentHallMap = false;
        IsNotificationPopup = false;
    }

    public static class StateKey
    {
        public const string Loading = nameof(Loading);
        public const string Orders = nameof(Orders);
        public const string HallMap = nameof(HallMap);
        public const string Warehouse = nameof(Warehouse);
        public const string Checks = nameof(Checks);
        public const string OrderDetail = nameof(OrderDetail);
        public const string PayPage = nameof(PayPage);

        public const string Success = nameof(Success);
        public const string Anything = "StateKey can be anything!";
        public const string ReplaceGrid = nameof(ReplaceGrid);
        public const string NoAnimate = nameof(NoAnimate);
        public const string NotFound = nameof(NotFound);
    }
}

