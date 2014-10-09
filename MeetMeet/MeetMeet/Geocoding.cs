using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace MeetMeet {
  public class Geocode {
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public override string ToString() {
      return string.Format("(lat: {0}, long: {1}", this.Latitude, this.Longitude);
    }
  }

  // from: http://stackoverflow.com/questions/17390133/parsing-google-maps-api-geocode-server-side-or-client-side
  public static class Geocoder {
    public static async Task<Geocode> GetGeocodeForLocation(string location) {
      // e.g.
      // http://maps.googleapis.com/maps/api/geocode/xml?address=portland,%20or
      string request = string.Format("http://maps.googleapis.com/maps/api/geocode/xml?address={0}", Uri.EscapeUriString(location));
      var xml = await (new HttpClient()).GetStringAsync(request);
      var loc = XDocument.Parse(xml).Element("GeocodeResponse").Element("result").Element("geometry").Element("location");

      return new Geocode {
        Latitude = double.Parse(loc.Element("lat").Value),
        Longitude = double.Parse(loc.Element("lng").Value),
      };
    }

    //static Geocode GetMiddleGeocode(Geocode geocode1, Geocode geocode2) {
    //  return new Geocode {
    //    Latitude = (geocode1.Latitude + geocode2.Latitude) / 2,
    //    Longitude = (geocode1.Longitude + geocode2.Longitude) / 2
    //  };
    //}

    // http://stackoverflow.com/questions/6830959/c-sharp-find-midpoint-of-two-latitude-longitudes
    public static Geocode GetGreatCircleMidpoint(Geocode g1, Geocode g2) {
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

    //static internal Uri GetMiddleUrl(string you, string them, string mode) {
    //  // get lat/long in the middle
    //  Geocode youGeocode = GetGeocodeForLocation(you);
    //  Geocode themGeocode = GetGeocodeForLocation(them);
    //  Geocode middleGeocode = GetMiddleGeocode(youGeocode, themGeocode);

    //  // construct URL for "food near " calculated lat/long
    //  //string url = string.Format("http://maps.google.com/?q={0} near {1},{2}", mode, middleGeocode.Latitude, middleGeocode.Longitude);
    //  // NOTE: updated for new Google maps ala http://www.jsonline.com/blogs/news/247119811.html
    //  string url = string.Format("https://www.google.com/maps/search/{0} loc: {1},{2}", mode, middleGeocode.Latitude, middleGeocode.Longitude);
    //  return new Uri(Uri.EscapeUriString(url));
    //}

  }
}
