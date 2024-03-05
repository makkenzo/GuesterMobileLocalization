

using System.Diagnostics.Metrics;

namespace Guester.Tamplates;

public partial class HeaderNavButton :ContentView
{
    public static readonly BindableProperty TitleProperty =	BindableProperty.Create(nameof(Title), typeof(string), typeof(HeaderNavButton), string.Empty);
    public static readonly BindableProperty CounterProperty = BindableProperty.Create(nameof(Counter), typeof(string), typeof(HeaderNavButton), string.Empty);
    public static readonly BindableProperty FontProperty = BindableProperty.Create(nameof(Font), typeof(string), typeof(HeaderNavButton), string.Empty);
    public static readonly BindableProperty IsCounterVisibleProperty = BindableProperty.Create(nameof(IsCounterVisible), typeof(bool), typeof(HeaderNavButton), true);
    public static readonly BindableProperty TextColorProperty = BindableProperty.Create(nameof(IsCounterVisible), typeof(string), typeof(HeaderNavButton), "White");



    public string Title
    {
        get => GetValue(TitleProperty) as string;
        set => SetValue(TitleProperty, value);
    }

    public string Counter
    {
        get => (string)GetValue(CounterProperty);
        set => SetValue(CounterProperty, value);
    }

    public string TextColor
    {
        get => (string)GetValue(TextColorProperty);
        set => SetValue(TextColorProperty, value);
    }
     public string Font
    {
        get => (string)GetValue(FontProperty);
        set => SetValue(FontProperty, value);
    }

    public bool IsCounterVisible
    {
        get => (bool)GetValue(IsCounterVisibleProperty);
        set => SetValue(IsCounterVisibleProperty, value);
    }




    public HeaderNavButton()
	{
		InitializeComponent();
	}
}
