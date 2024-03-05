using CommunityToolkit.Maui.Views;
using Microsoft.Maui.Storage;
using Mopups.Services;
using System.Xml.Linq;


namespace Guester.Widgets;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class AdminView
{
    AdminViewViewModel vm;
    private TaskCompletionSource<bool> _taskCompletionSource;
    public Task<bool> PopupClosedTask => _taskCompletionSource.Task;
    public AdminView()
    {
        InitializeComponent();
        vm = (AdminViewViewModel)BindingContext;
    
       
    
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        //vm.OnAppearing();

        _taskCompletionSource = new TaskCompletionSource<bool>();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        _taskCompletionSource.SetResult(vm.IsSuccess);
    }
}


 



