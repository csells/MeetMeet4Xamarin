using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;

using Xamarin.Forms.Platform.Android;
using Xamarin.Forms.Labs.Services;
using Xamarin.Forms.Labs;

// DONE
//[assembly: UsesPermission(Android.Manifest.Permission.AccessFineLocation)]
//[assembly: UsesPermission(Android.Manifest.Permission.AccessCoarseLocation)]

namespace MeetMeet.Droid {
  [Activity(Label = "MeetMeet", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
  public class MainActivity : AndroidActivity {
    protected override void OnCreate(Bundle bundle) {
      base.OnCreate(bundle);

      // DONE: set up services
      var resolverContainer = new SimpleContainer();
      resolverContainer.Register<IDevice>(t => AndroidDevice.CurrentDevice)
        .Register<IDisplay>(t => t.Resolve<IDevice>().Display)
        .Register<IDependencyContainer>(resolverContainer);
      Resolver.SetResolver(resolverContainer.GetResolver());

      Xamarin.Forms.Forms.Init(this, bundle);

      SetPage(App.GetMainPage());
    }
  }
}

