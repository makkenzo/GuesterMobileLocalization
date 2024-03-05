
using CommunityToolkit.Mvvm.Collections;
using Guester.Resources;
using Microsoft.AppCenter.Crashes;
using MvvmHelpers;
using Syncfusion.Maui.DataSource.Extensions;
using Syncfusion.Maui.ListView;
using System.Linq;

namespace Guester.ViewModels;

public partial class WarehouseContentViewModel : BaseViewModel

{
    private string searchTextProducts;
    private Realm realm;


    public ObservableRangeCollection<RemainsProduct> RemainsProducts { get; set; } = new();
    //public ObservableGroupedCollection<string, RemainsProduct> RemainsProductsGrouped { get; set; } = new();
    [ObservableProperty]
    RemainsOfWarehouse actualRemains;
    [ObservableProperty]
    string categoryTitle="";


    public string SearchTextProducts { get { return searchTextProducts; } set { searchTextProducts = value; OnPropertyChanged(nameof(SearchTextProducts)); OnSearchTextChanged(); } }
 
    public WarehouseContentViewModel()
	{
        try
        {

            realm ??= GetRealm();
            ActualRemains = realm.All<RemainsOfWarehouse>().LastOrDefault();
            getAllProducts();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }


    }
    private void getAllProducts()
    {
        ActualRemains ??= new();
        RemainsProducts.ReplaceRange( ActualRemains.RemainsProducts);
        CategoryTitle = $"{AppResources.AllProducts}";
    }

    //private async Task LoadRemains()
    //{
    //    RemainsProductsGrouped = new ObservableGroupedCollection<string, RemainsProduct>(
    //        ActualRemains.RemainsProducts
    //        .GroupBy(x=> x.DisplayName)
    //        .OrderBy(x=> x.Key)
    //        );

    //    OnPropertyChanged(nameof(RemainsProductsGrouped));
    //    await Task.CompletedTask;
    //}

    [RelayCommand]
    private void SelectCategory(GroupResult groupResult)
    {
        try
        {
            if (!IsNotNull(ActualRemains, groupResult)) return;
            /*  if (!groupResult.IsExpand)
                  return;*/


            CategoryTitle = groupResult.Key.ToString();
            RemainsProducts.ReplaceRange(ActualRemains.RemainsProducts.Where(x => x.DisplayName.Equals(groupResult.Key)).ToList());
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    [RelayCommand]
    private void ShowAll()
    {
        try
        {
            if (!IsNotNull(ActualRemains)) return;


            CategoryTitle = $"{AppResources.AllProducts}";
            RemainsProducts.ReplaceRange(ActualRemains.RemainsProducts);
        }
        catch (Exception ex) { Crashes.TrackError(ex); }

    }

    private void OnSearchTextChanged()
    {
        try
        {

            if (!IsNotNull(ActualRemains))
                return;

            if (IsNotNull(SearchTextProducts))
            {
                CategoryTitle = "...";



                RemainsProducts.ReplaceRange(
                    ActualRemains?.RemainsProducts.ToList().Where(x => x.Cost.ToString().Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase) || x.Amount.ToString().Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase)
                || x?.Product?.Name.Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase) == true)
                    );
                return;
            }
            getAllProducts();
        }
        catch (Exception ex) { Crashes.TrackError(ex); }


    }

    private bool FilterContacts(object obj)
    {
        //order.OrderDelivery.DeliveryStatus = DeliveryStatus.Success;
        var item = obj as RemainsProduct;
        if (
            item.Cost.ToString().Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase) 
            ||
            item.Amount.ToString().Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase)||
            (item?.Product?.Name.Contains(SearchTextProducts.ToLower(), StringComparison.CurrentCultureIgnoreCase) == true)
            )
            return true;
      
        else
            return false;
    }


}

