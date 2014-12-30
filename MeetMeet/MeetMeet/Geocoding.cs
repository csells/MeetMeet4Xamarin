using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Forms;
using Xamarin.Forms.Labs.Services.Geolocation;
//using Xamarin.Forms.Labs.Services.Geolocation;

namespace MeetMeet {
  public class Geocode {
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public override string ToString() {
      return string.Format("(lat: {0}, long: {1}", this.Latitude, this.Longitude);
    }
  }

  public class Place {
    public string Name { get; set; }
    public string Vicinity { get; set; }
    public Geocode Location { get; set; }
    public Uri Icon { get; set; }
  }

  public class Geocoder {
    public async Task<Geocode> GetGeocodeForLocation(string location) {
      // from: http://stackoverflow.com/questions/17390133/parsing-google-maps-api-geocode-server-side-or-client-side
      // e.g. http://maps.googleapis.com/maps/api/geocode/xml?address=portland,%20or
      string request = string.Format("http://maps.googleapis.com/maps/api/geocode/xml?address={0}", Uri.EscapeUriString(location));
      var xml = await (new HttpClient()).GetStringAsync(request);
      var loc = XDocument.Parse(xml).Element("GeocodeResponse").Element("result").Element("geometry").Element("location");

      return new Geocode {
        Latitude = double.Parse(loc.Element("lat").Value),
        Longitude = double.Parse(loc.Element("lng").Value),
      };
    }

    public Geocode GetGreatCircleMidpoint(Geocode g1, Geocode g2) {
      // from http://stackoverflow.com/questions/6830959/c-sharp-find-midpoint-of-two-latitude-longitudes

      // convert to radians
      var dLon = deg2rad(g2.Longitude - g1.Longitude);
      var lat1 = deg2rad(g1.Latitude);
      var lat2 = deg2rad(g2.Latitude);
      var lon1 = deg2rad(g1.Longitude);

      // calculate the great circle midpoint
      var Bx = Math.Cos(lat2) * Math.Cos(dLon);
      var By = Math.Cos(lat2) * Math.Sin(dLon);
      var lat3 = Math.Atan2(Math.Sin(lat1) + Math.Sin(lat2), Math.Sqrt((Math.Cos(lat1) + Bx) * (Math.Cos(lat1) + Bx) + By * By));
      var lon3 = lon1 + Math.Atan2(By, Math.Cos(lat1) + Bx);

      // convert to degrees
      return new Geocode {
        Latitude = rad2deg(lat3),
        Longitude = rad2deg(lon3),
      };
    }

    static double deg2rad(double deg) {
      return deg * (Math.PI / 180.0);
    }

    static double rad2deg(double rad) {
      return rad / (Math.PI / 180.0);
    }

    public async Task<IEnumerable<string>> GetPlacesAutocompleteAsync(string search) {
      // from: https://developers.google.com/places/documentation/autocomplete
      // e.g. https://maps.googleapis.com/maps/api/place/autocomplete/xml?input=Kirk&key=AddYourOwnKeyHere
      string request = string.Format("https://maps.googleapis.com/maps/api/place/autocomplete/xml?input={0}&key={1}", search, GetGoogleApiKey());
      var xml = await (new HttpClient()).GetStringAsync(request);
      var results = XDocument.Parse(xml).Element("AutocompletionResponse").Elements("prediction");

      var suggestions = new List<string>();
      foreach (var result in results) {
        var suggestion = result.Element("description").Value;
        suggestions.Add(suggestion);
      }

      return suggestions;
    }

    public async Task<IEnumerable<Place>> GetNearbyPlacesAsync(Geocode g, string keyword) {
      // from: https://developers.google.com/places/documentation/search
      // e.g. https://maps.googleapis.com/maps/api/place/nearbysearch/xml?location=46,-122&rankby=distance&keyword=coffee&key=AddYourOwnKeyHere
      string request = string.Format("https://maps.googleapis.com/maps/api/place/nearbysearch/xml?location={0},{1}&rankby=distance&keyword={2}&key={3}", g.Latitude, g.Longitude, keyword, GetGoogleApiKey());
      var xml = await (new HttpClient()).GetStringAsync(request);
      var results = XDocument.Parse(xml).Element("PlaceSearchResponse").Elements("result");

      var places = new List<Place>();
      foreach (var result in results) {
        var loc = result.Element("geometry").Element("location");
        var icon = result.Element("icon").Value;
        places.Add(new Place {
          Name = result.Element("name").Value,
          Icon = !string.IsNullOrWhiteSpace(icon) ? new Uri(icon) : null,
          Vicinity = result.Element("vicinity").Value,
          Location = new Geocode {
            Latitude = double.Parse(loc.Element("lat").Value),
            Longitude = double.Parse(loc.Element("lng").Value),
          },
        });
      }

      return places;
    }

    public void LaunchMapApp(Place place) {
      var name = Uri.EscapeUriString(place.Name);

#if __IOS__
      // from https://developer.apple.com/library/ios/featuredarticles/iPhoneURLScheme_Reference/MapLinks/MapLinks.html
      // e.g. http://maps.apple.com/?daddr=San+Francisco,+CA&saddr=cupertino
      var loc = string.Format("{0},{1}", place.Location.Latitude, place.Location.Longitude);
      var request = string.Format("http://maps.apple.com/maps?q={0}@{1}", name, loc);
      //Device.OpenUri(new Uri(request));
      MonoTouch.UIKit.UIApplication.SharedApplication.OpenUrl(new MonoTouch.Foundation.NSUrl(request));
#elif __ANDROID__
      // from: http://developer.android.com/guide/components/intents-common.html#Maps
      // e.g. geo:0,0?q=34.99,-106.61(Treasure)
      var loc = !string.IsNullOrWhiteSpace(place.Vicinity) ? Uri.EscapeUriString(place.Vicinity) : string.Format("{0},{1}", place.Location.Latitude, place.Location.Longitude);
      var request = string.Format("geo:0,0?q={0}({1})", loc, name);
      Device.OpenUri(new Uri(request));
#elif WINDOWS_PHONE
      // from http://msdn.microsoft.com/en-us/library/windows/apps/jj635237.aspx
      // e.g. bingmaps:?collection=point.36.116584_-115.176753_Caesars%20Palace
      var loc = string.Format("{0}_{1}", place.Location.Latitude, place.Location.Longitude);
      var request = string.Format("bingmaps:?collection=point.{0}_{1}", loc, name);
      Windows.System.Launcher.LaunchUriAsync(new Uri(request)).AsTask().RunSynchronously();
#else
      throw new Exception("No device type compile-time directive found");
#endif
    }

    string GetGoogleApiKey() {
      // from http://developer.xamarin.com/guides/cross-platform/xamarin-forms/working-with/files/
      // NOTE: expects Embedded Resource named config.xml of the following format:
      // <?xml version="1.0" encoding="utf-8" ?>
      // <config>
      //   <google-api-key>YourGoogleApiKeyHere</google-api-key>
      // </config>
      var type = this.GetType();
      var resource = type.Namespace + "." + Device.OnPlatform("iOS", "Droid", "WinPhone") + ".config.xml";
      using (var stream = type.Assembly.GetManifestResourceStream(resource))
      using (var reader = new StreamReader(stream)) {
        var doc = XDocument.Parse(reader.ReadToEnd());
        return doc.Element("config").Element("google-api-key").Value;
      }
    }

    public async Task<Geocode> GetCurrentLocationAsync() {
      var geolocator = DependencyService.Get<IGeolocator>();
      var loc = await geolocator.GetPositionAsync(10000);
      return new Geocode { Latitude = loc.Latitude, Longitude = loc.Longitude };
    }

    public async Task<string> GetAddressForLocationAsync(Geocode loc) {
      // from https://developers.google.com/maps/documentation/geocoding/#ReverseGeocoding
      // e.g. https://maps.googleapis.com/maps/api/geocode/xml?latlng=40.714224,-73.961452
      string request = string.Format("https://maps.googleapis.com/maps/api/geocode/xml?latlng={0},{1}", loc.Latitude, loc.Longitude);
      var xml = await (new HttpClient()).GetStringAsync(request);
      var address = XDocument.Parse(xml).Element("GeocodeResponse").Element("result").Element("formatted_address").Value;
      return address;
    }

  }

}
