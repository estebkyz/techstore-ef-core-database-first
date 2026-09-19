using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TiendaLinea.Models.Inventario;
using TiendaLinea.Models.Usuarios;
using TiendaLinea.Models.Ventas;
using TiendaLinea.Data.Context;

namespace TiendaLinea.UI.Controls
{
    public class EmpleadosControl : UserControl
    {
        private readonly AppState      _appState;
        private readonly DataGridView  _grid          = new();
        private readonly BindingSource _bindingSource  = new();
        private readonly ErrorProvider _errorProvider  = new();
        private UsuarioFields          _baseFields;

        public EmpleadosControl(AppState appState)
        {
            _appState = appState;
            InitializeLayout();
            _baseFields.NumCodigo.Value = _appState.GetNextCodigoUsuario();
        }

        private void InitializeLayout()
        {
            var formPanel = FormLayoutHelper.CreateFormPanel();
            _baseFields = UsuariosFieldsBuilder.AddUsuarioRows(formPanel);


            var buttonBar = FormLayoutHelper.CreateButtonBar();

            var btnAdd    = new Button { Text = "▶  Agregar",          AutoSize = true };
            var btnToggle = new Button { Text = "⏺  Activar/Inactivar", AutoSize = true };
            var btnExport = new Button { Text = "📤  Exportar JSON",   AutoSize = true };

            FormLayoutHelper.StyleButton(btnAdd,    ButtonStyle.Primary);
            FormLayoutHelper.StyleButton(btnToggle, ButtonStyle.Neutral);
            FormLayoutHelper.StyleButton(btnExport, ButtonStyle.Success);

            btnAdd.Click    += OnAddClick;
            btnToggle.Click += OnToggleStateClick;
            btnExport.Click += OnExportClick;

            buttonBar.Controls.Add(btnAdd);
            buttonBar.Controls.Add(btnToggle);
            buttonBar.Controls.Add(btnExport);

            _grid.Dock                = DockStyle.Fill;
            _grid.AutoGenerateColumns = true;
            _grid.ReadOnly            = true;
            FormLayoutHelper.StyleGrid(_grid);

            _bindingSource.DataSource = _appState.Empleados;
            _grid.DataSource          = _bindingSource;

            Controls.Add(_grid);
            Controls.Add(buttonBar);
            Controls.Add(formPanel);
        }

        private async void OnToggleStateClick(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not Usuario emp)
            {
                MessageBox.Show("Seleccione un Usuario de la tabla.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var confirm = MessageBox.Show(
                $"¿Desea {(emp.Activo ? "inactivar" : "activar")} a '{emp.Nombre}'?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                emp.Activo = !emp.Activo;
                await GridActions.RunDatabaseOperationAsync(this, "Actualizar", async () => await _appState.UpdateEmpleadoAsync(emp));
                MessageBox.Show(
                    $"'{emp.Nombre}' ahora está {(emp.Activo ? "ACTIVO" : "INACTIVO")}.",
                    "Estado actualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void OnAddClick(object? sender, EventArgs e)
        {
            if (!UsuariosFieldsBuilder.ValidateFields(_baseFields, _errorProvider)) return;

            if (await _appState.ExisteCorreoAsync(_baseFields.TxtCorreo.Text.Trim()))
            {
                MessageBox.Show("Ya existe un Usuario con este correo.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var codigo = _appState.GetNextCodigoUsuario();
            var emp = new Usuario {
                Codigo = codigo,
                Nombre = _baseFields.TxtNombre.Text.Trim(),
                Correo = _baseFields.TxtCorreo.Text.Trim(),
                Clave = _baseFields.TxtClave.Text.Trim(),
                Activo = _baseFields.ChkActivo.Checked,
                TipoUsuario = "Empleado"
            };

            await GridActions.RunDatabaseOperationAsync(this, "Crear", async () => await _appState.AddEmpleadoAsync(emp));

            emp.SaveToJson(GridActions.GetOutputPath("Usuario", emp.Codigo));
            ClearFields();
        }

        private void ClearFields()
        {
            _baseFields.NumCodigo.Value = _appState.GetNextCodigoUsuario();
            _baseFields.NumCodigo.ReadOnly = true;
            _baseFields.NumCodigo.BackColor = Color.FromArgb(232, 240, 254);
            _baseFields.TxtClave.Clear();
            _baseFields.ChkActivo.Checked = true;
        }

        private void OnExportClick(object? sender, EventArgs e) =>
            GridActions.ExportSelected<Usuario>(_grid, "Usuario.json", (emp, path) => emp.SaveToJson(path));
    }
}


