using Guester.Resources;
using Microsoft.AppCenter.Crashes;
using System.Net;
using System.Security.Cryptography;

namespace Guester.ViewModels;

public partial class DeviceAuthViewModel : BaseViewModel
{
    [ObservableProperty]
    string login,password;

    private Realm realm;

    internal async void OnAppearing()
    {
        GetData();
    }

    public DeviceAuthViewModel() 
    {
        
      
    }    

    private void GetData()
    {
        Login = Preferences.Get(nameof(Login), "");
        Password = Preferences.Get(nameof(Password), "");
    }
    private async  void GoToLoginPage()
    {
        try
        {
            Preferences.Set(nameof(Login), Login);
            Preferences.Set(nameof(Password), Password);


            App.Current.MainPage = new AppShell();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    [RelayCommand]
    public async  Task  Authorize()
    {
        if (IsBusy)
            return;

        if (!IsNotNull(Login, Password))
        {
            await DialogService.ShowToast($"{AppResources.Field} {(!IsNotNull(Login)?AppResources.Login:AppResources.Pswd)} {AppResources.FieldRequerd}");
            return;
        }

     
        try
        {
            IsBusy = true;
            await RealmService.Init();
            var result = await RestService.LoginAsync(Login, Password);

            if (!IsNotNull(result))
                throw new Exception("wrong_pswd");
            

            CurrentSalePointId = result["salePointId"];
            CurrentBrandId = result["brandId"];

            if(!IsNotNull(CurrentSalePointId, CurrentBrandId)) throw new Exception("wrong_pswd");
        
            await RealmService.LoginAsync();

            realm ??= GetRealm();

            CurrentCashShiftId = realm.All<Shift>().FirstOrDefault(doc => !doc.IsClosed).Id;
            GoToLoginPage();

        }
        catch (Exception ex)
        {
            var errorMessage = ex.Message switch
            {
                "wrong_pswd" => AppResources.WrongPswdError,
                "can't_create_cash_register" => AppResources.CantCreateCashRegisterError,
                _ => null
            };

            if (IsNotNull(errorMessage))
                await DialogService.ShowToast(errorMessage);
            else
               Crashes.TrackError(ex); 
        }
        finally
        {
            IsBusy = false;
    
        }

       
    }



}


