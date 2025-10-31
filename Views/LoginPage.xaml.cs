using ClassScheduleApp.Helpers;
using ClassScheduleApp.Services;
using SQLite;

namespace ClassScheduleApp.Views;

public partial class LoginPage : ContentPage
{
    private readonly AppDatabase _db;

    public LoginPage() : this(ServiceHelper.GetRequiredService<AppDatabase>()) { }
    public LoginPage(AppDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    private static bool LooksLikeEmail(string s)
        => !string.IsNullOrWhiteSpace(s) && s.Contains('@') && s.Contains('.');

    private async void SignIn_Clicked(object? sender, EventArgs e)
    {
        try
        {
            await _db.InitializeAsync();

            var email = (EmailEntry?.Text ?? string.Empty).Trim().ToLowerInvariant();
            var pw = (PasswordEntry?.Text ?? string.Empty).Trim();

            if (!LooksLikeEmail(email)) { await DisplayAlert("Login", "Please enter a valid email.", "OK"); return; }
            if (pw.Length < 6) { await DisplayAlert("Login", "Password must be at least 6 characters.", "OK"); return; }

            var ok = await _db.ValidateUserAsync(email, pw);
            if (!ok) { await DisplayAlert("Login", "Invalid email or password.", "OK"); return; }

            try { _ = SecureStorage.SetAsync("auth_email", email); } catch { }

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                // 1) Try Shell
                try
                {
                    var shell = new AppShell();                    // if this throws, we catch below
                    Application.Current!.MainPage = shell;         // no navigation needed
                    return;
                }
                catch (Exception shellEx)
                {
                    System.Diagnostics.Debug.WriteLine("Shell failed: " + shellEx);
                    await DisplayAlert("Startup", "Shell failed:\n" + shellEx.Message, "OK");
                }

                // 2) Fallback: plain page (no Shell, no routes)
                try
                {
                    Application.Current!.MainPage = new NavigationPage(new TermsPage());
                    return;
                }
                catch (Exception tpEx)
                {
                    System.Diagnostics.Debug.WriteLine("TermsPage fallback failed: " + tpEx);
                }

                // 3) Last resort: tiny safe page
                Application.Current!.MainPage = new NavigationPage(new SafeHomePage());
            });
        }
        catch (SQLiteException sx)
        {
            await DisplayAlert("Database Error", sx.Message, "OK");
        }
        catch (Exception ex)
        {
            await DisplayAlert("Login Error", ex.ToString(), "OK");
        }
    }

    private async void GoRegister_Clicked(object? sender, EventArgs e)
    {
        try
        {
            await _db.InitializeAsync();
            await Navigation.PushAsync(new RegisterPage());
        }
        catch (Exception ex)
        {
            await DisplayAlert("Navigation", $"Could not open Register.\n{ex.Message}", "OK");
        }
    }
}
