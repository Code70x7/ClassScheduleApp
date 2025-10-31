// File: Models/UserAccount.cs
using SQLite;

namespace ClassScheduleApp.Models
{
    public class UserAccount
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }

        [Indexed(Name = "idx_user_email", Unique = true)]
        public string Email { get; set; } = string.Empty;

        // PBKDF2 string "salt:hash" (or legacy plaintext for old rows)
        public string? PasswordHash { get; set; }
    }
}
