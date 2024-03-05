namespace Guester.Views;

public partial class OrderDatailPage : ContentPage
{
    private OrderDetailPageViewModel vm;

    public OrderDatailPage( OrderDetailPageViewModel _vm)
	{
		InitializeComponent();
		// присваиваем  значение локальной переменной vm
		vm = _vm;
		BindingContext = vm;

		//vm.ItemsListView = itemListView;
	
	}

    private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {

    }

	protected override void OnAppearing()
	{
        base.OnAppearing();
        vm.OnAppearing();
    }
	protected override void OnDisappearing()
	{
        base.OnDisappearing();
        vm.OnDisappearing();
	}
}