namespace Guester.Tamplates;

public partial class WarehouseContent : ContentView
{
	WarehouseContentViewModel vm;
    public WarehouseContent()
	{
		InitializeComponent();
		BindingContext = vm = ServiceHelper.GetService<WarehouseContentViewModel>();
	}

	public WarehouseContent(WarehouseContentViewModel _vm)
	{
        InitializeComponent();
        BindingContext = vm = _vm;
    }
}
