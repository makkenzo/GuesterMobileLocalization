using Guester.ViewModels;
using Mopups.Pages;

namespace Guester.Widgets;

public partial class WarningView : PopupPage
{
    private TaskCompletionSource<bool> _taskCompletionSource;
    public Task<bool> PopupClosedTask => _taskCompletionSource.Task;

    WarningViewModel vm;
    public WarningView(string title, string message, bool needconfirm)
    {
        InitializeComponent();
        vm = (WarningViewModel)BindingContext;
        vm.IsNeedConfirm = needconfirm;
        vm.Title = title;
        vm.Message = message;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _taskCompletionSource = new TaskCompletionSource<bool>();
    }

    protected override void OnDisappearing()
    {
        _taskCompletionSource.SetResult(vm.Result);
        base.OnDisappearing();
    }
}
