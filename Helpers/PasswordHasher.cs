// File: Helpers/PasswordHasher.cs
using System.Security.Cryptography;
using System.Text;

namespace ClassScheduleApp.Helpers
{
    public static class PasswordHasher
    {
        // Format: base64(salt) + ":" + base64(hash)
        public static string HashNew(string password, int saltSize = 16, int iter = 100_000, int keySize = 32)
        {
            using var rng = RandomNumberGenerator.Create();
            var salt = new byte[saltSize];
            rng.GetBytes(salt);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256);
            var key = pbkdf2.GetBytes(keySize);

            return $"{Convert.ToBase64String(salt)}:{Convert.ToBase64String(key)}";
        }

        public static bool Verify(string password, string stored, int iter = 100_000, int keySize = 32)
        {
            if (string.IsNullOrWhiteSpace(stored) || !stored.Contains(':')) return false;

            var parts = stored.Split(':');
            if (parts.Length != 2) return false;

            var salt = Convert.FromBase64String(parts[0]);
            var expected = Convert.FromBase64String(parts[1]);

            using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iter, HashAlgorithmName.SHA256);
            var actual = pbkdf2.GetBytes(keySize);

            // constant-time compare
            var diff = 0;
            for (int i = 0; i < expected.Length; i++)
                diff |= expected[i] ^ actual[i];
            return diff == 0;
        }
    }
}
