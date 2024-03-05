using CommunityToolkit.Maui.Core;
using Guester.Resources;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using Realms.Exceptions;
using Syncfusion.Maui.DataSource.Extensions;
using System.Globalization;
using System.Security.Cryptography;
using static Realms.ThreadSafeReference;

namespace Guester.ViewModels;

public partial class LoginViewModel : BaseViewModel
{

	private Realm realm;

	private Shift CurrentCashShift;
    private Employer user;
    private IQueryable<Employer> users;

    [ObservableProperty]
	string code = "";

	[ObservableProperty]
	bool circle1, circle2, circle3, circle4;



	internal async void OnAppearing()
	{
        try
        {

            realm ??= GetRealm();

            //await RealmService.SetSubscription(realm, SubscriptionType.All);


            if (!IsNotNull(CurrentBrandId, CurrentSalePointId))
                App.Current.MainPage = new DeviceAuthPage(ServiceHelper.GetService<DeviceAuthViewModel>());

            users = realm.All<Employer>();

            Code = string.Empty;
            UpdateCircles();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }

	[RelayCommand]
	public async Task Authorization()
	{
		if (Code.Length == 4)
		{

            IsBusy = true;
			await Task.Delay(250);
            try
			{
                realm ??= GetRealm(); 
                user = users.FirstOrDefault(x =>  x.PinCode == Code && !x.IsDeleted);
                CurrentCashShift = realm.Find<Shift>(CurrentCashShiftId);

                if (user is null)
				    throw new Exception("invalid_code");


                if (user.Post?.IsTerminal == false)
                    throw new Exception("don't access work");

				CurrentEmpId = user.Id;
				AuthorName = user.FullName;

                if (IsNotNull(SaveCulture))
                {
                    switch (SaveCulture)
                    {
                        case "uz-Latn":
                            AppResources.Culture = new CultureInfo("uz-Latn");
                            Thread.CurrentThread.CurrentCulture = new CultureInfo("uz-Latn");

                            break;

                        case "en-US":
                            AppResources.Culture = new CultureInfo("en-US");
                            Thread.CurrentThread.CurrentCulture = new CultureInfo("en-US");

                            break;
                        default:
                            AppResources.Culture = new CultureInfo("ru-Rus");
                            Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-Rus");
                            break;

                    }
                }

                if (!IsNotNull(CurrentCashShift)||CurrentCashShift?.IsClosed==true)
                {

                    CurrentCashShift = await CreateCashRegisterShift();
                    CurrentCashShiftId=CurrentCashShift.Id;
                    IsBusy = false;

                    _ = AppShell.Current.GoToAsync($"//{nameof(HomePage)}?IsOpenPopup=true");
                    return;
                }
              
                if ( !CurrentCashShift.PersonalShifts.ToList().Any(x => x.Employer.Equals( user )&& !x.IsClosed))
                {
                    await realm.WriteAsync(() => {

                        CurrentCashShift.PersonalShifts.Add(
                        new()
                        {
                            Employer = user
                        });


                    });
                }
                

                IsBusy = false;


                await AppShell.Current.GoToAsync($"//{nameof(HomePage)}");


            }
            catch(RealmException ex) { }
            catch (Exception ex)
			{

				Code = "";
                UpdateCircles();
                IsBusy = false;

                if (ex.Message.Equals("invalid_code"))
                    await DialogService.ShowToast($"{Resources.AppResources.InvalidCode}");     
                else if (ex.Message.Equals("open_shift"))
                    await DialogService.ShowToast(AppResources.NoFindCashRegister);
                else if (ex.Message.Contains("access work"))
                    await DialogService.ShowToast($"{AppResources.YouDontWorkTerminal}");
                else
                {
                    await DialogService.ShowToast($"{AppResources.FailedLogin}",ToastDuration.Long);
                    Crashes.TrackError(ex);
                }

            }
			finally
			{
                IsBusy = false;
            }

        }

	}

    [RelayCommand]
	public void KeyInput(string parm)
	{
        try
        {
            if (parm == "back" && Code.Length > 0)
            {
                Code = Code.Substring(0, Code.Length - 1);
                UpdateCircles();
                return;
            }
            if (parm == "=")
            {
                Code = "";
                UpdateCircles();
                return;
            }

            if (parm.Length != 1)
                return;

            if (Code.Length < 4)
                Code += parm;
            UpdateCircles();
            return;
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }



	private async void UpdateCircles()
	{
		Circle1 = Code.Length > 0;
		Circle2 = Code.Length > 1;
		Circle3 = Code.Length > 2;
		Circle4 = Code.Length > 3;
		if (Circle4)
		{
			await Task.Delay(50);
			await Authorization();
		}
	}


    private async Task<Shift> CreateCashRegisterShift()
    {
        using (var transaction= realm.BeginWrite())
        {
            try
            {
                if (!IsNotNull(CurrentBrandId, CurrentSalePointId))
                {
                    throw new ArgumentNullException("sale_point_id or current_brand_id is null");
                }
                Employer emp = realm.Find<Employer>(CurrentEmpId);

                var cashRegisterShift = new Shift()
                {
                    BrandId = CurrentBrandId,
                    SalePointId = CurrentSalePointId,
                    IsClosed = false,   
                };
          

                PersonalShift persShift = new()
                {
                    Employer = emp,
                };

                var paymentMethods=realm.All<PaymentMethod>()
                    .ToList()
                    .Where(x => x.PaymentMethodSalesPoints.Any(x => x.SalesPointId == CurrentSalePointId))
                    .Select(x=>new ShiftPayment() 
                    { 
                        PaymentMethodId=x.Id, 
                        Sum="0",
                    })
                    .ToList();

                paymentMethods.ForEach(cashRegisterShift.ShiftPayments.Add);

                cashRegisterShift.PersonalShifts.Add(persShift);

                realm.Add(cashRegisterShift);
 

                if (transaction.State == TransactionState.Running)
                {
                  await transaction.CommitAsync();
                }
                return cashRegisterShift;
            }
            catch (ArgumentNullException ex)
            {
                if (transaction.State != TransactionState.RolledBack &&
                transaction.State != TransactionState.Committed)
                {
                    transaction.Rollback();

                }
                throw new ArgumentNullException(ex.Message);
            }
            catch (Exception ex)
            {
                if (transaction.State != TransactionState.RolledBack &&
                   transaction.State != TransactionState.Committed)
                {
                    transaction.Rollback();
                  
                }
                throw new Exception("open_shift");
            }
         
        

        }
    }




    [RelayCommand]
    private void LogOut()
    {
        try
        {
            CurrentEmpId = string.Empty;
            CurrentBrandId = string.Empty;
            CurrentSalePointId = string.Empty;
            CurrentDivaceId = string.Empty;

            App.Current.MainPage = new DeviceAuthPage(ServiceHelper.GetService<DeviceAuthViewModel>());
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }
























    //private string Decrypt(string cipherText)
    //{
    //	try
    //	{
    //		using (Aes aesAlg = Aes.Create())
    //		{
    //			aesAlg.Key = Encoding.UTF8.GetBytes(_key);
    //			aesAlg.IV = Encoding.UTF8.GetBytes(iv);

    //			byte[] cipherBytes = Convert.FromBase64String(cipherText);

    //			ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

    //			using (MemoryStream msDecrypt = new MemoryStream(cipherBytes, 0, cipherBytes.Length))
    //			{
    //				using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
    //				{
    //					using (StreamReader srDecrypt = new StreamReader(csDecrypt))
    //					{

    //						return srDecrypt.ReadToEnd();
    //					}
    //				}
    //			}
    //		}
    //	}
    //	catch (Exception)
    //	{
    //		return "";
    //	}
    //}


    #region FieldData
    //   private void field_data()
    //{
    //       #region Field orderData
    //       //			realm.Write(() =>
    //       //{
    //       //	for (int i = 35; i < 40; i++)
    //       //	{
    //       //		realm.Add(new Orders { Name = $"order{i}", CreationDate = DateTime.Now.AddDays(-1), OrderReceipt = new() { Table = new Table { Name = $"123 {i}" } }, Number = i, SalesPointId = CurrentSalePointId, OrderType = OrderType.InTheInstitution, OrderStatus = OrderStatus.New });
    //       //	}
    //       //});

    //       //		var users = realm.All<Employer>().ToList();
    //       #endregion

    //       #region Field menu items
    //       //await realm.WriteAsync(() =>
    //       //	 {
    //       //	 //for (int i = 25; i < 40; i++)
    //       //	 //{
    //       //	 var menu = new Menu { Name = "New MEnu Test", BrandId = CurrentBrandId };
    //       //	 var sale = realm.All<SalesPoint>().FirstOrDefault();

    //       //                     menu.SalePoints.Add(sale);
    //       //			 var p = new Product
    //       //			 {
    //       //				 TotalSum = 200,
    //       //				 Name = $"Product Test{1}",
    //       //				 BrandId = CurrentBrandId,


    //       //			 };
    //       //			 var it1 = new Items
    //       //			 {
    //       //				 Name = $"Product Test {1}",
    //       //				 Picture = "",
    //       //				 Product = p,
    //       //				 Price = 200,
    //       //				 IsActive = true,
    //       //				 IsMain = true

    //       //			 };
    //       //			 var it2 = new Items
    //       //			 {
    //       //				 Name = $"Category {1}",
    //       //				 Picture = "",
    //       //				 Product = p,
    //       //				 Price = 200,
    //       //				 IsActive = true,
    //       //				 IsCategory = true,
    //       //				 ParentItem = it1,


    //       //			 };
    //       //			 menu.Items.Add(it1);
    //       //			 menu.Items.Add(it2);

    //       //			 realm.Add(menu);
    //       //			 realm.Add(p);
    //       //			 realm.Add(it2);
    //       //			 realm.Add(it1);

    //       //		 //}
    //       //	 });
    //       #endregion

    //       #region Field Data
    //       //await realm.WriteAsync(() =>
    //       //{
    //       //	realm.Add(new Category { BrandId = CurrentBrandId });
    //       //});

    //       //await realm.WriteAsync(() =>
    //       //{
    //       //	var p = realm.All<Product>().FirstOrDefault(x=>x.Id== "653a3f473a41b5f1bda859ea");


    //       //	//var menu = new Menu();
    //       //	//menu.Items.Add(new Items { Name = "123Test", Description = "123", IsMain = true, Product = p, Price = 200 };
    //       //	realm.Add(new Items { Name = p.Name, Description = "Хлебб", Picture=p.Picture ,  IsMain = true, Product = p, Price = 200, IsActive = true}) ;
    //       //});


    //       //await realm.WriteAsync(() =>
    //       //{
    //       //	realm.Add(new Client {  BrandId = CurrentBrandId, FullName="Client full name"  });
    //       //});

    //       //var categories = realm.All<Category>().ToList();
    //       #endregion

    //   }

    #endregion



}


