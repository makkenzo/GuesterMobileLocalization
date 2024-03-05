

using Syncfusion.Maui.DataSource;
using Syncfusion.Maui.ListView;

namespace Guester.Tamplates;

[XamlCompilation (XamlCompilationOptions.Compile)]
public partial class OrdersContent : ContentView
{
    OrdersContentViewModel vm;
	public OrdersContent(OrdersContentViewModel _vm)
	{
		InitializeComponent();        
        BindingContext = vm = _vm;
        vm.ListView = listView;
    }
    
    public OrdersContent()
    {
        InitializeComponent();
        BindingContext = vm = ServiceHelper.GetService<OrdersContentViewModel>();
        vm.ListView = listView;
        //SfListView.GroupHeaderTemplateProperty.PropertyName
    }

    //TODO в релизе не корректно работает пиккер
    private void Picker_SelectedIndexChanged(object sender, EventArgs e)
    {
        try
        {
            var picker = (Picker)sender;
            var order = (Orders)picker.BindingContext;
            var realm = RealmService.GetMainThreadRealm();
            if (order.OrderDelivery?.Courier == null || (Employer)picker.SelectedItem == null) return;
            realm.Write(() =>
            {
                order.OrderDelivery.Courier = (Employer)picker.SelectedItem;

            });
        }
        catch { }
    }

    //private void listView_ItemTapped(object sender, Syncfusion.Maui.ListView.ItemTappedEventArgs e)
    //{
    //    vm.GotoDetail((Orders)e.DataItem);
    //}


}
