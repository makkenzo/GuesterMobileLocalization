
using Syncfusion.Maui.ListView;

namespace Guester.Tamplates;

public partial class OrderDatail : ContentView
{

    OrderDetailPageViewModel vm;
	public OrderDatail()
	{
		InitializeComponent();
        BindingContext = vm = ServiceHelper.GetService<OrderDetailPageViewModel>();
       // vm.ListView = OrderItemList;
        vm.ItemsListView = itemListView;
        itemListView.ItemAppearing += ItemListView_ItemAppearing;
        vm.ModifiersListView = modifiersListView;
        vm.AllOrdersListView = allOrdersListView;
        vm.PromotionBonusTempateListView = PromotionBonusTempateListView;
    }



    private void ItemListView_ItemAppearing(object sender, ItemAppearingEventArgs e)
    {
        //var ls = sender as SfListView;
        //ls.Opacity = .8;
        //ls.FadeTo(1, 200, Easing.BounceIn);
       
    }

    private void DatePicker_DateSelected(object sender, DateChangedEventArgs e)
    {
		
    }

    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        //var picker = (Picker)sender;
        //var order = (Orders)picker.BindingContext;
        //var realm = RealmService.GetMainThreadRealm();
        //realm.Write(() =>
        //{
        //    order.OrderDelivery.Courier = (Employer)picker.SelectedItem;

        //});
    }
}
