using TiendaLinea.Models;
using TiendaLinea.UI;

namespace TiendaLinea.UI.Controls
{
    public class AdministradoresControl : UserControl, IReferenceDataConsumer
    {
        private readonly AppState _state;
        private readonly ErrorProvider _errors = new();

        private DataGridView _grid = null!;
        private TextBox _txtId = null!, _txtNombre = null!, _txtCorreo = null!, _txtNivel = null!;
        private Button _btnAgregar = null!, _btnActualizar = null!, _btnEliminar = null!, _btnLimpiar = null!;
        private Usuario? _selected;

        public AdministradoresControl(AppState state) { _state = state; BuildUI(); }

        private void BuildUI()
        {
            Dock = DockStyle.Fill;
            var panel = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 5, Padding = new Padding(8), AutoSize = true };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 130));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            void AddRow(string label, Control ctrl, int row) { panel.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, row); panel.Controls.Add(ctrl, 1, row); }

            _txtId     = new TextBox { Dock = DockStyle.Fill };
            _txtNombre = new TextBox { Dock = DockStyle.Fill };
            _txtCorreo = new TextBox { Dock = DockStyle.Fill };
            _txtNivel  = new TextBox { Dock = DockStyle.Fill };

            AddRow("ID:",            _txtId,     0);
            AddRow("Nombre:",        _txtNombre, 1);
            AddRow("Correo:",        _txtCorreo, 2);
            AddRow("Nivel Acceso:",  _txtNivel,  3);

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            _btnAgregar   = new Button { Text = "Agregar",    Width = 90 };
            _btnActualizar = new Button { Text = "Actualizar", Width = 90 };
            _btnEliminar  = new Button { Text = "Eliminar",   Width = 90 };
            _btnLimpiar   = new Button { Text = "Limpiar",    Width = 90 };
            btnPanel.Controls.AddRange(new Control[] { _btnAgregar, _btnActualizar, _btnEliminar, _btnLimpiar });
            panel.Controls.Add(btnPanel, 1, 4);

            _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, AutoGenerateColumns = false };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Id",          HeaderText = "ID"          });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre",      HeaderText = "Nombre"      });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Correo",      HeaderText = "Correo"      });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "NivelAcceso", HeaderText = "Nivel Acceso"});
            _grid.DataSource = _state.Administradores;

            Controls.Add(_grid);
            Controls.Add(panel);

            _grid.SelectionChanged  += (_, _) => LoadSelected();
            _btnAgregar.Click       += async (_, _) => await OnAgregarAsync();
            _btnActualizar.Click    += async (_, _) => await OnActualizarAsync();
            _btnEliminar.Click      += async (_, _) => await OnEliminarAsync();
            _btnLimpiar.Click       += (_, _) => Limpiar();
        }

        private void LoadSelected()
        {
            if (_grid.CurrentRow?.DataBoundItem is not Usuario u) { _selected = null; return; }
            _selected = u;
            _txtId.Text     = u.Id.ToString();
            _txtNombre.Text = u.Nombre;
            _txtCorreo.Text = u.Correo;
            _txtNivel.Text  = u.NivelAcceso?.ToString() ?? "";
            _txtId.ReadOnly = true;
        }

        private void Limpiar() { _selected = null; _txtId.Text = _txtNombre.Text = _txtCorreo.Text = _txtNivel.Text = ""; _txtId.ReadOnly = false; _errors.Clear(); }

        private bool Validate(out int id)
        {
            id = 0;
            bool ok = true;
            if (!Validators.ValidatePositiveInt(_txtId.Text, "ID", _errors, _txtId, out id)) ok = false;
            if (!Validators.ValidateNotEmpty(_txtNombre.Text, "Nombre", _errors, _txtNombre)) ok = false;
            if (!Validators.ValidateEmail(_txtCorreo.Text, _errors, _txtCorreo)) ok = false;
            return ok;
        }

        private async Task OnAgregarAsync()
        {
            if (!Validate(out var id)) return;
            int.TryParse(_txtNivel.Text, out var nivel);
            var adm = new Usuario { Id = id, Nombre = _txtNombre.Text.Trim(), Correo = _txtCorreo.Text.Trim(), NivelAcceso = nivel, UserType = UserTypes.Administrador };
            await GridActions.RunDatabaseOperationAsync(() => _state.AddAdminAsync(adm), msg => MessageBox.Show(msg, "Error"));
            Limpiar();
        }

        private async Task OnActualizarAsync()
        {
            if (_selected is null || !Validate(out _)) return;
            int.TryParse(_txtNivel.Text, out var nivel);
            _selected.Nombre      = _txtNombre.Text.Trim();
            _selected.Correo      = _txtCorreo.Text.Trim();
            _selected.NivelAcceso = nivel;
            await GridActions.RunDatabaseOperationAsync(() => _state.UpdateAdminAsync(_selected), msg => MessageBox.Show(msg, "Error"));
        }

        private async Task OnEliminarAsync()
        {
            if (_selected is null) return;
            if (MessageBox.Show($"¿Eliminar al administrador {_selected.Nombre}?", "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await GridActions.RunDatabaseOperationAsync(() => _state.DeleteAdminAsync(_selected), msg => MessageBox.Show(msg, "Error"));
            Limpiar();
        }

        public void RefreshReferenceData() { }
    }
}
