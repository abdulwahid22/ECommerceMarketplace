using System.Text.RegularExpressions;

namespace IdentityService.API.Helpers
{
    public static class PasswordValidator
    {
        public static List<string> Validate(string password)
        {
            var errors = new List<string>();

            if (string.IsNullOrWhiteSpace(password))
            {
                errors.Add("Password is required.");
                return errors;
            }

            if (password.Length < 8)
            {
                errors.Add("Password must be at least 8 characters.");
            }

            if (!Regex.IsMatch(password, "[A-Z]"))
            {
                errors.Add("Password must contain at least one uppercase letter.");
            }

            if (!Regex.IsMatch(password, "[a-z]"))
            {
                errors.Add("Password must contain at least one lowercase letter.");
            }

            if (!Regex.IsMatch(password, "[0-9]"))
            {
                errors.Add("Password must contain at least one number.");
            }

            if (!Regex.IsMatch(password, "[^a-zA-Z0-9]"))
            {
                errors.Add("Password must contain at least one special character.");
            }

            return errors;
        }
    }
}