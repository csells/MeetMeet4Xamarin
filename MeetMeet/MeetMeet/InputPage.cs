using System;
using System.Linq;
using Xamarin.Forms;

namespace MeetMeet {
  class InputPage : ContentPage {
    public InputPage() {
      this.Padding = 20;

      var loc1 = new EntryCell {
        Label = "Location #1",
        Placeholder = "my location",
        Text = "portland, or",
      };

      var loc2 = new EntryCell {
        Label = "Location #2",
        Placeholder = "enter location",
        Text = "seattle, wa",
      };

      var okButton = new Button {
        Text = "OK",
        HorizontalOptions = LayoutOptions.EndAndExpand,
        WidthRequest = 200,
      };

      var progressIndicator = new ActivityIndicator();
      var progressText = new Label() { XAlign = TextAlignment.Center };

      Content = new TableView {
        Intent = TableIntent.Menu,
        Root = new TableRoot() { // only shows on WP8
          new TableSection("Locations") { // shows everywhere
            loc1,
            loc2,
            new ViewCell {
              View = new StackLayout {
                Orientation = StackOrientation.Horizontal,
                Children = { okButton },
              },
            },
            new ViewCell { View = progressIndicator },
            new ViewCell { View = progressText },
          },
        },
      };

      okButton.Clicked += async (sender, e) => {
        try {
          progressIndicator.IsRunning = true;
          progressText.Text = "loading...";

          var geocoder = new Geocoder();
          var g1 = await geocoder.GetGeocodeForLocation(loc1.Text);
          var g2 = await geocoder.GetGeocodeForLocation(loc2.Text);
          var g3 = geocoder.GetGreatCircleMidpoint(g1, g2);
          var places = geocoder.GetNearbyPlaces(g3, "coffee"); // TODO: mode
          var output = new OutputPage(await places);
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

  }
}
