
using Guester.Resources;
using Microsoft.AppCenter.Crashes;
using Microsoft.Maui.Controls.Shapes;
using MvvmHelpers;
using System.Reflection;
using static Guester.ViewModels.HomePageViewModel;

namespace Guester.ViewModels;

public partial class HallMapContentViewModel : BaseViewModel
{
    private Realm realm;
    //[ObservableProperty]
    //private User currentUser;

    [ObservableProperty]
    Premises hall;

    [ObservableProperty]
    IQueryable<Premises> halls;

    //[ObservableProperty]
    //private IQueryable<Table> tables ;

    [ObservableProperty]
    private bool isAddNewOrder;

    [ObservableProperty]
    private int popupPosX, popupPosY;

    [ObservableProperty]
    AbsoluteLayout hallHolst;


    private Orders createOrder;

    private HomePageViewModel homePage;



    public HallMapContentViewModel()
    {

        try
        {
            homePage = HomePageViewModel.getInstance();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

    internal void OnAppearing()
    {
        try
        {
            closing_popups();
            realm = GetRealm();


            Hall = homePage.CurrentHall;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }


    }




    [RelayCommand]
    private void OpenHalMapPopup() => homePage.IsOpenCurrentHallMap = !homePage.IsOpenCurrentHallMap;









    [RelayCommand]
    private async Task TapedInTable(Table currentTable)
    {
        try
        {
            if (!IsNotNull(currentTable)) return;

            var brand = realm.Find<Brand>(CurrentBrandId);
            var detail = realm.All<Detail>().ToList().Where(x => !x.IsDeleted
                && x.Name != string.Empty
                && x.OnTerminal
                && x.OrderType == OrderType.InTheInstitution
                && x.SalesPointsId.ToList().Any(x => x.Id == CurrentSalePointId)).FirstOrDefault();
            createOrder = new Orders
            {
                CreationDate = DateTime.Now,
                Detail = detail,

                ShiftId = CurrentCashShiftId,
                OrderStatus = OrderStatus.New,
                OrderReceipt = new()
                {

                    CashierId = CurrentEmpId,
                    Table = null,

                },
                Name = AppResources.InEnvy,
                SalePointId = CurrentSalePointId,
                OrderDelivery = null

            };

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
            if (homePage.CurrentBrand.IsGuestsCount)
            {
                createOrder.GuestCount = 1;
                await SaveOrder(createOrder);
            }
            else
            {
                OpenPopup(currentTable.PosX, currentTable.PosY, currentTable.Height, currentTable.Width);
            }



        }
        catch (Exception ex) { Crashes.TrackError(ex); }
        //Popup.Show(table.PosX + table.Width / 2, table.PosY + table.Height + 100);

    }

    private void OpenPopup(int _posX, int _posY, int _height, int _width)
    {
        IsAddNewOrder = true;
        PopupPosX = _posX + ((_width / 2) - 115);
        PopupPosY = _posY + ((_height / 2) + 15);
    }

    [RelayCommand]
    private async void SeletGuestCount(string guest_count)
    {
        try
        {
            if (!IsNotNull(createOrder)) return;
            homePage.CurrentState = StateKey.Loading;
            int.TryParse(guest_count, out var count);
            if (count == 0) count++;
            realm.Write(() =>
            {
                createOrder.GuestCount = count;
            });
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
                homePage.CurrentState = StateKey.HallMap;

                closing_popups();
                return;
            }


            await realm.WriteAsync(() =>
            {

                order.CreaterEmpId = CurrentEmpId;
                order.Logs.Add(new Logs { CreaterDate = DateTimeOffset.Now, Title = AppResources.WasCreateNewOrder, DeviceName = CurrentDivaceId });
                for (int i = 0; i < order.GuestCount; i++)
                {
                    order.OrderSales.Add(new OrderSale { GuestIndex = i + 1 });

                    order.Logs.Add(new Logs { CreaterDate = DateTimeOffset.Now, Title = $"{AppResources.GuestAdded} {AppResources.Guest} {i + 1}", DeviceName = CurrentDivaceId });
                }


                realm.Add(order);

            });


            HomePageViewModel.getInstanceOrderContentPage().MakeFilter();
            homePage.CurrentState = StateKey.OrderDetail;
            var orderPage = HomePageViewModel.getInstanceOrderDetailPage();

            closing_popups();


            orderPage.Order = order;

            orderPage.OnAppearing();
            IsAddNewOrder = false;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }


    [RelayCommand]
    private void closing_popups()
    {
        IsAddNewOrder = false;

    }

    [RelayCommand]
    public void AddOrEditOrder()
    {
        //await AppShell.Current.GoToAsync($"{nameof(AddOrderPage)}");
    }


}

