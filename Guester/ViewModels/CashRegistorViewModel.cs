using Guester.Resources;
using Microsoft.AppCenter.Crashes;
using Mopups.Services;


namespace Guester.ViewModels
{
	public partial class CashRegistorViewModel : ObservableObject
	{
        [ObservableProperty]
        string title = "", comment;


		[ObservableProperty]
		decimal summMoney = 0m;

        [ObservableProperty]
        bool canClose;
        public CashRegisterPopupData CashRegisterPopupData { get; set; }

        public CashRegistorViewModel()
		{
		}

		[RelayCommand]
		public async Task ConfirmCash(string param="")
		{
			try
			{
				if (param != null)
					if (CanClose && param.Equals("close"))
						goto Close;

				if (SummMoney < 0)
				{
					await DialogService.ShowToast(AppResources.EnterSum);
					return;
				}

				CashRegisterPopupData ??= new();
				CashRegisterPopupData.Comment = Comment;
				CashRegisterPopupData.TotalSum = SummMoney;

			Close:
				await MopupService.Instance.PopAsync();
			}
            catch (Exception ex) { Crashes.TrackError(ex); }
        }
	}
}

