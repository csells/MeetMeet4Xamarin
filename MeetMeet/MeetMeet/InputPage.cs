using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Xamarin.Forms;
using Xamarin.Forms.Labs.Controls;

namespace MeetMeet {
  class InputModel : INotifyPropertyChanged {
    Geocode currentLocation;
    string currentAddress;
    ObservableCollection<object> suggestions = new ObservableCollection<object>();
    Exception currentLocationException;

    public Exception CurrentLocationException {
      get { return this.currentLocationException; }
      set {
        if (this.currentLocationException != value) {
          this.currentLocationException = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("CurrentLocationDisplay");
        }
      }
    }

    public Geocode CurrentLocation {
      get { return this.currentLocation; }
      set {
        if (this.currentLocation != value) {
          this.currentLocation = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("CurrentLocationDisplay");
        }
      }
    }

    public string CurrentAddress {
      get { return this.currentAddress; }
      set {
        if (this.currentAddress != value) {
          this.currentAddress = value;
          NotifyPropertyChanged();
          NotifyPropertyChanged("CurrentLocationDisplay");
        }
      }
    }

    public string CurrentLocationDisplay {
      get {
        if (this.currentLocationException != null) {
          return this.currentLocationException.Message;
        }

        var loc = this.currentLocation;
        return
          loc == null
          ? "(loading...)"
          : string.IsNullOrWhiteSpace(this.currentAddress)
            ? string.Format("({0:F}, {1:F})", loc.Latitude, loc.Longitude)
            : string.Format("{2} ({0:F}, {1:F})", loc.Latitude, loc.Longitude, this.currentAddress);
      }
    }

    public ObservableCollection<object> Suggestions {
      get { return this.suggestions; }
    }

    public void SetSuggestions(IEnumerable<string> suggestions) {
      this.suggestions.Clear();
      if (suggestions == null) { return; }
      foreach (var suggestion in suggestions) { this.suggestions.Add(suggestion); }
    }

    public event PropertyChangedEventHandler PropertyChanged;
    void NotifyPropertyChanged([CallerMemberName] String propertyName = "") {
      Debug.Assert(!string.IsNullOrWhiteSpace(propertyName));
      if (PropertyChanged != null) {
        PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
      }
    }
  }

  class InputPage : ContentPage {
    Geocoder geocoder = new Geocoder();
    InputModel model = new InputModel();

    public InputPage() {
      this.Padding = 20;
      this.BindingContext = model;

      var loc1 = new Label();
      loc1.SetBinding(Label.TextProperty, new Binding("CurrentLocationDisplay"));

      // from https://github.com/XLabs/Xamarin-Forms-Labs/wiki/AutoComplete
      var loc2 = new AutoCompleteView {
        Placeholder = "enter location",
        ShowSearchButton = false,
        Sugestions = model.Suggestions,
        SelectedCommand = new Command(() => { }),
      };

      var modes = new[] {
        new { Name = "Coffee", Keyword = "coffee" },
        new { Name = "Food", Keyword = "restaurant" },
        new { Name = "Drink", Keyword = "bar" },
        new { Name = "Romance", Keyword = "hotel" },
      };

      var modePicker = new Picker();
      foreach (var mode in modes) { modePicker.Items.Add(mode.Name); }
      modePicker.SelectedIndex = 0;

      var okButton = new Button {
        Text = "Search the Middle",
        HorizontalOptions = LayoutOptions.End,
        WidthRequest = 250,
      };

      var progressIndicator = new ActivityIndicator();
      var progressText = new Label() { XAlign = TextAlignment.Center };

      Content = new StackLayout {
        Children = {
          new Label { Text = "Your Location:" },
          loc1,
          new Label { Text = "Their Location:" },
          loc2,
          new Label { Text = "Mode:" },
          modePicker,
          okButton,
          progressIndicator,
          progressText
        },
      };

      loc2.PropertyChanged += async (sender, e) => {
        if (e.PropertyName == "Text") {
          if (loc2.Text.Length > 2) {
            Debug.WriteLine("getting suggestions for: '{0}'", loc2.Text);
            var suggestions = (await this.geocoder.GetPlacesAutocomplete(loc2.Text)).ToArray();
            this.model.SetSuggestions(suggestions);
            Debug.WriteLine("got {0} suggestions for: '{1}'", suggestions.Length, loc2.Text);
          }
          else {
            Debug.WriteLine("clearing suggestions for: '{0}'", loc2.Text);
            this.model.SetSuggestions(null);
          }
        }
      };

      okButton.Clicked += async (sender, e) => {
        try {
          progressIndicator.IsRunning = true;
          progressText.Text = "loading...";

          var g1 = model.CurrentLocation;
          var g2 = await geocoder.GetGeocodeForLocation(loc2.Text);
          var g3 = geocoder.GetGreatCircleMidpoint(g1, g2);
          var places = await geocoder.GetNearbyPlaces(g3, modePicker.Items[modePicker.SelectedIndex]);
          var output = new OutputPage(places);
          await Navigation.PushAsync(output);
          progressText.Text = "";
        }
        catch (Exception ex) {
          progressText.Text = ex.Message;
        }
        finally {
          progressIndicator.IsRunning = false;
        }
      };

    }

    protected async override void OnAppearing() {
      base.OnAppearing();

      try {
        var loc = await this.geocoder.GetCurrentLocation();
        this.model.CurrentLocation = loc;
        var address = await this.geocoder.GetAddressForLocation(loc);
        this.model.CurrentAddress = address;
      }
      catch (Exception ex) {
        this.model.CurrentLocationException = ex;
      }
    }

  }
}
