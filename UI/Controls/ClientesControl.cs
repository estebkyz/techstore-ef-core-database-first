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
    public class ClientesControl : UserControl
    {
        private readonly AppState      _appState;
        private readonly DataGridView  _grid          = new();
        private readonly BindingSource _bindingSource  = new();
        private readonly ErrorProvider _errorProvider  = new();
        private UsuarioFields          _baseFields;
        private readonly TextBox       _txtDireccion  = new();

        public ClientesControl(AppState appState)
        {
            _appState = appState;
            InitializeLayout();
            _baseFields.NumCodigo.Value = _appState.GetNextCodigoUsuario();
        }

        private void InitializeLayout()
        {
            var formPanel = FormLayoutHelper.CreateFormPanel();
            _baseFields = UsuariosFieldsBuilder.AddUsuarioRows(formPanel);
            FormLayoutHelper.AddRow(formPanel, "Dirección:", _txtDireccion);


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

            _bindingSource.DataSource = _appState.Clientes;
            _grid.DataSource          = _bindingSource;

            Controls.Add(_grid);
            Controls.Add(buttonBar);
            Controls.Add(formPanel);
        }

        private async void OnToggleStateClick(object? sender, EventArgs e)
        {
            if (_grid.CurrentRow?.DataBoundItem is not Usuario cli)
            {
                MessageBox.Show("Seleccione un Usuario de la tabla.", "Aviso",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            var confirm = MessageBox.Show(
                $"¿Desea {(cli.Activo ? "inactivar" : "activar")} a '{cli.Nombre}'?",
                "Confirmar", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                cli.Activo = !cli.Activo;
                await GridActions.RunDatabaseOperationAsync(this, "Actualizar", async () => await _appState.UpdateClienteAsync(cli));
                
                MessageBox.Show(
                    $"'{cli.Nombre}' ahora está {(cli.Activo ? "ACTIVO" : "INACTIVO")}.",
                    "Estado actualizado", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private async void OnAddClick(object? sender, EventArgs e)
        {
            if (!UsuariosFieldsBuilder.ValidateFields(_baseFields, _errorProvider)) return;
            if (!Validators.IsRequired(_txtDireccion.Text))
            {
                _errorProvider.SetError(_txtDireccion, "La dirección es obligatoria.");
                return;
            }
            _errorProvider.SetError(_txtDireccion, string.Empty);

            if (await _appState.ExisteCorreoAsync(_baseFields.TxtCorreo.Text.Trim()))
            {
                MessageBox.Show("Ya existe un usuario con este correo electrónico.", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var codigo = _appState.GetNextCodigoUsuario();
            var c = new Usuario {
                Codigo = codigo,
                Nombre = _baseFields.TxtNombre.Text.Trim(),
                Correo = _baseFields.TxtCorreo.Text.Trim(),
                ClaveHash = _baseFields.TxtClave.Text.Trim(),
                Direccion = _txtDireccion.Text.Trim(),
                Activo = _baseFields.ChkActivo.Checked,
                TipoUsuario = "Cliente"
            };

            await GridActions.RunDatabaseOperationAsync(this, "Crear", async () => await _appState.AddClienteAsync(c));
            c.SaveToJson(GridActions.GetOutputPath("Usuario", c.Codigo));
            
            ClearFields();
        }

        private void ClearFields()
        {
            _baseFields.NumCodigo.Value = _appState.GetNextCodigoUsuario();
            _baseFields.NumCodigo.ReadOnly = true;
            _baseFields.NumCodigo.BackColor = Color.FromArgb(232, 240, 254);
            _baseFields.TxtClave.Clear();
            _baseFields.ChkActivo.Checked = true;
            _txtDireccion.Clear();
        }

        private void OnExportClick(object? sender, EventArgs e) =>
            GridActions.ExportSelected<Usuario>(_grid, "Usuario.json", (c, path) => c.SaveToJson(path));
    }
}



