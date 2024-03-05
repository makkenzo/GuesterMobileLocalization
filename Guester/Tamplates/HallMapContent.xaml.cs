namespace Guester.Tamplates;

public partial class HallMapContent : ContentView
{
    private HallMapContentViewModel vm;
    public HallMapContent()
	{
		InitializeComponent();
        vm = HomePageViewModel.getInstanceHallMapPage();

        BindingContext = vm;

        vm.HallHolst = HallHolst;
        // vm.Popup = sfPopup;
        vm.OnAppearing();
    }

    //protected override void OnAppearing()
    //{
    //    base.OnAppearing();
    //    vm.OnAppearing();


    //}

    //protected override void OnDisappearing()
    //{
    //    base.OnDisappearing();
    //    vm.OnDisappearing();
    //}
}

