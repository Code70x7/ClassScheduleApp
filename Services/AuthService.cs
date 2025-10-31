// File: Services/AuthService.cs
using System.Threading.Tasks;
using Microsoft.Maui.Storage; // Preferences
using ClassScheduleApp.Models;

namespace ClassScheduleApp.Services;

public sealed class AuthService : IAuthService
{
    private const string KeyUserEmail = "auth.current.email";
    private readonly AppDatabase _db;

    public AuthService(AppDatabase db) => _db = db;

    public string? CurrentUserEmail => Preferences.Get(KeyUserEmail, null);

    public Task<bool> IsSignedInAsync()
    {
        // Cheap check: just look for the persisted email
        var signedIn = !string.IsNullOrWhiteSpace(CurrentUserEmail);
        return Task.FromResult(signedIn);
    }

    public async Task<bool> SignInAsync(string email, string password)
    {
        // For the project we’ll accept any existing user record (or create one).
        var user = await _db.GetUserByEmailAsync(email);
        if (user is null)
        {
            user = new UserAccount { Email = email, PasswordHash = password }; // keep simple
            await _db.SaveUserAsync(user);
        }

        // Persist “session”
        Preferences.Set(KeyUserEmail, email);
        return true;
    }

    public Task SignOutAsync()
    {
        Preferences.Remove(KeyUserEmail);
        return Task.CompletedTask;
    }
}
