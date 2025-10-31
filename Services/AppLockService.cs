using System.Security.Cryptography;
using System.Text;

namespace ClassScheduleApp.Services
{
    public interface IAppLockService
    {
        Task SetPinAsync(string pin);
        Task<bool> ValidatePinAsync(string pin);
        Task<bool> IsSetAsync();
    }

    public class AppLockService : IAppLockService
    {
        private const string Key = "app_pin_hash";

        public async Task SetPinAsync(string pin)
        {
            var hash = Hash(pin);
            await SecureStorage.SetAsync(Key, hash);
        }

        public async Task<bool> ValidatePinAsync(string pin)
        {
            var saved = await SecureStorage.GetAsync(Key);
            if (string.IsNullOrEmpty(saved)) return false;
            return saved == Hash(pin);
        }

        public async Task<bool> IsSetAsync()
        {
            var saved = await SecureStorage.GetAsync(Key);
            return !string.IsNullOrEmpty(saved);
        }

        private static string Hash(string s)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(s));
            return Convert.ToHexString(bytes);
        }
    }
}
