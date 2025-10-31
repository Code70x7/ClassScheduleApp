// File: Services/IAuthService.cs
using System.Threading.Tasks;

namespace ClassScheduleApp.Services;

public interface IAuthService
{
    Task<bool> IsSignedInAsync();
    Task<bool> SignInAsync(string email, string password); // keep simple for class project
    Task SignOutAsync();

    // optional current user info
    string? CurrentUserEmail { get; }
}
