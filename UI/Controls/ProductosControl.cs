using TiendaLinea.Models;
using TiendaLinea.UI;

namespace TiendaLinea.UI.Controls
{
    public class ProductosControl : UserControl, IReferenceDataConsumer
    {
        private readonly AppState _state;
        private readonly ErrorProvider _errors = new();

        private DataGridView _grid = null!;
        private TextBox _txtCodigo = null!, _txtNombre = null!, _txtDesc = null!, _txtPrecio = null!, _txtStock = null!;
        private ComboBox _cmbCategoria = null!;
        private Button _btnAgregar = null!, _btnActualizar = null!, _btnEliminar = null!, _btnLimpiar = null!;
        private Button _btnAgregarCat = null!;
        private Producto? _selected;

        public ProductosControl(AppState state) { _state = state; BuildUI(); }

        private void BuildUI()
        {
            Dock = DockStyle.Fill;
            var panel = new TableLayoutPanel { Dock = DockStyle.Top, ColumnCount = 2, RowCount = 8, Padding = new Padding(8), AutoSize = true };
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100));
            panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));

            void AddRow(string label, Control ctrl, int row) { panel.Controls.Add(new Label { Text = label, TextAlign = ContentAlignment.MiddleRight, Dock = DockStyle.Fill }, 0, row); panel.Controls.Add(ctrl, 1, row); }

            _txtCodigo = new TextBox { Dock = DockStyle.Fill };
            _txtNombre = new TextBox { Dock = DockStyle.Fill };
            _txtDesc   = new TextBox { Dock = DockStyle.Fill };
            _txtPrecio = new TextBox { Dock = DockStyle.Fill };
            _txtStock  = new TextBox { Dock = DockStyle.Fill };
            
            var catPanel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = new Padding(0) };
            catPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            catPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
            _cmbCategoria = new ComboBox { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
            _btnAgregarCat = new Button { Text = "Agregar Cat.", Dock = DockStyle.Fill };
            catPanel.Controls.Add(_cmbCategoria, 0, 0);
            catPanel.Controls.Add(_btnAgregarCat, 1, 0);

            AddRow("Código:",      _txtCodigo, 0);
            AddRow("Nombre:",      _txtNombre, 1);
            AddRow("Descripción:", _txtDesc,   2);
            AddRow("Precio:",      _txtPrecio, 3);
            AddRow("Stock:",       _txtStock,  4);
            AddRow("Categorías:",  catPanel,   5);

            var btnPanel = new FlowLayoutPanel { Dock = DockStyle.Fill };
            _btnAgregar   = new Button { Text = "Agregar",    Width = 90 };
            _btnActualizar = new Button { Text = "Actualizar", Width = 90 };
            _btnEliminar  = new Button { Text = "Eliminar",   Width = 90 };
            _btnLimpiar   = new Button { Text = "Limpiar",    Width = 90 };
            btnPanel.Controls.AddRange(new Control[] { _btnAgregar, _btnActualizar, _btnEliminar, _btnLimpiar });
            panel.Controls.Add(btnPanel, 1, 6);

            _grid = new DataGridView { Dock = DockStyle.Fill, ReadOnly = true, SelectionMode = DataGridViewSelectionMode.FullRowSelect, AllowUserToAddRows = false, AutoGenerateColumns = false };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Codigo", HeaderText = "Código" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Nombre", HeaderText = "Nombre" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Precio", HeaderText = "Precio" });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { DataPropertyName = "Stock",  HeaderText = "Stock" });
            
            // Columna para mostrar categorías
            var colCat = new DataGridViewTextBoxColumn { HeaderText = "Categorías", ReadOnly = true };
            _grid.Columns.Add(colCat);
            _grid.CellFormatting += (s, e) => {
                if (e.ColumnIndex == 4 && e.RowIndex >= 0 && _grid.Rows[e.RowIndex].DataBoundItem is Producto p)
                {
                    e.Value = string.Join(", ", p.CategoriaCodigos.Select(c => c.Nombre));
                }
            };
            
            _grid.DataSource = _state.Productos;

            Controls.Add(_grid);
            Controls.Add(panel);

            _grid.SelectionChanged  += (_, _) => LoadSelected();
            _btnAgregar.Click       += async (_, _) => await OnAgregarAsync();
            _btnActualizar.Click    += async (_, _) => await OnActualizarAsync();
            _btnEliminar.Click      += async (_, _) => await OnEliminarAsync();
            _btnLimpiar.Click       += (_, _) => Limpiar();
            _btnAgregarCat.Click    += async (_, _) => await OnAgregarCategoriaAsync();
        }

        public void RefreshReferenceData()
        {
            var oldVal = _cmbCategoria.SelectedValue;
            var list = _state.Categorias.Select(c => new ComboItem(c.Nombre, c.Codigo)).ToList();
            list.Insert(0, new ComboItem("-- Seleccione --", ""));
            _cmbCategoria.DataSource = list;
            _cmbCategoria.DisplayMember = "Display";
            _cmbCategoria.ValueMember = "Value";
            if (oldVal != null) _cmbCategoria.SelectedValue = oldVal;
        }

        private void LoadSelected()
        {
            if (_grid.CurrentRow?.DataBoundItem is not Producto p) { _selected = null; return; }
            _selected = p;
            _txtCodigo.Text = p.Codigo.ToString();
            _txtNombre.Text = p.Nombre;
            _txtDesc.Text   = p.Descripcion ?? "";
            _txtPrecio.Text = p.Precio.ToString("0.00");
            _txtStock.Text  = p.Stock.ToString();
            _txtCodigo.ReadOnly = true;
        }

        private void Limpiar() { _selected = null; _txtCodigo.Text = _txtNombre.Text = _txtDesc.Text = _txtPrecio.Text = _txtStock.Text = ""; _txtCodigo.ReadOnly = false; _errors.Clear(); }

        private bool Validate(out int codigo, out decimal precio, out int stock)
        {
            codigo = 0; precio = 0; stock = 0;
            bool ok = true;
            if (!Validators.ValidatePositiveInt(_txtCodigo.Text, "Código", _errors, _txtCodigo, out codigo)) ok = false;
            if (!Validators.ValidateNotEmpty(_txtNombre.Text, "Nombre", _errors, _txtNombre)) ok = false;
            if (!Validators.ValidatePositiveDecimal(_txtPrecio.Text, "Precio", _errors, _txtPrecio, out precio)) ok = false;
            if (!int.TryParse(_txtStock.Text, out stock) || stock < 0) { _errors.SetError(_txtStock, "Debe ser mayor o igual a cero"); ok = false; } else { _errors.SetError(_txtStock, ""); }
            return ok;
        }

        private async Task OnAgregarAsync()
        {
            if (!Validate(out var codigo, out var precio, out var stock)) return;
            var prod = new Producto { Codigo = codigo, Nombre = _txtNombre.Text.Trim(), Descripcion = _txtDesc.Text.Trim(), Precio = precio, Stock = stock };
            await GridActions.RunDatabaseOperationAsync(() => _state.AddProductoAsync(prod), msg => MessageBox.Show(msg, "Error"));
            Limpiar();
        }

        private async Task OnActualizarAsync()
        {
            if (_selected is null || !Validate(out _, out var precio, out var stock)) return;
            _selected.Nombre      = _txtNombre.Text.Trim();
            _selected.Descripcion = _txtDesc.Text.Trim();
            _selected.Precio      = precio;
            _selected.Stock       = stock;
            await GridActions.RunDatabaseOperationAsync(() => _state.UpdateProductoAsync(_selected), msg => MessageBox.Show(msg, "Error"));
            _grid.Invalidate(); // Refrescar categorías
        }

        private async Task OnEliminarAsync()
        {
            if (_selected is null) return;
            if (MessageBox.Show($"¿Eliminar producto {_selected.Nombre}?", "Confirmar", MessageBoxButtons.YesNo) != DialogResult.Yes) return;
            await GridActions.RunDatabaseOperationAsync(() => _state.DeleteProductoAsync(_selected), msg => MessageBox.Show(msg, "Error"));
            Limpiar();
        }
        
        private async Task OnAgregarCategoriaAsync()
        {
            if (_selected is null) { MessageBox.Show("Seleccione un producto primero."); return; }
            if (_cmbCategoria.SelectedValue is not string catId || string.IsNullOrEmpty(catId)) { MessageBox.Show("Seleccione una categoría."); return; }
            
            var cat = _state.Categorias.FirstOrDefault(c => c.Codigo == catId);
            if (cat == null) return;
            
            await GridActions.RunDatabaseOperationAsync(() => _state.AddCategoriaToProductoAsync(_selected, cat), msg => MessageBox.Show(msg, "Error"));
            _grid.Invalidate();
        }
    }
}
