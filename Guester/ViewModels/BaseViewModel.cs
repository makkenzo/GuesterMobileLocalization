using Microsoft.AppCenter.Crashes;
using System.Reflection;

namespace Guester;

public partial class BaseViewModel : ObservableObject
{
    public static string CurrentSalePointId { get => Preferences.Get(nameof(CurrentSalePointId), ""); set { Preferences.Set(nameof(CurrentSalePointId), value); } }
    public static string CurrentBrandId { get => Preferences.Get(nameof(CurrentBrandId), ""); set => Preferences.Set(nameof(CurrentBrandId), value); }
    public static string CurrentDivaceId { get => Preferences.Get(nameof(CurrentDivaceId), ""); set => Preferences.Set(nameof(CurrentDivaceId), value); }
    public static string CurrentEmpId { get => Preferences.Get(nameof(CurrentEmpId), ""); set => Preferences.Set(nameof(CurrentEmpId), value); }
    public static string CurrentPremisesId { get => Preferences.Get(nameof(CurrentPremisesId), ""); set => Preferences.Set(nameof(CurrentPremisesId), value); }
    public static string CurrentCashShiftId { get => Preferences.Get(nameof(CurrentCashShiftId), ""); set => Preferences.Set(nameof(CurrentCashShiftId), value); }
    public static string CurrentHallId { get => Preferences.Get(nameof(CurrentHallId), ""); set => Preferences.Set(nameof(CurrentHallId), value); }
    public static string AuthorName { get => Preferences.Get(nameof(AuthorName), ""); set => Preferences.Set(nameof(AuthorName), value); }
    public string SaveCulture { get => Preferences.Get(nameof(SaveCulture), ""); set => Preferences.Set(nameof(SaveCulture), value); }
    public string AppVersion
    {
        get
        {

            return App.AppVersionn();
        }
    }



    [ObservableProperty]
    protected bool isBusy;


    //[ObservableProperty]
    //private string title;

    protected Action currentDismissAction;



    // Запуск индикатора активности
    partial void OnIsBusyChanged(bool value)
    {
        try
        {
            if (value)
            {

                Task.Run(async () =>
                {
                    await Task.Delay(150);

                }).Wait();


                currentDismissAction = DialogService.ShowActivityIndicator();
            }
            else
            {
                currentDismissAction?.Invoke();
                currentDismissAction = null;
            }
        }
        catch (Exception ex) { Crashes.TrackError(ex); }
    }


    public bool IsNotNull(params string[] values) => !values.Any(string.IsNullOrWhiteSpace);
    public bool IsNotNull(params object[] values) => !values.Any(x => x == null);

    public Realm GetRealm() => RealmService.GetMainThreadRealm();

    public async void CheckResponceStatus(ReceiptResponce response)
    {
        switch (response.Error)
        {
            case "NoLoginedUser":
                await DialogService.ShowAlertAsync("Не выполнена авторизация на ККМ.", "Повторите попытку.", "OK");
                break;
            case "ZreportIsNotOpen":
                await DialogService.ShowAlertAsync("Z-Отчет ККМ", "Не возможно открыть Z-Отчет.", "OK");
                break;
            case "PaperEnded":
                await DialogService.ShowAlertAsync("Закончилась бумага в ККМ.", "Чек не напечатан, замените бумагу и повторите попытку.", "OK");
                break;
            case "FailToGetSaleFromServer":
                await DialogService.ShowAlertAsync("Ошика соединения в ККМ.", "Не возможно получить заказ с сервера, проверте соединение с интернетом.", "OK");
                break;
            case "ReceiptStoreDaysLimitExceeded":
                await DialogService.ShowAlertAsync("Не выгружены оффлайн чеки.", "Выгрузите оффлайн чеки, и  повторите попытку", "OK");
                break;
            case "BadRequest":
                await DialogService.ShowAlertAsync("Системная ошибка в ККМ.", "Чек не напечатан, требуется перезапустить ККМ и повторите попытку.", "OK");
                break;
            case "PSIDError":
                await DialogService.ShowAlertAsync("Ошибка ИКПУ.", "ИКПУ не найден, заполните ИКПУ.", "OK");
                break;
            default:
                await DialogService.ShowAlertAsync("Ошибка.", response.Error, "OK");
                break;
        }
    }
}
