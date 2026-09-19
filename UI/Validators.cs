namespace TiendaLinea.UI
{
    /// <summary>
    /// Validation helpers for UI controls. Validation happens here (not in the model POCOs)
    /// because database-first scaffolded classes have no constructors or guard clauses.
    /// </summary>
    public static class Validators
    {
        public static bool ValidateNotEmpty(string? value, string fieldName, ErrorProvider errorProvider, Control control)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                errorProvider.SetError(control, $"{fieldName} es obligatorio.");
                return false;
            }
            errorProvider.SetError(control, string.Empty);
            return true;
        }

        public static bool ValidatePositiveInt(string? value, string fieldName, ErrorProvider errorProvider, Control control, out int result)
        {
            result = 0;
            if (!int.TryParse(value, out result) || result <= 0)
            {
                errorProvider.SetError(control, $"{fieldName} debe ser un número entero positivo.");
                return false;
            }
            errorProvider.SetError(control, string.Empty);
            return true;
        }

        public static bool ValidatePositiveDecimal(string? value, string fieldName, ErrorProvider errorProvider, Control control, out decimal result)
        {
            result = 0;
            if (!decimal.TryParse(value, out result) || result <= 0)
            {
                errorProvider.SetError(control, $"{fieldName} debe ser un número positivo.");
                return false;
            }
            errorProvider.SetError(control, string.Empty);
            return true;
        }

        public static bool ValidateEmail(string? value, ErrorProvider errorProvider, Control control)
        {
            if (string.IsNullOrWhiteSpace(value) || !value.Contains('@'))
            {
                errorProvider.SetError(control, "El correo electrónico no es válido.");
                return false;
            }
            errorProvider.SetError(control, string.Empty);
            return true;
        }
    }
}
