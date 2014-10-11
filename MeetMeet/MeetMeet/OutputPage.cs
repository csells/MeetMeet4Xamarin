using System;
using System.Linq;
using System.Collections.Generic;
using Xamarin.Forms;

namespace MeetMeet {
  class OutputPage : ContentPage {
    public OutputPage(IEnumerable<Place> places) {
      this.Padding = 20;

      var template = new DataTemplate(typeof(ImageCell));
      template.SetBinding(ImageCell.TextProperty, "Name");
      template.SetBinding(ImageCell.DetailProperty, "Vicinity");
      template.SetBinding(ImageCell.ImageSourceProperty, "Icon");

      var list = new ListView { ItemsSource = places.Take(20), ItemTemplate = template };
      list.ItemSelected += (sender, e) => {
        if (e.SelectedItem == null) { return; }
        (new Geocoder()).LaunchMapApp((Place)e.SelectedItem);
      };

      Content = list;
    }
  }
}
