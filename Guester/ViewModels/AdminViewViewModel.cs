using Guester.Resources;
using Microsoft.AppCenter.Crashes;
using Mopups.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Guester.ViewModels
{
    public partial class AdminViewViewModel : ObservableObject
    {
        [ObservableProperty]
        private string password;


        [ObservableProperty]
        bool isSuccess;

        private Realm realm;



        [RelayCommand]
        public async Task ConfirmAdmin(string param = "")
        {
            try
            {
                if (param != null)
                {
                    if (param.Equals("close"))
                    {
                        IsSuccess = false;
                        await MopupService.Instance.PopAsync();
                        return;
                    }
                }
                realm = RealmService.GetMainThreadRealm();

                IsSuccess = realm.Find<Brand>(HomePageViewModel.CurrentBrandId)?.Password == Password;

                if (!IsSuccess)
                {
                    await DialogService.ShowAlertAsync("Ошибка", "Пароль не совпадает", "OK");
                    return;
                }



                await MopupService.Instance.PopAsync();

            }
            catch (Exception ex) { Crashes.TrackError(ex); }
        }


    }
}
