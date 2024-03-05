namespace Guester.Tamplates;


[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class ChecksContent : ContentView
{

	private ChecksContentViewModel vm;

   
    public ChecksContent(ChecksContentViewModel _vm)
    {
        InitializeComponent();
        BindingContext = vm = _vm;
        vm.MakeFilter();
    }

    public ChecksContent()
    {
        InitializeComponent();
        BindingContext = vm = ServiceHelper.GetService<ChecksContentViewModel>();
        vm.ListView = listView;
        vm.MakeFilter();
        //SfListView.GroupHeaderTemplateProperty.PropertyName
    }

}
