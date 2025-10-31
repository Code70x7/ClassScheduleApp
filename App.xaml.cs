// File: App.xaml.cs
using System.Threading.Tasks;
using ClassScheduleApp.Views;        // <-- needed for LoginPage
using ClassScheduleApp.Services;     // if you call into services at startup

namespace ClassScheduleApp;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();  // <-- requires App.xaml x:Class to match exactly
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("UNHANDLED (AppDomain): " + e.ExceptionObject);
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("UNOBSERVED (TaskScheduler): " + e.Exception);
            e.SetObserved();
        };
#if ANDROID
Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (s, e) =>
{
    System.Diagnostics.Debug.WriteLine("UNHANDLED (Android): " + e.Exception);
};
#endif

        // Start at Login wrapped in a NavigationPage
        MainPage = new NavigationPage(new LoginPage());

        // Optional: global crash logging so silent crashes show up in Output
        AppDomain.CurrentDomain.UnhandledException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("UNHANDLED (AppDomain): " + e.ExceptionObject);
        };
        TaskScheduler.UnobservedTaskException += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("UNOBSERVED (TaskScheduler): " + e.Exception);
            e.SetObserved();
        };
#if ANDROID
        Android.Runtime.AndroidEnvironment.UnhandledExceptionRaiser += (s, e) =>
        {
            System.Diagnostics.Debug.WriteLine("UNHANDLED (Android): " + e.Exception);
            // allow system to continue default handling
        };
#endif
    }
}
