// File: Views/RegisterPage.xaml.cs
using ClassScheduleApp.Helpers;
using ClassScheduleApp.Services;
using SQLite;

namespace ClassScheduleApp.Views;

public partial class RegisterPage : ContentPage
{
    private readonly AppDatabase _db;

    public RegisterPage() : this(ServiceHelper.GetRequiredService<AppDatabase>()) { }
    public RegisterPage(AppDatabase db)
    {
        InitializeComponent();
        _db = db;
    }

    private static bool LooksLikeEmail(string s)
        => !string.IsNullOrWhiteSpace(s) && s.Contains('@') && s.Contains('.');

    // XAML: Clicked="CreateAccount_Clicked"
    private async void CreateAccount_Clicked(object? sender, EventArgs e)
    {
        try
        {
            // Ensure tables/migrations exist on a fresh install
            await _db.InitializeAsync();

            var email = (EmailEntry?.Text ?? "").Trim().ToLowerInvariant();
            var pw = (PasswordEntry?.Text ?? "").Trim();
            var pw2 = (ConfirmEntry?.Text ?? "").Trim();

            if (!LooksLikeEmail(email)) { await DisplayAlert("Register", "Enter a valid email.", "OK"); return; }
            if (pw.Length < 6) { await DisplayAlert("Register", "Password must be ≥ 6 characters.", "OK"); return; }
            if (pw != pw2) { await DisplayAlert("Register", "Passwords don’t match.", "OK"); return; }

            var ok = await _db.CreateUserAsync(email, pw);
            if (!ok) { await DisplayAlert("Register", "Email already exists.", "OK"); return; }

            await DisplayAlert("Success", "Account created. You can sign in now.", "OK");

            // Safely return to Login regardless of how we got here
            if (Navigation?.NavigationStack?.Count > 1)
                await Navigation.PopAsync();
            else if (Navigation?.ModalStack?.Count > 0)
                await Navigation.PopModalAsync();
            else
                Application.Current!.MainPage = new NavigationPage(new LoginPage());
        }
        catch (SQLiteException sx)
        {
            await DisplayAlert("SQLite Error", sx.Message, "OK");
        }
        catch (InvalidOperationException iox)
        {
            // Navigation pop mismatch (rare on fresh install)
            await DisplayAlert("Navigation", iox.Message, "OK");
            Application.Current!.MainPage = new NavigationPage(new LoginPage());
        }
        catch (Exception ex)
        {
            // Show the exact failure instead of crashing
            await DisplayAlert("Unexpected Error", ex.ToString(), "OK");
        }
    }

    // XAML: Clicked="BackToLogin_Clicked"
    private async void BackToLogin_Clicked(object? sender, EventArgs e)
    {
        if (Navigation?.NavigationStack?.Count > 1)
            await Navigation.PopAsync();
        else if (Navigation?.ModalStack?.Count > 0)
            await Navigation.PopModalAsync();
        else
            Application.Current!.MainPage = new NavigationPage(new LoginPage());
    }
}
