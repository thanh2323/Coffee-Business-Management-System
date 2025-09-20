using System;
using System.Text.RegularExpressions;

namespace CoffeeShop.Domain.Rules
{
    public static class UserRules
    {
        // 1. Validate email format
        public static bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return false;

            try
            {
                var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
                return regex.IsMatch(email);
            }
            catch
            {
                return false;
            }
        }

        // 2. Validate username format
        public static bool IsValidUsername(string username)
        {
            if (string.IsNullOrEmpty(username))
                return false;

            // Username must be at least 3 characters, alphanumeric and underscore only
            if (username.Length < 3 || username.Length > 50)
                return false;

            var regex = new Regex(@"^[a-zA-Z0-9_]+$");
            return regex.IsMatch(username);
        }

        // 3. Validate password strength
        public static bool IsValidPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(password))
                return false;

            // Password must be at least 6 characters
            return password.Length >= 6;
        }

        // 4. Validate user data
        public static bool IsValidUserData(string username, string email, string password)
        {
            return IsValidUsername(username) && 
                   IsValidEmail(email) && 
                   IsValidPassword(password);
        }

        // 5. Get password requirements
        public static string GetPasswordRequirements()
        {
            return "Password must be at least 6 characters long";
        }

        // 6. Get username requirements
        public static string GetUsernameRequirements()
        {
            return "Username must be 3-50 characters, alphanumeric and underscore only";
        }
    }
}
