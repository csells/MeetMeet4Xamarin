using Xamarin.Forms;

namespace MeetMeet {
  public class App {
    public static Page GetMainPage() {
      return new NavigationPage(new InputPage());
    }
  }
}
