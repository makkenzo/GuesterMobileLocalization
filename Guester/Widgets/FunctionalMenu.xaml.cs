using CommunityToolkit.Maui.Views;
using CommunityToolkit.Mvvm.Messaging;
using Guester.ViewModels;
using Mopups.Services;

namespace Guester.Widgets;

public partial class FunctionalMenu : Popup
{
    private FunctionalMenuViewModel vm;
    public FunctionalMenu()
    {
        InitializeComponent();
        vm = new FunctionalMenuViewModel();
        BindingContext = vm;
        WeakReferenceMessenger.Default.Register<Helpers.CloseFunctionMenu>(this, (sender, message) =>
        {
            Close_();
        });

    }



   

    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();
    //    vm.OnAppearing();
    //}

    public async void Close_()
    {
        await CloseAsync();

        WeakReferenceMessenger.Default.Unregister<Helpers.CloseFunctionMenu>("Unregister");
        // await MopupService.Instance.PopAsync();
    }
}
