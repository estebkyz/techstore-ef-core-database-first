using TiendaLinea.Models;
using TiendaLinea.UI;

namespace TiendaLinea.UI.Controls
{
    public class CategoriasControl : UserControl, IReferenceDataConsumer
    {
        private readonly AppState _state;
        private readonly ErrorProvider _errors = new();

        private DataGridView _grid = null!;
        private TextBox _txtCodigo = null!, _txtNombre = null!;
        private Button _btnAgregar = null!, _btnActualizar = null!, _btnEliminar = null!, _btnLimpiar = null!;
        private Categoria? _selected;

        public CategoriasControl(AppState state) { _state = state; BuildUI(); }

        private void BuildUI()
        {
            Dock = DockStyle.Fill;
            var panel = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 3, Padding = new Padding(8), AutoSize = true };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            void AddRow(string label, Control ctrl, int row) { panel.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, row); panel.Controls.Add(ctrl, 1, row); }

            _txtCodigo = new TextBox { Dock = DockStyle.Fill };
            _txtNombre = new TextBox { Dock = DockStyle.Fill };

            AddRow("Código:", _txtCodigo, 0);
            AddRow("Nombre:", _txtNombre, 1);

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            _btnAgregar   = new Button { Text = "Agregar",    Width = 90 };
            _btnActualizar = new Button { Text = "Actualizar", Width = 90 };
            _btnEliminar  = new Button { Text = "Eliminar",   Width = 90 };
            _btnLimpiar   = new Button { Text = "Limpiar",    Width = 90 };
            btnPanel.Controls.AddRange(new Control[] { _btnAgregar, _btnActualizar, _btnEliminar, _btnLimpiar });
            panel.Controls.Add(btnPanel, 1, 2);

            _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, AutoGenerateColumns = false };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", HeaderText = "Código" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre", HeaderText = "Nombre" });
            _grid.DataSource = _state.Categorias;

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
            if (_grid.CurrentRow?.DataBoundItem is not Categoria c) { _selected = null; return; }
            _selected = c;
            _txtCodigo.Text = c.Codigo;
            _txtNombre.Text = c.Nombre;
            _txtCodigo.ReadOnly = true;
        }

        private void Limpiar() { _selected = null; _txtCodigo.Text = _txtNombre.Text = ""; _txtCodigo.ReadOnly = false; _errors.Clear(); }

        private bool ValidateForm()
        {
            bool ok = true;
            if (!Validators.ValidateNotEmpty(_txtCodigo.Text, "Código", _errors, _txtCodigo)) ok = false;
            if (!Validators.ValidateNotEmpty(_txtNombre.Text, "Nombre", _errors, _txtNombre)) ok = false;
            return ok;
        }

        private async Task OnAgregarAsync()
        {
            if (!ValidateForm()) return;
            var cat = new Categoria { Codigo = _txtCodigo.Text.Trim(), Nombre = _txtNombre.Text.Trim() };
            await GridActions.RunDatabaseOperationAsync(() => _state.AddCategoriaAsync(cat), msg => MessageBox.Show(msg, "Error"));
            Limpiar();
        }

        private async Task OnActualizarAsync()
        {
            if (_selected is null || !ValidateForm()) return;
            _selected.Nombre = _txtNombre.Text.Trim();
            await GridActions.RunDatabaseOperationAsync(() => _state.UpdateCategoriaAsync(_selected), msg => MessageBox.Show(msg, "Error"));
        }

        private async Task OnEliminarAsync()
        {
            if (_selected is null) return;
            if (MessageBox.Show($"¿Eliminar la categoría {_selected.Nombre}?", "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await GridActions.RunDatabaseOperationAsync(() => _state.DeleteCategoriaAsync(_selected), msg => MessageBox.Show(msg, "Error"));
            Limpiar();
        }

        public void RefreshReferenceData() { }
    }
}
