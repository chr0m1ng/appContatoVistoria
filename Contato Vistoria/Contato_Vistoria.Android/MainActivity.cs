
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Plugin.Permissions;
using System.IO;

namespace Contato_Vistoria.Droid
{
    [Activity(Label = "Contato Vistoria", Icon = "@drawable/iconCar", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(bundle);

            global::Xamarin.Forms.Forms.Init(this, bundle);
            LoadApplication(new App());
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Directory.Delete("/storage/emulated/0/Android/data/Contato_Vistoria.Android/files/Pictures", true);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

