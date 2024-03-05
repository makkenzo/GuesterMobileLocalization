using Mopups.Pages;
using Mopups.Services;

namespace Guester.Widgets;

public partial class BusyMopup :PopupPage
{
	public BusyMopup()
	{
		InitializeComponent();
	}

	public async void Close()
    {
		try
		{
            await MopupService.Instance.PopAsync();
        }
		catch (Exception)
		{

		}
    }
}