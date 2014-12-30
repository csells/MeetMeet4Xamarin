using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;

namespace Xamarin.Forms.Labs.Controls {
  public class MyAutoCompleteView : ContentView {
    private Entry entText;
    private Button btnSearch;
    private ListView lstSugestions;
    private StackLayout stkBase;

    public MyAutoCompleteView() {

      //InitializeComponent();
      stkBase = new StackLayout();
      var innerLayout = new StackLayout();
      entText = new Entry() {
        HorizontalOptions = LayoutOptions.FillAndExpand,
        VerticalOptions = LayoutOptions.Start
      };
      btnSearch = new Button() {
        VerticalOptions = LayoutOptions.Center,
        Text = "Search"
      };

      lstSugestions = new ListView() {
        HeightRequest = 250,
        HasUnevenRows = true
      };

      innerLayout.Children.Add(entText);
      innerLayout.Children.Add(btnSearch);
      stkBase.Children.Add(innerLayout);
      stkBase.Children.Add(lstSugestions);

      Content = stkBase;


      entText.TextChanged += (s, e) => {
        Text = e.NewTextValue;
      };
      btnSearch.Clicked += (s, e) => {
        if (SearchCommand != null && SearchCommand.CanExecute(Text))
          SearchCommand.Execute(Text);
      };
      lstSugestions.ItemSelected += (s, e) => {
        entText.Text = GetSearchString(e.SelectedItem);

        AvailableSugestions.Clear();
        ShowHideListbox(false);
        SelectedCommand.Execute(e);
        if (ExecuteOnSugestionClick
           && SearchCommand != null && SearchCommand.CanExecute(Text)) {
          SearchCommand.Execute(e);
        }

      };
      AvailableSugestions = new ObservableCollection<object>();
      this.ShowHideListbox(false);
      lstSugestions.ItemsSource = this.AvailableSugestions;
      //lstSugestions.ItemTemplate = this.SugestionItemDataTemplate;
    }

    private void ShowHideListbox(bool show) {
      lstSugestions.IsVisible = show;
    }

    public Entry TextEntry {
      get {
        return entText;
      }
    }

    public ListView ListViewSugestions {
      get {
        return lstSugestions;
      }
    }

    public ObservableCollection<object> AvailableSugestions {
      get;
      private set;
    }


    #region Bindable Properties

    public static readonly BindableProperty SugestionsProperty =
        BindableProperty.Create<MyAutoCompleteView, ObservableCollection<object>>
    (p => p.Sugestions, null);

    public ObservableCollection<object> Sugestions {
      get { return (ObservableCollection<object>)GetValue(SugestionsProperty); }
      set { SetValue(SugestionsProperty, value); }
    }

    public static readonly BindableProperty TextProperty =
        BindableProperty.Create<MyAutoCompleteView, string>(p => p.Text, "", BindingMode.TwoWay, null,
            new BindableProperty.BindingPropertyChangedDelegate<string>(TextChanged), null, null);

    public string Text {
      get { return (string)GetValue(TextProperty); }
      set { SetValue(TextProperty, value); }
    }

    static void TextChanged(BindableObject obj, string oldPlaceHolderValue, string newPlaceHolderValue) {
      var control = (obj as MyAutoCompleteView);

      control.btnSearch.IsEnabled = !string.IsNullOrEmpty(newPlaceHolderValue);
      string cleanedNewPlaceHolderValue = Regex.Replace((newPlaceHolderValue ?? "").ToLowerInvariant(), @"\s+", string.Empty);
      if (!string.IsNullOrEmpty(cleanedNewPlaceHolderValue) && control.Sugestions != null) {
        var filteredsugestions = control.Sugestions.Where(x => {
          return Regex.Replace(GetSearchString(x).ToLowerInvariant(), @"\s+", string.Empty).Contains(cleanedNewPlaceHolderValue);
        }).OrderByDescending(x => {
          return Regex.Replace(GetSearchString(x).ToLowerInvariant(), @"\s+", string.Empty).StartsWith(cleanedNewPlaceHolderValue);
        }).ToArray();

        control.AvailableSugestions.Clear();

        foreach (var item in filteredsugestions) {
          control.AvailableSugestions.Add(item);
        }
        if (control.AvailableSugestions.Count > 0) {
          control.ShowHideListbox(true);
        }
      }
      else {
        if (control.AvailableSugestions.Count > 0) {
          control.AvailableSugestions.Clear();
          control.ShowHideListbox(false);
        }
      }
    }

    public static string GetSearchString(object x) {
      AutoCompleteSearchObject itm;
      if ((itm = x as AutoCompleteSearchObject) != null) {
        return itm.StringToSearchBy();
      }
      else {
        return x.ToString();
      }
    }

    public static readonly BindableProperty PlaceholderProperty =
        BindableProperty.Create<MyAutoCompleteView, string>(
            p => p.Placeholder, "", BindingMode.TwoWay, null,
            new BindableProperty.BindingPropertyChangedDelegate<string>(PlaceHolderChanged));

    public string Placeholder {
      get { return (string)GetValue(PlaceholderProperty); }
      set { SetValue(PlaceholderProperty, value); }
    }

    static void PlaceHolderChanged(BindableObject obj, string oldPlaceHolderValue, string newPlaceHolderValue) {
      (obj as MyAutoCompleteView).TextEntry.Placeholder = newPlaceHolderValue;
    }

    public static readonly BindableProperty ShowSearchProperty =
        BindableProperty.Create<MyAutoCompleteView, bool>(
            p => p.ShowSearchButton, true, BindingMode.TwoWay, null, new BindableProperty.BindingPropertyChangedDelegate<bool>(ShowSearchChanged));

    public bool ShowSearchButton {
      get { return (bool)GetValue(ShowSearchProperty); }
      set { SetValue(ShowSearchProperty, value); }
    }

    static void ShowSearchChanged(BindableObject obj, bool oldShowSearchValue, bool newShowSearchValue) {
      (obj as MyAutoCompleteView).btnSearch.IsVisible = newShowSearchValue;
    }

    public static readonly BindableProperty SearchCommandProperty =
        BindableProperty.Create<MyAutoCompleteView, ICommand>(p => p.SearchCommand, null);

    public ICommand SearchCommand {
      get { return (ICommand)GetValue(SearchCommandProperty); }
      set { SetValue(SearchCommandProperty, value); }
    }

    public static readonly BindableProperty SelectedCommandProperty =
        BindableProperty.Create<MyAutoCompleteView, ICommand>(
            p => p.SelectedCommand, null);

    public ICommand SelectedCommand {
      get { return (ICommand)GetValue(SelectedCommandProperty); }
      set { SetValue(SelectedCommandProperty, value); }
    }

    public static readonly BindableProperty SugestionItemDataTemplateProperty =
        BindableProperty.Create<MyAutoCompleteView, DataTemplate>(p => p.SugestionItemDataTemplate, null,
            BindingMode.TwoWay, null,
            new BindableProperty.BindingPropertyChangedDelegate<DataTemplate>(SugestionItemDataTemplateChanged), null, null);

    public DataTemplate SugestionItemDataTemplate {
      get { return (DataTemplate)GetValue(SugestionItemDataTemplateProperty); }
      set { SetValue(SugestionItemDataTemplateProperty, value); }
    }

    static void SugestionItemDataTemplateChanged(BindableObject obj, DataTemplate oldShowSearchValue, DataTemplate newShowSearchValue) {
      (obj as MyAutoCompleteView).lstSugestions.ItemTemplate = newShowSearchValue;
    }

    public static readonly BindableProperty SearchBackgroundColorProperty =
        BindableProperty.Create<MyAutoCompleteView, Color>(p => p.SearchBackgroundColor, Color.Red,
            BindingMode.TwoWay, null,
            new BindableProperty.BindingPropertyChangedDelegate<Color>(SearchBackgroundColorChanged), null, null);

    public Color SearchBackgroundColor {
      get { return (Color)GetValue(SearchBackgroundColorProperty); }
      set { SetValue(SearchBackgroundColorProperty, value); }
    }

    static void SearchBackgroundColorChanged(BindableObject obj, Color oldValue, Color newValue) {
      (obj as MyAutoCompleteView).stkBase.BackgroundColor = newValue;
    }

    public static readonly BindableProperty SugestionBackgroundColorProperty =
        BindableProperty.Create<MyAutoCompleteView, Color>(p => p.SugestionBackgroundColor, Color.Red,
            BindingMode.TwoWay, null,
            new BindableProperty.BindingPropertyChangedDelegate<Color>(SugestionBackgroundColorChanged), null, null);

    public Color SugestionBackgroundColor {
      get { return (Color)GetValue(SugestionBackgroundColorProperty); }
      set { SetValue(SugestionBackgroundColorProperty, value); }
    }

    static void SugestionBackgroundColorChanged(BindableObject obj, Color oldValue, Color newValue) {
      (obj as MyAutoCompleteView).lstSugestions.BackgroundColor = newValue;
    }

    public static readonly BindableProperty ExecuteOnSugestionClickProperty =
        BindableProperty.Create<MyAutoCompleteView, bool>(p => p.ExecuteOnSugestionClick, false);

    public bool ExecuteOnSugestionClick {
      get { return (bool)GetValue(ExecuteOnSugestionClickProperty); }
      set { SetValue(ExecuteOnSugestionClickProperty, value); }
    }
    #endregion
  }

  public interface MyAutoCompleteSearchObject {
    string StringToSearchBy();
  }
}
