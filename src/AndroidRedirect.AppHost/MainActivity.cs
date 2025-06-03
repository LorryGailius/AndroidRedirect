using Android.Content;

namespace AndroidRedirect.AppHost
{
    [Activity(Label = "@string/app_name", MainLauncher = true)]
    public class MainActivity : Activity
    {
        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var intent = PackageManager?.GetLaunchIntentForPackage("?package_name?");

            if (intent != null)
            {
                intent.AddCategory(Intent.CategoryLauncher);
                intent.SetFlags(ActivityFlags.NewTask);
                StartActivity(intent);
            }

            FinishAndRemoveTask();
        }
    }
}