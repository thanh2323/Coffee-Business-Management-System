
using System.Text.RegularExpressions;

namespace CoffeeShop.Domain.Rules
{
    public static class UserRules
    {
        // Password requirements
        public const int MIN_PASSWORD_LENGTH = 6;
        public const int MAX_PASSWORD_LENGTH = 100;

        // Username requirements
        public const int MIN_USERNAME_LENGTH = 3;
        public const int MAX_USERNAME_LENGTH = 50;

        // Email validation regex
        private static readonly Regex EmailRegex = new Regex(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$");

        // 1. Validate user data
        public static bool IsValidUserData(string username, string email, string password)
        {
            return IsValidUsername(username) &&
                   IsValidEmail(email) &&
                   IsValidPassword(password);
        }

        // 2. Validate username
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return false;

            if (username.Length < MIN_USERNAME_LENGTH || username.Length > MAX_USERNAME_LENGTH)
                return false;

            // Username should only contain letters, numbers, and underscores
            return Regex.IsMatch(username, @"^[a-zA-Z0-9_]+$");
        }

        // 3. Validate email
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            return EmailRegex.IsMatch(email);
        }

        // 4. Validate password
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            if (password.Length < MIN_PASSWORD_LENGTH || password.Length > MAX_PASSWORD_LENGTH)
                return false;

            return true;
        }

        // 5. Get password requirements message
        public static string GetPasswordRequirements()
        {
            return $"Password must be between {MIN_PASSWORD_LENGTH} and {MAX_PASSWORD_LENGTH} characters.";
        }

        // 6. Get username requirements message
        public static string GetUsernameRequirements()
        {
            return $"Username must be between {MIN_USERNAME_LENGTH} and {MAX_USERNAME_LENGTH} characters and contain only letters, numbers, and underscores.";
        }
    }
}