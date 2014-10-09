using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace MeetMeet {
  public class App {
    public static Page GetMainPage() {
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

      var gps1 = new Label();
      var gps2 = new Label();
      var gps3 = new Label();

      var okButton = new Button {
        Text = "OK",
        HorizontalOptions = LayoutOptions.EndAndExpand,
        WidthRequest = 200,
      };

      okButton.Clicked += async (sender, e) => {
        var g1 = await Geocoder.GetGeocodeForLocation(loc1.Text);
        gps1.Text = "loc1= " + g1.ToString();

        var g2 = await Geocoder.GetGeocodeForLocation(loc2.Text);
        gps2.Text = "loc2= " + g2.ToString();

        var g3 = Geocoder.GetGreatCircleMidpoint(g1, g2);
        gps3.Text = "mid= " + g3.ToString();
      };

      return new ContentPage {
        Content = new TableView {
          Intent = TableIntent.Form,
          Root = new TableRoot("TableRoot Title") { // only shows on WP8
            new TableSection("TableSection Title") { // shows everywhere
              loc1,
              loc2,
              new ViewCell {
                View = new StackLayout {
                  Orientation = StackOrientation.Horizontal,
                  Children = { okButton },
                },
              },
              new ViewCell { View = gps1 },
              new ViewCell { View = gps2 },
              new ViewCell { View = gps3 },
            },
          },
        },
      };

    }

  }

}
