using System.Windows.Forms;
using TiendaLinea.UI;

namespace TiendaLinea.UI.Controls
{
    public class UsuarioFields
    {
        public NumericUpDown NumCodigo { get; set; }
        public TextBox TxtNombre { get; set; }
        public TextBox TxtCorreo { get; set; }
        public TextBox TxtClave { get; set; }
        public CheckBox ChkActivo { get; set; }
    }

    internal static class UsuariosFieldsBuilder
    {
        public static UsuarioFields AddUsuarioRows(TableLayoutPanel panel)
        {
            var fields = new UsuarioFields
            {
                NumCodigo = new NumericUpDown { 
                    Minimum = 1, 
                    Maximum = 999999, 
                    Value = 1,
                    ReadOnly = true,
                    BackColor = System.Drawing.Color.FromArgb(232, 240, 254)
                },
                TxtNombre = new TextBox(),
                TxtCorreo = new TextBox(),
                TxtClave = new TextBox { PasswordChar = '*' },
                ChkActivo = new CheckBox { Checked = true }
            };

            FormLayoutHelper.AddRow(panel, "Código:", fields.NumCodigo);
            FormLayoutHelper.AddRow(panel, "Nombre:", fields.TxtNombre);
            FormLayoutHelper.AddRow(panel, "Correo:", fields.TxtCorreo);
            FormLayoutHelper.AddRow(panel, "Clave:", fields.TxtClave);
            FormLayoutHelper.AddRow(panel, "Activo:", fields.ChkActivo);

            return fields;
        }

        public static bool ValidateFields(UsuarioFields fields, ErrorProvider errorProvider)
        {
            bool ok = true;

            if (!Validators.IsRequired(fields.TxtNombre.Text))
            {
                errorProvider.SetError(fields.TxtNombre, "El nombre es obligatorio.");
                ok = false;
            }
            else errorProvider.SetError(fields.TxtNombre, string.Empty);

            if (!Validators.IsValidEmail(fields.TxtCorreo.Text))
            {
                errorProvider.SetError(fields.TxtCorreo, "Correo inválido.");
                ok = false;
            }
            else errorProvider.SetError(fields.TxtCorreo, string.Empty);

            if (!Validators.IsRequired(fields.TxtClave.Text))
            {
                errorProvider.SetError(fields.TxtClave, "La clave es obligatoria.");
                ok = false;
            }
            else errorProvider.SetError(fields.TxtClave, string.Empty);

            return ok;
        }
    }
}



