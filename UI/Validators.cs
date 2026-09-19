using System.Text.RegularExpressions;

namespace TiendaLinea.UI
{
    internal static class Validators
    {
        public static bool IsRequired(string value) => !string.IsNullOrWhiteSpace(value);
        
        public static bool IsValidEmail(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return false;
            try
            {
                return Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase);
            }
            catch
            {
                return false;
            }
        }
    }
}


